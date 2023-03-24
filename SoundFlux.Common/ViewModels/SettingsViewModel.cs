using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace SoundFlux.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        public static List<KeyValuePair<string, string>> SupportedLanguages
            => LanguageManager.SupportedLanguages.ToList();

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
