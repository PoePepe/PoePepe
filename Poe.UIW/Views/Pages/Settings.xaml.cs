using System.Windows;
using Poe.UIW.Properties;
using Poe.UIW.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Poe.UIW.Views.Pages;

public partial class Settings : INavigableView<SettingsViewModel>
{
    public Settings(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;

        Loaded += OnLoaded;

        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.NotificationSoundPath = UserSettings.Default.NotificationSoundPath;
    }

    public SettingsViewModel ViewModel { get; }
}