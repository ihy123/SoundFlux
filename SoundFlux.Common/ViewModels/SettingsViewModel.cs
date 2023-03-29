using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using SoundFlux.Services;
using System.Collections.Generic;
using System.Threading;

namespace SoundFlux.ViewModels
{
    internal partial class SettingsViewModel : ObservableObject
    {
        private KeyValuePair<string, string> language;
        public KeyValuePair<string, string> Language
        {
            get => language;
            set
            {
                if (SetProperty(ref language, value))
                    LanguageManager.CurrentLangCode = language.Key;
            }
        }

        public TrayIconViewModel? TrayIconVM { get; private set; }

        public SettingsViewModel(TrayIconViewModel? trayIconVM)
        {
            TrayIconVM = trayIconVM;
            OnPropertyChanged(nameof(TrayIconVM));

            // set language
            string? lang = ServiceRegistry.SettingsManager.Get("Interface", "Language", null);

            if (string.IsNullOrEmpty(lang))
                lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (string.IsNullOrEmpty(lang))
                lang = "en";

            LanguageManager.CurrentLangCode = lang;

            Language = KeyValuePair.Create(LanguageManager.CurrentLangCode,
                LanguageManager.SupportedLanguages[LanguageManager.CurrentLangCode]);

            // set theme
            var app = Application.Current;
            if (app != null)
                app.RequestedThemeVariant = ThemeVariant.Light;
        }

        public void SaveSettings()
        {
            ServiceRegistry.SettingsManager.Set("Interface",
                "Language", LanguageManager.CurrentLangCode);
        }
    }
}
