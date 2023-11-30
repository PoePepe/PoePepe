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
        DataObject.AddPastingHandler(PoeSessionIdTextBox, OnPaste);
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
        if (!isText)
        {
            return;
        }

        if (e.SourceDataObject.GetData(DataFormats.UnicodeText) is not string text)
        {
            return;
        }

        ViewModel.PoeSessionId = "";

        if (text.StartsWith("POESESSID="))
        {
            ViewModel.PoeSessionId = text[10..];
        }
        else if (text.Length == 32)
        {
            ViewModel.PoeSessionId = text;
        }
        else
        {
            ViewModel.PoeSessionId = text;
        }

        e.CancelCommand();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.NotificationSoundPath = UserSettings.Default.NotificationSoundPath;
    }

    public SettingsViewModel ViewModel { get; }
}