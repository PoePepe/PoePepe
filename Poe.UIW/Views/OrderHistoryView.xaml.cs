using Poe.UIW.ViewModels;

namespace Poe.UIW.Views;

public partial class OrderHistoryView
{
    public OrderHistoryViewModel ViewModel { get; }

    public OrderHistoryView(OrderHistoryViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;

        InitializeComponent();
    }
}