using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// Represents nopCommerce version
    /// </summary>
    public static class AppVersion
    {
        /// <summary>
        /// Gets the major store version
        /// </summary>
        public const string CURRENT_VERSION = "1.0";

        /// <summary>
        /// Gets the minor store version
        /// </summary>
        public const string MINOR_VERSION = "0";

        /// <summary>
        /// Gets the full store version
        /// </summary>
        public const string FULL_VERSION = CURRENT_VERSION + "." + MINOR_VERSION;
    }

}
