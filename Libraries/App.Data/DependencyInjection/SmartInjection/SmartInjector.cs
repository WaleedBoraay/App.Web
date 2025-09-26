using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using App.Core.Infrastructure.DependencyInjection.SmartInjection;
using App.Core.Infrastructure.DependencyInjection.SmartInjection.Attributes;

namespace App.Core.Infrastructure.DependencyInjection.SmartInjection
{
    /// <summary>
    /// Simple, robust attribute-driven registrar that wires into IServiceCollection.
    /// - Registers classes with [RegisterService].
    /// - Registers open generics with [RegisterOpenGeneric].
    /// - Registers factories with [RegisterFactory] (static factory methods).
    /// - Deterministic, no magic; clear error messages with type and assembly info.
    /// </summary>
    public static class SmartInjector
    {
        public static IServiceCollection AddAttributedServices(
            this IServiceCollection services,
            ILogger? logger,
            params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var types = assemblies
                .Where(a => a.IsDynamic == false && a.FullName != null && a.FullName.StartsWith("App", StringComparison.OrdinalIgnoreCase))
                .SelectMany(a =>
                {
                    try { return a.DefinedTypes; }
                    catch (ReflectionTypeLoadException rtle)
                    {
                        logger?.LogWarning(rtle, "SmartInjector: Skipping assembly {Asm} due to load error.", a.FullName);
                        return rtle.Types.Where(t => t != null).Select(t => t!.GetTypeInfo());
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "SmartInjector: Skipping assembly {Asm} due to error.", a.FullName);
                        return Array.Empty<TypeInfo>();
                    }
                })
                .ToArray();

            RegisterOpenGenerics(services, logger, types);
            RegisterFactories(services, logger, types);
            RegisterClosedTypes(services, logger, types);

            return services;
        }

        private static void RegisterClosedTypes(IServiceCollection services, ILogger? logger, IEnumerable<TypeInfo> types)
        {
            var candidates = types.Where(t => !t.IsAbstract && !t.IsInterface
                                              && t.GetCustomAttribute<DependencyInjection.Attributes.RegisterServiceAttribute>() != null);

            foreach (var ti in candidates)
            {
                var attr = ti.GetCustomAttribute<DependencyInjection.Attributes.RegisterServiceAttribute>()!;
                var lifetime = MapLifetime((Lifetime)attr.Lifetime);

                try
                {
                    var serviceTypes = ResolveServiceTypes(ti, attr);
                    if (!serviceTypes.Any())
                    {
                        serviceTypes = new[] { ti.AsType() }; // fallback to self
                    }

                    foreach (var service in serviceTypes)
                    {
                        Add(services, service, ti.AsType(), lifetime);
                        logger?.LogDebug("SmartInjector: Registered {Service} -> {Impl} ({Lifetime})",
                            service.FullName, ti.FullName, lifetime);
                    }

                    if (attr.AsSelf && serviceTypes.All(st => st != ti.AsType()))
                    {
                        Add(services, ti.AsType(), ti.AsType(), lifetime);
                        logger?.LogDebug("SmartInjector: Registered self {Impl} ({Lifetime})", ti.FullName, lifetime);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex,
                        "SmartInjector: Failed to register type {Type} from {Asm}.",
                        ti.FullName, ti.Assembly.GetName().Name);
                    throw;
                }
            }
        }

        private static void RegisterOpenGenerics(IServiceCollection services, ILogger? logger, IEnumerable<TypeInfo> types)
        {
            foreach (var ti in types)
            {
                var attrs = ti.GetCustomAttributes<RegisterOpenGenericAttribute>(inherit: false).ToArray();
                if (!attrs.Any()) continue;

                foreach (var a in attrs)
                {
                    try
                    {
                        ValidateOpenGeneric(a.ServiceOpenGeneric, nameof(a.ServiceOpenGeneric));
                        ValidateOpenGeneric(a.ImplementationOpenGeneric, nameof(a.ImplementationOpenGeneric));

                        var lifetime = MapLifetime(a.Lifetime);
                        AddOpenGeneric(services, a.ServiceOpenGeneric, a.ImplementationOpenGeneric, lifetime);

                        logger?.LogDebug("SmartInjector: Registered open generic {Svc} -> {Impl} ({Lifetime})",
                            a.ServiceOpenGeneric.FullName, a.ImplementationOpenGeneric.FullName, lifetime);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex,
                            "SmartInjector: Failed open-generic registration on {Type} in {Asm}.",
                            ti.FullName, ti.Assembly.GetName().Name);
                        throw;
                    }
                }
            }
        }

        private static void RegisterFactories(IServiceCollection services, ILogger? logger, IEnumerable<TypeInfo> types)
        {
            foreach (var ti in types)
            {
                var attrs = ti.GetCustomAttributes<RegisterFactoryAttribute>(inherit: false).ToArray();
                if (!attrs.Any()) continue;

                foreach (var a in attrs)
                {
                    try
                    {
                        var lifetime = MapLifetime(a.Lifetime);
                        var method = ti.GetMethod(a.FactoryMethod, BindingFlags.Public | BindingFlags.Static);
                        if (method == null)
                            throw new InvalidOperationException($"Factory method '{a.FactoryMethod}' not found on '{ti.FullName}'.");

                        // Must be (IServiceProvider) => object
                        var parameters = method.GetParameters();
                        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IServiceProvider))
                            throw new InvalidOperationException($"Factory method '{a.FactoryMethod}' on '{ti.FullName}' must be: (IServiceProvider) => object.");

                        // Register with factory
                        AddFactory(services, a.ServiceType, sp => method.Invoke(null, new object[] { sp })!, lifetime);

                        logger?.LogDebug("SmartInjector: Registered factory for {Service} via {Declaring}.{Method} ({Lifetime})",
                            a.ServiceType.FullName, ti.FullName, a.FactoryMethod, lifetime);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex,
                            "SmartInjector: Failed factory registration on {Type} in {Asm}.",
                            ti.FullName, ti.Assembly.GetName().Name);
                        throw;
                    }
                }
            }
        }

        private static ServiceLifetime MapLifetime(Lifetime lifetime) => lifetime switch
        {
            Lifetime.Singleton => ServiceLifetime.Singleton,
            Lifetime.Scoped => ServiceLifetime.Scoped,
            Lifetime.Transient => ServiceLifetime.Transient,
            _ => ServiceLifetime.Scoped
        };

        private static void ValidateOpenGeneric(Type t, string name)
        {
            if (!t.IsGenericTypeDefinition)
                throw new InvalidOperationException($"Type '{t}' provided to '{name}' must be an open generic (e.g., typeof(IRepo<>)).");
        }

        private static IEnumerable<Type> ResolveServiceTypes(TypeInfo implementation, DependencyInjection.Attributes.RegisterServiceAttribute attr)
        {
            if (attr.OnlySelf)
                return new[] { implementation.AsType() };

            if (attr.ServiceType != null)
                return new[] { attr.ServiceType };

            var ifaces = implementation.ImplementedInterfaces
                .Where(i => !i.IsGenericTypeDefinition)
                .ToArray();

            return ifaces;
        }

        private static void Add(IServiceCollection services, Type service, Type implementation, ServiceLifetime lifetime)
        {
            var descriptor = new ServiceDescriptor(service, implementation, lifetime);
            services.Add(descriptor);
        }

        private static void AddOpenGeneric(IServiceCollection services, Type serviceOpenGeneric, Type implOpenGeneric, ServiceLifetime lifetime)
        {
            var descriptor = new ServiceDescriptor(serviceOpenGeneric, implOpenGeneric, lifetime);
            services.Add(descriptor);
        }

        private static void AddFactory(IServiceCollection services, Type service, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            var descriptor = new ServiceDescriptor(service, factory, lifetime);
            services.Add(descriptor);
        }
    }
}
