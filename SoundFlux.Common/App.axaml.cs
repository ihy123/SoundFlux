using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using SoundFlux.Services;
using SoundFlux.Views;
using System.Threading;

namespace SoundFlux
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            //GlobalEvents.OnExitEvent += () =>
            //{
            //    var sect = SharedSettings.Instance.AddSection("Interface");
            //    sect.Add("Language", LanguageManager.Instance.CurrentLangCode);
            //};

            string? lang = ServiceRegistry.SettingsManager.Get("Interface", "Language", null);

            if (string.IsNullOrEmpty(lang))
                lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (string.IsNullOrEmpty(lang))
                lang = "en";

            LanguageManager.CurrentLangCode = lang;

            RequestedThemeVariant = ThemeVariant.Light;

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
