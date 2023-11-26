using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Poe.UIW.ViewModels;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Poe.UIW.Views;

public partial class ManageOrderView
{
    private ManageOrderViewModel _viewModel;

    public ManageOrderView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void ManageOrderModelOnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
    {
        if (!_viewModel.ManageOrderModel.HasErrors)
        {
            HideError(_validatableNameTextBox);
            HideError(_validatableLinkTextBox);
            return;
        }

        var error = _viewModel.ManageOrderModel.GetErrors(e.PropertyName).FirstOrDefault();

        switch (e.PropertyName)
        {
            case "Link" when error is null:
                HideError(_validatableLinkTextBox);
                break;

            case "Link":
                ShowError(_validatableLinkTextBox, error);
                break;

            case "Name" when error is null:
                HideError(_validatableNameTextBox);
                break;

            case "Name":
                ShowError(_validatableNameTextBox, error);
                break;

            default:
                return;
        }
    }

    private ValidatableTextBox _validatableNameTextBox;
    private ValidatableTextBox _validatableLinkTextBox;

    private void ShowError(ValidatableTextBox validatableTextBox, ValidationResult error)
    {
        TextBlock errorTextBlock;
        if (validatableTextBox.HasError)
        {
            errorTextBlock = (TextBlock) validatableTextBox.ErrorPanel.Children[1];
            if (errorTextBlock.Text == error.ErrorMessage)
            {
                return;
            }
        }

        var textBoxTemplate = validatableTextBox.TextBox.Template;
        var accentBorder = (Border) textBoxTemplate.FindName("AccentBorder", validatableTextBox.TextBox);
        var contentBorder = (Border) textBoxTemplate.FindName("ContentBorder", validatableTextBox.TextBox);
        _accentBorderBrush ??= accentBorder.BorderBrush;
        _contentBorderBrush ??= contentBorder.BorderBrush;

        accentBorder.BorderBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#d50000"));
        contentBorder.BorderBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#d50000"));

        validatableTextBox.ErrorPanel.Visibility = Visibility.Visible;

        errorTextBlock = (TextBlock) validatableTextBox.ErrorPanel.Children[1];
        errorTextBlock.Text = error.ErrorMessage;
    }

    private Brush _accentBorderBrush;
    private Brush _contentBorderBrush;

    private void HideError(ValidatableTextBox validatableTextBox)
    {
        if (!validatableTextBox.HasError)
        {
            return;
        }

        var textBoxTemplate = validatableTextBox.TextBox.Template;
        var accentBorder = (Border) textBoxTemplate.FindName("AccentBorder", validatableTextBox.TextBox);
        var contentBorder = (Border) textBoxTemplate.FindName("ContentBorder", validatableTextBox.TextBox);

        accentBorder.BorderBrush = _accentBorderBrush ?? accentBorder.BorderBrush;
        contentBorder.BorderBrush = _contentBorderBrush ?? contentBorder.BorderBrush;

        validatableTextBox.ErrorPanel.Visibility = Visibility.Hidden;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = (ManageOrderViewModel) DataContext;

        if (_viewModel.IsEditing)
        {
            TitleTextBlock.Text = "Edit order";
        }

        _viewModel.ManageOrderModel.ErrorsChanged += ManageOrderModelOnErrorsChanged;
        _validatableNameTextBox = new ValidatableTextBox(NameTextBox, NameErrorStackPanel);
        _validatableLinkTextBox = new ValidatableTextBox(LinkTextBox, LinkErrorStackPanel);
    }

    private class ValidatableTextBox
    {
        public ValidatableTextBox(TextBox textBox, StackPanel errorPanel)
        {
            TextBox = textBox;
            ErrorPanel = errorPanel;
        }

        public bool HasError => ErrorPanel.Visibility == Visibility.Visible;
        public TextBox TextBox { get; set; }
        public StackPanel ErrorPanel { get; set; }
    }
}