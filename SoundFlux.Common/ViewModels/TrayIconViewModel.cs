using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoundFlux.Services;
using SoundFlux.Views;
using System;

namespace SoundFlux.ViewModels
{
    internal partial class TrayIconViewModel : ObservableObject
    {
        public static bool CanMinimizeToTray
            => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime;

        public WindowState WindowStartupState { get; private set; }

        // handle minimizing window to tray
        public WindowState MainWindowState
        {
            set
            {
                mainWindowState = value;

                if (IsMinimizeToTrayEnabled && mainWindowState == WindowState.Minimized)
                    mainWindow.Hide();

                OnPropertyChanged(nameof(IsMinimizedToTray));
            }
        }
        private WindowState mainWindowState;

        [ObservableProperty]
        private bool isMinimizeToTrayEnabled;

        public bool IsMinimizedToTray
            => mainWindowState == WindowState.Minimized && IsMinimizeToTrayEnabled;

        private MainWindow mainWindow;

        [RelayCommand]
        private void TrayIconClicked()
        {
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Show();
        }

        [RelayCommand]
        private void TrayIconExit()
        {
            var lifetime = Application.Current?.ApplicationLifetime;
            if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
        }

        public TrayIconViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            // set minimize to tray
            IsMinimizeToTrayEnabled = ServiceRegistry.SettingsManager.Get(
                "Interface", "MinimizeToTray", false);

            // set window state
            WindowStartupState = (WindowState)ServiceRegistry.SettingsManager.Get(
                "Interface", "MainWindowState", (int)mainWindowState);

            // TODO: fix it, or not...
            if (IsMinimizeToTrayEnabled && WindowStartupState == WindowState.Minimized)
                mainWindow.Opened += HandleWindowStartupState;
        }

        public void SaveSettings()
        {
            ServiceRegistry.SettingsManager.Set("Interface",
                "MinimizeToTray", IsMinimizeToTrayEnabled);
            ServiceRegistry.SettingsManager.Set("Interface",
                "MainWindowState", (int)mainWindowState);
        }

        private void HandleWindowStartupState(object? s, EventArgs e)
        {
            mainWindow.Opened -= HandleWindowStartupState;
            mainWindow.WindowState = WindowStartupState;
        }
    }
}
