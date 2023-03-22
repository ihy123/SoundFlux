using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace SoundFlux.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private KeyValuePair<string, string> language;
        public KeyValuePair<string, string> Language
        {
            get => language;
            set
            {
                if (SetProperty(ref language, value))
                    LanguageManager.Instance.CurrentThemeCode = language.Key;
            }
        }

        public SettingsViewModel()
        {
            Language = KeyValuePair.Create(LanguageManager.Instance.CurrentThemeCode,
                LanguageManager.SupportedLanguages[LanguageManager.Instance.CurrentThemeCode]);
        }
    }
}
