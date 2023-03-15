using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SoundFlux.Views;
using System.Threading;

namespace SoundFlux
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            GlobalContext.OnExitEvent += () =>
            {
                var sect = SharedSettings.Instance.AddSection("Interface");
                sect.Add("Language", LanguageManager.Instance.CurrentThemeCode);
            };

            var sect = SharedSettings.Instance.GetSection("Interface");
            string? lang = sect?.Get("Language");

            if (string.IsNullOrEmpty(lang)) lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (string.IsNullOrEmpty(lang)) lang = "en";

            LanguageManager.Instance.CurrentThemeCode = lang;

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var view = new MainView();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    Content = view
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
                singleViewPlatform.MainView = view;

            base.OnFrameworkInitializationCompleted();
        }
    }
}
