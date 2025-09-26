using App.Core;
using App.Core.Infrastructure;
using App.Core.Singletons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Configuration
{
    /// <summary>
    /// Represents the data settings manager
    /// </summary>
    public partial class AppDataSettingsManager
    {
        private static bool _isDatabaseInstalled;

        #region Fields

        /// <summary>
        /// Gets a cached value indicating whether the database is installed. We need this value invariable during installation process
        /// </summary>
        private static readonly object _locker = new();
        private static bool? _databaseIsInstalled;

        /// <summary>
        /// المسار المنطقي لملف الإعدادات الداخلية
        /// </summary>
        private const string DataSettingsVirtualPath = "~/App_Data/appsettings.json";

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
                        dataSettings.DataProvider = Enum.TryParse(value, true, out DataProviderType providerType) ? providerType : DataProviderType.Unknown;
                        continue;
                    case "DataConnectionString":
                        dataSettings.ConnectionString = value;
                        continue;
                    case "SQLCommandTimeout":
                        //If parsing isn't successful, we set a negative timeout, that means the current provider will use a default value
                        dataSettings.SQLCommandTimeout = int.TryParse(value, out var timeout) ? timeout : -1;
                        continue;
                    default:
                        break;
                }
            }

            return dataSettings;
        }

        /// <summary>
        /// Gets data settings from the old json file (appsettings.json)
        /// </summary>
        /// <param name="data">Old json file data</param>
        /// <returns>Data settings</returns>
        protected static DataConfig LoadDataSettingsFromOldJsonFile(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            var jsonDataSettings = JsonConvert.DeserializeAnonymousType(data,
                new { DataConnectionString = "", DataProvider = DataProviderType.SqlServer, SQLCommandTimeout = "" });
            var dataSettings = new DataConfig
            {
                ConnectionString = jsonDataSettings.DataConnectionString,
                DataProvider = jsonDataSettings.DataProvider,
                SQLCommandTimeout = int.TryParse(jsonDataSettings.SQLCommandTimeout, out var result) ? result : null
            };

            return dataSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load data settings
        /// </summary>
        /// <param name="fileProvider">File provider</param>
        /// <param name="reload">Force loading settings from disk</param>
        /// <returns>Data settings</returns>
        public static DataConfig LoadSettings(IAppFileProvider fileProvider = null, bool reload = false)
        {

            fileProvider ??= CommonHelper.DefaultFileProvider;

            // المسار الأساسي (ContentRoot)
            var jsonPath = fileProvider.MapPath(DataSettingsVirtualPath);
            DataConfig settings = null;

            // 1) من خلال الـ provider
            if (fileProvider.FileExists(jsonPath))
            {
                var text = fileProvider.ReadAllText(jsonPath, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(text))
                    settings = JsonConvert.DeserializeObject<DataConfig>(text);
            }
            else
            {
                // 2) fallback إلى AppContext.BaseDirectory (لو الـ provider متضبط بدري بديناميكية غير صحيحة)
                var fallback = Path.Combine(AppContext.BaseDirectory, "App_Data", "appsettings.json");
                if (System.IO.File.Exists(fallback))
                {
                    var text = System.IO.File.ReadAllText(fallback, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(text))
                        settings = JsonConvert.DeserializeObject<DataConfig>(text);
                }
            }

            settings ??= new DataConfig();

            lock (_locker)
            {
                Singleton<DataConfig>.Instance = settings;
            }

            return settings;
        }


        /// <summary>
        /// Save data settings
        /// </summary>
        /// <param name="dataSettings">Data settings</param>
        /// <param name="fileProvider">File provider</param>
        public static void SaveSettings(DataConfig dataSettings, IAppFileProvider fileProvider)
        {
            if (dataSettings == null)
                throw new ArgumentNullException(nameof(dataSettings));

            if (fileProvider == null)
                throw new ArgumentNullException(nameof(fileProvider));

            // Resolve the file path
            var filePathJson = ResolvePath(fileProvider, AppDataSettingsDefaults.FilePath);

            // Ensure the directory exists
            var directory = Path.GetDirectoryName(filePathJson);
            if (!string.IsNullOrEmpty(directory))
                fileProvider.CreateDirectory(directory);

            // Serialize the settings to JSON and save to file
            var json = JsonConvert.SerializeObject(dataSettings, Formatting.Indented);
            fileProvider.WriteAllText(filePathJson, json, Encoding.UTF8);

            // Reload the settings into the singleton
            Singleton<DataConfig>.Instance = dataSettings;
        }

        /// <summary>
        /// Gets a value indicating whether database is already installed
        /// </summary>
        public static bool IsDatabaseInstalled()
        {
            _databaseIsInstalled ??= !string.IsNullOrEmpty(LoadSettings()?.ConnectionString);

            return _databaseIsInstalled.Value;
        }

        /// <summary>
        /// Marks the database as installed.
        /// </summary>
        public static void MarkDatabaseAsInstalled()
        {
            // Update installation status (e.g., write to a config file or database)
            _isDatabaseInstalled = true;

            // Example: Save to a configuration file
            var configFilePath = "App_Data/InstallationStatus.json";
            File.WriteAllText(configFilePath, "{ \"DatabaseInstalled\": true }");
        }

        /// <summary>
        /// Gets the command execution timeout.
        /// </summary>
        /// <value>
        /// Number of seconds. Negative timeout value means that a default timeout will be used. 0 timeout value corresponds to infinite timeout.
        /// </value>
        public static int GetSqlCommandTimeout()
        {
            return LoadSettings()?.SQLCommandTimeout ?? -1;
        }

        /// <summary>
        /// Gets a value that indicates whether to add NoLock hint to SELECT statements (Applies only to SQL Server, otherwise returns false)
        /// </summary>
        public static bool UseNoLock()
        {
            var settings = LoadSettings();

            if (settings is null)
                return false;

            return settings.DataProvider == DataProviderType.SqlServer && settings.WithNoLock;
        }

        #endregion

        // resolve absolute paths safely
        static string ResolvePath(IAppFileProvider prov, string rel)
            => prov != null ? prov.MapPath(rel)
                            : Path.Combine(AppContext.BaseDirectory, rel.Replace('/', Path.DirectorySeparatorChar));
    }

}
