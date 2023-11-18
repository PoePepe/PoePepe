using Avalonia.Controls;
using Avalonia.Input;
using Poe.UI.Models;
using Poe.UI.Services;
using Poe.UI.ViewModels.OrderItemInfoViewModels;

namespace Poe.UI.Views.OrderItemInfoViews;

public partial class DivinationCardInfoView : UserControl
{
    public DivinationCardInfoView()
    {
        InitializeComponent();
    }
    
    protected override void OnInitialized()
    {
        var orderItem = ((DivinationCardInfoViewModel)DataContext!).OrderItem;

        StackSizeTextBlock.Text = ((DivinationCardInfo)orderItem.ItemInfo).StackSize;
        
        base.OnInitialized();
    }
    
}