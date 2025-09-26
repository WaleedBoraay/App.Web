using System.Text;
using App.Core.Configuration;
using App.Core.Infrastructure;
using App.Core.Singletons;
using App.Core.AppSettingsConfig;
using Newtonsoft.Json.Linq;

namespace App.Core;

/// <summary>
/// Represents the data settings manager
/// </summary>
public partial class DataSettingsManager
{
    #region Fields

    /// <summary>
    /// Gets a cached value indicating whether the database is installed. We need this value invariable during installation process
    /// </summary>
    protected static bool? _databaseIsInstalled;

    #endregion

    #region Utilities

    /// <summary>
    /// Gets data settings from the old txt file (Settings.txt)
    /// </summary>
    /// <param name="data">Old txt file data</param>
    /// <returns>Data settings</returns>
    protected static DataConfig LoadDataSettingsFromOldTxtFile(string data)
    {
        if (string.IsNullOrEmpty(data))
            return null;

        var dataSettings = new DataConfig();
        using var reader = new StringReader(data);
        string settingsLine;
        while ((settingsLine = reader.ReadLine()) != null)
        {
            var separatorIndex = settingsLine.IndexOf(':');
            if (separatorIndex == -1)
                continue;

            var key = settingsLine[0..separatorIndex].Trim();
            var value = settingsLine[(separatorIndex + 1)..].Trim();

            switch (key)
            {
                case "DataProvider":
                    dataSettings.DataProvider = Enum.TryParse(value, true, out DataProviderType providerType)
                        ? providerType
                        : DataProviderType.Unknown;
                    continue;
                case "DataConnectionString":
                    dataSettings.ConnectionString = value;
                    continue;
                case "SQLCommandTimeout":
                    dataSettings.SQLCommandTimeout = int.TryParse(value, out var timeout) ? timeout : -1;
                    continue;
                default:
                    break;
            }
        }

        return dataSettings;
    }

    /// <summary>
    /// Gets data settings from the old json file (dataSettings.json)
    /// </summary>
    /// <param name="data">Old json file data</param>
    /// <returns>Data settings</returns>
    protected static DataConfig LoadDataSettingsFromOldJsonFile(string data)
    {
        if (string.IsNullOrEmpty(data))
            return null;

        var root = JObject.Parse(data);
        var section = root["ConnectionStrings"] ?? root;

        var jsonDataSettings = section.ToObject<DataSettingsJson>();

        var dataSettings = new DataConfig
        {
            ConnectionString = jsonDataSettings?.ConnectionString,
            DataProvider = Enum.TryParse<DataProviderType>(jsonDataSettings?.DataProvider, true,
                out var providerType)
                ? providerType
                : DataProviderType.Unknown,
            SQLCommandTimeout = int.TryParse(jsonDataSettings?.SQLCommandTimeout, out var result)
                ? result
                : null
        };

        return dataSettings;
    }

    #endregion

    #region Methods

    protected static bool IsUncPath(string path)
    {
        return Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsUnc;
    }

    public virtual string Combine(params string[] paths)
    {
        var path = Path.Combine(paths
            .SelectMany(p => IsUncPath(p) ? new[] { p } : p.Split('\\', '/'))
            .ToArray());

        if (Environment.OSVersion.Platform == PlatformID.Unix && !IsUncPath(path))
            path = "/" + path;

        return path;
    }

    private string MapPath(string path)
    {
        path = path.Replace("~/", string.Empty).TrimStart('/');
        var pathEnd = path.EndsWith('/') ? Path.DirectorySeparatorChar.ToString() : string.Empty;
        return Combine(string.Empty, path) + pathEnd;
    }

    public static DataConfig LoadSettings(IAppFileProvider fileProvider = null, bool reload = false)
    {
        if (!reload && Singleton<DataConfig>.Instance is not null)
            return Singleton<DataConfig>.Instance;

        fileProvider ??= CommonHelper.DefaultFileProvider;

        // استخدم dataSettings.json
        var fullPath = Path.Combine(AppContext.BaseDirectory, "App_Data", "dataSettings.json");
        var manager = new DataSettingsManager();
        var filePath_json = manager.MapPath(fullPath);
        var filePath_txt = manager.MapPath(AppDataSettingsDefaults.ObsoleteFilePath);

        DataConfig dataSettings = null;

        if (fileProvider.FileExists(filePath_json))
        {
            var jsonContent = fileProvider.ReadAllText(filePath_json, Encoding.UTF8);
            dataSettings = LoadDataSettingsFromOldJsonFile(jsonContent);
        }
        else if (fileProvider.FileExists(filePath_txt))
        {
            var txtContent = fileProvider.ReadAllText(filePath_txt, Encoding.UTF8);
            dataSettings = LoadDataSettingsFromOldTxtFile(txtContent);
        }

        // fallback لو الملف مش موجود أو فاضي
        dataSettings ??= new DataConfig();

        // خزن في Singleton مرة واحدة
        Singleton<DataConfig>.Instance = dataSettings;

        return Singleton<DataConfig>.Instance;
    }


    public static void SaveSettings(DataConfig dataSettings, IAppFileProvider fileProvider)
    {
        AppSettingsConfig.AppSettingsHelper.SaveAppSettings(new List<IConfig> { dataSettings }, fileProvider);
        LoadSettings(fileProvider, reload: true);
    }

    public static bool IsDatabaseInstalled()
    {
        _databaseIsInstalled ??= !string.IsNullOrEmpty(LoadSettings()?.ConnectionString);
        return _databaseIsInstalled.Value;
    }

    public static int GetSqlCommandTimeout()
    {
        return LoadSettings()?.SQLCommandTimeout ?? -1;
    }

    public static bool UseNoLock()
    {
        var settings = LoadSettings();
        if (settings is null)
            return false;

        return settings.DataProvider == DataProviderType.SqlServer && settings.WithNoLock;
    }

    public void SaveSettings(DataConfig dataSettings)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Nested classes

    public class DataSettingsJson
    {
        public string ConnectionString { get; set; }
        public string DataProvider { get; set; }
        public string SQLCommandTimeout { get; set; }
    }

    #endregion
}
