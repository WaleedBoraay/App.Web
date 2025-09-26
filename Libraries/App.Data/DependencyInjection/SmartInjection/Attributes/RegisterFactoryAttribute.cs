using System;

namespace App.Core.Infrastructure.DependencyInjection.SmartInjection.Attributes
{
    /// <summary>
    /// Registers a service via a factory method.
    /// Use on a static type; specify ServiceType and FactoryMethod name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RegisterFactoryAttribute : Attribute
    {
        /// <summary>Service contract to register.</summary>
        public required Type ServiceType { get; init; }

        /// <summary>Name of a public static method: (IServiceProvider sp) => object.</summary>
        public required string FactoryMethod { get; init; }

        public Lifetime Lifetime { get; init; } = Lifetime.Singleton;
    }
}
