using System;

namespace App.Core.Infrastructure.DependencyInjection.SmartInjection.Attributes
{
    /// <summary>
    /// Registers an open generic mapping, e.g. IRepository<> -> EfRepository<>.
    /// Put this on any class (a marker) inside the assembly you scan.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RegisterOpenGenericAttribute : Attribute
    {
        public required Type ServiceOpenGeneric { get; init; }    // e.g. typeof(IRepository<>)
        public required Type ImplementationOpenGeneric { get; init; } // e.g. typeof(EfRepository<>)

        public Lifetime Lifetime { get; init; } = Lifetime.Scoped;
    }
}
