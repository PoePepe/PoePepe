using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PoePepe.UI.Models;
using PoePepe.UI.Properties;
using PoePepe.UI.ViewModels;
using Wpf.Ui.Common.Interfaces;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace PoePepe.UI.Views.Pages;

public partial class Settings : INavigableView<SettingsViewModel>
{
    private Brush _accentBorderBrush;
    private Brush _contentBorderBrush;

    private ValidatableTextBox _validatablePoeSessIdTextBox;

    public Settings(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;

        Loaded += OnLoaded;

        InitializeComponent();
        DataObject.AddPastingHandler(PoeSessionIdTextBox, OnPaste);
    }

    public SettingsViewModel ViewModel { get; }

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
        ViewModel.CurrentSound =
            ViewModel.DefaultSoundNames.First(x => x.Path == UserSettings.Default.NotificationSoundPath);

        ViewModel.ErrorsChanged += ViewModelOnErrorsChanged;
        ViewModel.ValidationErrorsChanged += ViewModelOnErrorsChanged;
        _validatablePoeSessIdTextBox = new ValidatableTextBox(PoeSessionIdTextBox, PoeSessIdErrorStackPanel);
    }

    private void ViewModelOnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
    {
        if (!ViewModel.HasErrors && !ViewModel.HasValidationErrors)
        {
            HideError(_validatablePoeSessIdTextBox);
            return;
        }

        var error = ViewModel.GetValidationErrors(e.PropertyName).FirstOrDefault();

        switch (e.PropertyName)
        {
            case "PoeSessionId" when error is null:
                HideError(_validatablePoeSessIdTextBox);
                break;

            case "PoeSessionId":
                ShowError(_validatablePoeSessIdTextBox, error);
                break;

            default:
                return;
        }
    }

    private void ShowError(ValidatableTextBox validatableTextBox, ValidationResult error)
    {
        TextBlock errorTextBlock;
        if (validatableTextBox.HasError)
        {
            errorTextBlock = (TextBlock)validatableTextBox.ErrorPanel.Children[1];
            if (errorTextBlock.Text == error.ErrorMessage)
            {
                return;
            }
        }

        var textBoxTemplate = validatableTextBox.TextBox.Template;
        var accentBorder = (Border)textBoxTemplate.FindName("AccentBorder", validatableTextBox.TextBox);
        var contentBorder = (Border)textBoxTemplate.FindName("ContentBorder", validatableTextBox.TextBox);
        _accentBorderBrush ??= accentBorder.BorderBrush;
        _contentBorderBrush ??= contentBorder.BorderBrush;

        accentBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d50000"));
        contentBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d50000"));

        validatableTextBox.ErrorPanel.Visibility = Visibility.Visible;

        errorTextBlock = (TextBlock)validatableTextBox.ErrorPanel.Children[1];
        errorTextBlock.Text = error.ErrorMessage;
    }

    private void HideError(ValidatableTextBox validatableTextBox)
    {
        if (!validatableTextBox.HasError)
        {
            return;
        }

        var textBoxTemplate = validatableTextBox.TextBox.Template;
        var accentBorder = (Border)textBoxTemplate.FindName("AccentBorder", validatableTextBox.TextBox);
        var contentBorder = (Border)textBoxTemplate.FindName("ContentBorder", validatableTextBox.TextBox);

        accentBorder.BorderBrush = _accentBorderBrush ?? accentBorder.BorderBrush;
        contentBorder.BorderBrush = _contentBorderBrush ?? contentBorder.BorderBrush;

        validatableTextBox.ErrorPanel.Visibility = Visibility.Hidden;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox cmb)
        {
            return;
        }

        if (!cmb.IsDropDownOpen)
        {
            return;
        }

        var selected = (Sound)e.AddedItems[0];
        if (selected.Name.StartsWith("Custom"))
        {
            ViewModel.OpenSoundFile();
        }

        ViewModel.TestSound();
    }

    private void SoundElement_OnSelected(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBlock textBlock)
        {
            return;
        }

        if (textBlock.Text.StartsWith("Custom"))
        {
            ViewModel.OpenSoundFile();
        }
    }

    private void PoeSessionIdTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.RemoveValidationError("PoeSessionId");
    }

    private class ValidatableTextBox
    {
        public ValidatableTextBox(TextBox textBox, StackPanel errorPanel)
        {
            TextBox = textBox;
            ErrorPanel = errorPanel;
        }

        public bool HasError => ErrorPanel.Visibility == Visibility.Visible;
        public TextBox TextBox { get; }
        public StackPanel ErrorPanel { get; }
    }
}