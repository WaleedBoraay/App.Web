using System;
using System.Linq;
using System.Reflection;
using App.Core.Infrastructure.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace App.Core.Infrastructure.DependencyInjection.Extensions
{
    /// <summary>
    /// Scans given assemblies for [RegisterService] and adds descriptors to Microsoft DI.
    /// </summary>
    public static class AttributedRegistrationExtensions
    {
        public static IServiceCollection AddAttributedServices(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName != null && a.FullName.StartsWith("App", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }

            foreach (var asm in assemblies)
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException rtl)
                {
                    types = rtl.Types.Where(t => t != null).Cast<Type>().ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var impl in types.Where(t => t is { IsClass: true, IsAbstract: false }))
                {
                    var attr = impl.GetCustomAttributes(typeof(RegisterServiceAttribute), false)
                                   .Cast<RegisterServiceAttribute>()
                                   .FirstOrDefault();
                    if (attr == null) continue;

                    var lifetime = MapLifetime(attr.Lifetime);
                    var targets = ResolveServiceTypes(impl, attr);

                    if (targets.Length == 0)
                    {
                        // fallback to self
                        services.Add(new ServiceDescriptor(impl, impl, lifetime));
                    }
                    else
                    {
                        foreach (var svc in targets)
                            services.Add(new ServiceDescriptor(svc, impl, lifetime));
                    }

                    if (attr.AsSelf && targets.All(t => t != impl))
                        services.Add(new ServiceDescriptor(impl, impl, lifetime));
                }
            }

            return services;
        }

        private static Type[] ResolveServiceTypes(Type impl, RegisterServiceAttribute attr)
        {
            if (attr.OnlySelf) return new[] { impl };
            if (attr.ServiceType != null) return new[] { attr.ServiceType };

            var ifaces = impl.GetInterfaces();
            return ifaces.Length > 0 ? ifaces : Array.Empty<Type>();
        }

        private static Microsoft.Extensions.DependencyInjection.ServiceLifetime MapLifetime(Attributes.ServiceLifetime lt) =>
            lt switch
            {
                Attributes.ServiceLifetime.Singleton => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton,
                Attributes.ServiceLifetime.Scoped => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped,
                Attributes.ServiceLifetime.Transient => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient,
                _ => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped
            };
    }
}
