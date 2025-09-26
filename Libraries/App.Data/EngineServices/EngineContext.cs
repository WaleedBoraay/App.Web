using App.Core.EngineServices;
using App.Core.Singletons;
using App.Data.Startup;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace App.Data.EngineServices;

/// <summary>
/// Provides access to the singleton instance of the Nop engine.
/// </summary>
public partial class EngineContext
{
    #region Methods

    /// <summary>
    /// Create a static instance of the Nop engine.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static IEngine Create(IServiceProvider serviceProvider = null)
    {
        if (Singleton<IEngine>.Instance != null)
            return Singleton<IEngine>.Instance;

        if (serviceProvider == null)
        {
            // إنشاء ServiceCollection جديد وتسجيل AppDbStartup
            var services = new ServiceCollection();
            var startup = new AppDbStartup();
            startup.ConfigureServices(services, null);
            serviceProvider = services.BuildServiceProvider();
        }

        // إنشاء Engine مربوط بالـ ServiceProvider
        Singleton<IEngine>.Instance = new AppEngine(serviceProvider);

        return Singleton<IEngine>.Instance;
    }

    /// <summary>
    /// Sets the static engine instance to the supplied engine. Use this method to supply your own engine implementation.
    /// </summary>
    /// <param name="engine">The engine to use.</param>
    /// <remarks>Only use this method if you know what you're doing.</remarks>
    public static void Replace(IEngine engine)
    {
        Singleton<IEngine>.Instance = engine;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the singleton Nop engine used to access Nop services.
    /// </summary>
    public static IEngine Current
    {
        get
        {
            if (Singleton<IEngine>.Instance == null)
            {
                Create();
            }

            return Singleton<IEngine>.Instance;
        }
    }

    #endregion
}