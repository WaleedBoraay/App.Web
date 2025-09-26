using App.Core.Configuration;

namespace App.Core.AppSettingsConfig;

/// <summary>
/// Represents the event that is raised when App Settings are saving
/// </summary>
public partial class AppSettingsSavingEvent
{
    #region Ctor

    public AppSettingsSavingEvent(IList<IConfig> configurations)
    {
        if (configurations == null || configurations.Any(config => config == null || config.Name == null))
            throw new ArgumentException("Configurations contain null or invalid entries.", nameof(configurations));

        Configurations = configurations;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add configuration to save
    /// </summary>
    /// <param name="config">Configuration to save</param>
    public void AddConfig<TConfig>(TConfig config) where TConfig : class, IConfig
    {
        if (Configurations.OfType<TConfig>().FirstOrDefault() is TConfig currentConfig)
            Configurations[Configurations.IndexOf(currentConfig)] = config;
        else
            Configurations.Add(config);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets configurations to save
    /// </summary>
    public IList<IConfig> Configurations { get; protected set; }

    #endregion
}