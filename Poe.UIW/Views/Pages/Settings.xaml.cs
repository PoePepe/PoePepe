using Poe.UIW.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Poe.UIW.Views.Pages;

public partial class Settings : INavigableView<SettingsViewModel>
{
    public Settings(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; }
}