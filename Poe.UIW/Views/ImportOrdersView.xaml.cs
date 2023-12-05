using System.Windows;
using System.Windows.Controls;
using Poe.UIW.ViewModels;

namespace Poe.UIW.Views;

public partial class ImportOrdersView
{
    private ImportOrdersViewModel _viewModel;

    public ImportOrdersView()
    {
        Loaded += OnLoaded;
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = (ImportOrdersViewModel)DataContext;
    }

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.ClearCommonValidationError();
    }
}