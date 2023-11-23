using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Poe.LiveSearch.Services;
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

        // Loaded += OnLoaded;

        InitializeComponent();
    }

    // private void OnLoaded(object sender, RoutedEventArgs e)
    // {
    //     // LeagueNameComboBox.ItemsSource = _leagueService.ActualLeagueNames;
    // }

    public SettingsViewModel ViewModel { get; }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        UserSettings.Default.Save();
        UserSettings.Default.Reload();
        
        var serviceState = App.Current.Services.GetRequiredService<ServiceState>();
        serviceState.Session = UserSettings.Default.Session;
    }
}