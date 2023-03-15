using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;

namespace SoundFlux.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        public struct LangTuple
        {
            public string code, name;

            public string Name => name;
        }

        [ObservableProperty]
        private LangTuple[] languages = Array.Empty<LangTuple>();

        private LangTuple language;
        public LangTuple Language
        {
            get => language;
            set
            {
                if (SetProperty(ref language, value))
                    LanguageManager.Instance.CurrentThemeCode = language.code;
            }
        }

        public SettingsViewModel()
        {
            Languages = LanguageManager.SupportedLanguages
                .Select(e =>
                {
                    LangTuple lt = new() { code = e.Key, name = e.Value };
                    if (lt.code == LanguageManager.Instance.CurrentThemeCode)
                        Language = lt;
                    return lt;
                }).ToArray();
        }
    }
}
