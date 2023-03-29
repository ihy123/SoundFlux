using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SoundFlux.ViewModels;
using System;

namespace SoundFlux.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();

            MainTabControl.ItemsPanel = new FuncTemplate<Panel>(() => new Grid()
            {
                ColumnDefinitions = new ColumnDefinitions("Auto * *")
            });
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            var dc = (MainViewModel?)DataContext;
            if (dc == null)
                throw new Exception("MainViewModel's DataContext is not set");

            serverView.DataContext = dc.ServerVM;
            clientView.DataContext = dc.ClientVM;
            settingsView.DataContext = dc.SettingsVM;
        }
    }
}
