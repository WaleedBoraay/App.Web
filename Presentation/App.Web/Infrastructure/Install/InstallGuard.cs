using System;
using System.Reflection;

namespace App.Web.Infrastructure.Install
{
    /// <summary>
    /// Reflection-based wrapper to detect installation status without hard coupling
    /// to a specific manager class name.
    /// </summary>
    public static class InstallGuard
    {
        private static bool? _cached;
        public static void ResetCache() => _cached = null;


        public static bool IsInstalled()
        {
            if (_cached.HasValue) return _cached.Value;

            // Try known candidates
            var candidates = new[]
            {
                "App.Core.Data.DataSettingsManager, App.Core",
                "App.Core.DataSettingsManager, App.Core",
                "App.Data.MigratorServices.AppDataSettingsManager, App.Data"
            };

            foreach (var typeName in candidates)
            {
                try
                {
                    var t = Type.GetType(typeName, throwOnError: false);
                    if (t == null) continue;
                    var m = t.GetMethod("IsDatabaseInstalled", BindingFlags.Public | BindingFlags.Static);
                    if (m == null) continue;
                    var result = m.Invoke(null, null);
                    if (result is bool b)
                    {
                        _cached = b;
                        return b;
                    }
                }
                catch { /* ignore and continue */ }
            }

            // Default: not installed
            _cached = false;
            return false;
        }
    }
}
