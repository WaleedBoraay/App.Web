using System;

namespace App.Core.Infrastructure.DependencyInjection.SmartInjection
{
    /// <summary>
    /// Lifetime for smart injection registrations.
    /// </summary>
    public enum Lifetime
    {
        Singleton,
        Scoped,
        Transient
    }
}
