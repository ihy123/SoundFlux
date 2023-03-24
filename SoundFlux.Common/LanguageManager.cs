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

        private string currentThemeCode = "";
        public string CurrentThemeCode
        {
            get => currentThemeCode;
            set
            {
                if (currentThemeCode != value && SupportedLanguages.ContainsKey(value))
                {
                    currentThemeCode = value;

                    var d = App.Current!.Resources.MergedDictionaries;
                    if (currentResourceInclude != null) d.Remove(currentResourceInclude);

                    currentResourceInclude = new ResourceInclude((System.Uri?)null)
                    {
                        Source = new System.Uri($"avares://SoundFlux.Common/Assets/Languages/{currentThemeCode}.axaml")
                    };
                    d.Add(currentResourceInclude);

                    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(currentThemeCode);
                }
            }
        }

        private ResourceInclude? currentResourceInclude = null;
    }
}
