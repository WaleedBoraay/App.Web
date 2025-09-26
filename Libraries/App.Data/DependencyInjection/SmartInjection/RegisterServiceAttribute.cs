using System;

namespace App.Core.Infrastructure.DependencyInjection.Attributes
{
    /// <summary>
    /// Mark a concrete class for registration into Microsoft DI.
    /// Default behavior: register as implemented interfaces; if none -> as self.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RegisterServiceAttribute : Attribute
    {
        /// <summary>Explicit service contract to register as. If null -> implemented interfaces (or self if none).</summary>
        public Type? ServiceType { get; init; }

        /// <summary>Desired lifetime. Default: Scoped.</summary>
        public ServiceLifetime Lifetime { get; init; } = ServiceLifetime.Scoped;

        /// <summary>Also register the class as itself alongside interfaces.</summary>
        public bool AsSelf { get; init; } = false;

        /// <summary>Ignore interfaces and register only as self.</summary>
        public bool OnlySelf { get; init; } = false;
    }

    public enum ServiceLifetime
    {
        Singleton,
        Scoped,
        Transient
    }
}
