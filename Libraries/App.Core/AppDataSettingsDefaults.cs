using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// Represents default values related to data settings
    /// </summary>
    public static partial class AppDataSettingsDefaults
    {
        /// <summary>
        /// Gets a path to the file that was used in old nopCommerce versions to contain data settings
        /// </summary>
        public static string ObsoleteFilePath => "App_Data/appsettings.txt";

        /// <summary>
        /// Gets a path to the file that contains data settings
        /// </summary>
        public static string FilePath => "App_Data/appsettings.json";

        /// <summary>
        /// Validates the file path
        /// </summary>
        /// <param name="filePath">The file path to validate</param>
        /// <exception cref="InvalidOperationException">Thrown when the file path is null or empty</exception>
        public static void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new InvalidOperationException("File path cannot be null or empty.");
        }
    }
}
