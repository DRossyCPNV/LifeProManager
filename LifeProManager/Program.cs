/// <file>Program.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7</version>
/// <date>February 22th, 2026</date>

using System;
using System.Globalization;
using System.Windows.Forms;

namespace LifeProManager
{
    static class Program
    {
        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Applies the UI language before any form is created.
            ApplyLocalization();

            Application.Run(new frmMain());
        }

        /// <summary>
        /// Applies the UI language based on application settings.
        /// On first launch, detects OS language and stores it.
        /// </summary>
        private static void ApplyLocalization()
        {
            // Reads stored language code from application settings.
            string storedLanguageCode = Properties.Settings.Default.appLanguageCode;

            // First launch: no language stored yet.
            if (string.IsNullOrWhiteSpace(storedLanguageCode))
            {
                // Detects OS language.
                string osLang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;

                if (osLang == "fr")
                {
                    storedLanguageCode = "fr";
                }
                else
                {
                    storedLanguageCode = "en";
                }

                // Saves detected language.
                Properties.Settings.Default.appLanguageCode = storedLanguageCode;
                Properties.Settings.Default.Save();
            }

            // Applies the language to the localization manager.
            LocalizationManager.SetLanguage(storedLanguageCode);
        }
    }
}

