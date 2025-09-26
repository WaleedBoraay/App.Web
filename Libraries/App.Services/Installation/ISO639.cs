using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using App.Core.Domain.Localization;

namespace App.Services.Installation
{
    /// <summary>
    /// Provides ISO639 languages loader from XML installation files.
    /// </summary>
    public static class ISO639
    {
        private static readonly string LocalizationPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"App_Data\Localization\Installation\");

        /// <summary>
        /// Reads available languages from Installation XML files.
        /// </summary>
        public static IEnumerable<Language> GetCollection()
        {
            var languages = new List<Language>();
            if (!System.IO.Directory.Exists(LocalizationPath))
                return languages;

            int order = 1;
            foreach (var file in System.IO.Directory.GetFiles(LocalizationPath, "installation.*.xml"))
            {
                var culture = Path.GetFileNameWithoutExtension(file)
                                  .Replace("installation.", "");

                // اقرأ الـ XML
                var doc = XDocument.Load(file);
                var root = doc.Root;
                var nameAttr = root?.Attribute("Name")?.Value;
                var rtlAttr = root?.Attribute("IsRightToLeft")?.Value;

                bool rtl = false;
                if (!string.IsNullOrEmpty(rtlAttr) && bool.TryParse(rtlAttr, out var parsed))
                    rtl = parsed;
                else
                    rtl = culture.StartsWith("ar") || culture.StartsWith("he") || culture.StartsWith("fa") || culture.StartsWith("ur");

                var lang = new Language
                {
                    Name = string.IsNullOrWhiteSpace(nameAttr) ? culture : nameAttr,
                    LanguageCulture = culture,
                    UniqueSeoCode = culture.Split('-')[0],
                    FlagImageFileName = $"{culture.Split('-')[0]}.png",
                    Rtl = rtl,
                    Published = (culture == "en-US" || culture == "ar-SA"),
                    DisplayOrder = order++
                };
                languages.Add(lang);
            }

            return languages;
        }
    }
}
