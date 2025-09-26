using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using App.Core.Infrastructure;
using App.Core.AppSettingsConfig;
using App.Core.Singletons;
using App.Core.Configuration;

namespace App.Data.Configuration;

/// <summary>
/// Represents the app settings helper
/// </summary>
public partial class AppSettingsHelper
{
    #region Fields

    protected static Dictionary<string, int> _configurationOrder;

    #endregion

    #region Methods

    /// <summary>
    /// Create app settings with the passed configurations and save it to the file
    /// </summary>
    /// <param name="configurations">Configurations to save</param>
    /// <param name="fileProvider">File provider</param>
    /// <param name="overwrite">Whether to overwrite appsettings file</param>
    /// <returns>App settings</returns>
    public static async Task<AppSettings> SaveAppSettings(IList<IConfig> configurations, IAppFileProvider fileProvider, bool overwrite = true)
    {
        // Validate inputs
        if (configurations == null || configurations.Any(config => config == null || config.Name == null))
            throw new ArgumentException("Configurations contain null or invalid entries.", nameof(configurations));

        if (fileProvider == null)
            throw new ArgumentNullException(nameof(fileProvider), "File provider cannot be null.");

        // Initialize configuration order
        _configurationOrder = configurations.ToDictionary(config => config.Name, config => config.GetOrder());

        // Create app settings
        var appSettings = Singleton<AppSettings>.Instance ?? new AppSettings();
        appSettings.Update(configurations);
        Singleton<AppSettings>.Instance = appSettings;

        // Create file if not exists
        var filePath = fileProvider.MapPath(AppConfigurationDefaults.AppSettingsFilePath);
        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidOperationException("File path cannot be null or empty.");

        var fileExists = fileProvider.FileExists(filePath);
        fileProvider.CreateFile(filePath);

        // Get raw configuration parameters
        var fileContent = fileProvider.ReadAllText(filePath, Encoding.UTF8);
        if (string.IsNullOrWhiteSpace(fileContent))
            fileContent = "{}"; // Default to an empty JSON object

        var configuration = JsonConvert.DeserializeObject<AppSettings>(fileContent)?.Configuration ?? new();
        foreach (var config in configurations)
        {
            configuration[config.Name] = JToken.FromObject(config);
        }

        // Sort configurations for display by order
        appSettings.Configuration = configuration
            .SelectMany(outConfig => _configurationOrder.Where(inConfig => inConfig.Key == outConfig.Key).DefaultIfEmpty(),
                (outConfig, inConfig) => new { OutConfig = outConfig, InConfig = inConfig })
            .OrderBy(config => config.InConfig.Value)
            .Select(config => config.OutConfig)
            .ToDictionary(config => config.Key, config => config.Value);

        // Save app settings to the file
        if (!fileExists || overwrite)
        {
            var text = JsonConvert.SerializeObject(appSettings, Formatting.Indented);
            fileProvider.WriteAllText(filePath, text, Encoding.UTF8);
        }

        return appSettings;
    }

    #endregion
}