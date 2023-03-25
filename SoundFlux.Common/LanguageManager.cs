using Avalonia.Markup.Xaml.Styling;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace SoundFlux
{
    public class LanguageManager
    {
        public static readonly LanguageManager Instance = new();

        public static readonly Dictionary<string, string> SupportedLanguages = new()
        {
            { "en", "English" },
            { "ru", "Русский" }
        };

        private string currentLangCode = "";
        public string CurrentLangCode
        {
            get => currentLangCode;
            set
            {
                if (currentLangCode != value && SupportedLanguages.ContainsKey(value))
                {
                    currentLangCode = value;

                    var d = App.Current!.Resources.MergedDictionaries;
                    if (currentResourceInclude != null) d.Remove(currentResourceInclude);

                    currentResourceInclude = new ResourceInclude((System.Uri?)null)
                    {
                        Source = new System.Uri($"avares://SoundFlux.Common/Assets/Languages/{currentLangCode}.axaml")
                    };
                    d.Add(currentResourceInclude);

                    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(currentLangCode);
                }
            }
        }

        private ResourceInclude? currentResourceInclude = null;
    }
}
