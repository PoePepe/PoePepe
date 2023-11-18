using Avalonia.Controls;
using Avalonia.Input;
using Poe.UI.Models;
using Poe.UI.Services;
using Poe.UI.ViewModels.OrderItemInfoViewModels;

namespace Poe.UI.Views.OrderItemInfoViews;

public partial class StackedItemInfoView : UserControl
{
    private OrderItemDto _orderItem;
    private  StackedItemInfo _itemInfo;
    public StackedItemInfoView()
    {
        InitializeComponent();
    }
    
    protected override void OnInitialized()
    {
        _orderItem = ((StackedItemInfoViewModel)DataContext!).OrderItem;
        _itemInfo = ((StackedItemInfo)_orderItem.ItemInfo);
        
        StackSizeTextBlock.Text = _itemInfo.StackSize;
        ItemLevelTextBlock.IsVisible = _itemInfo.ItemLevel > 0;

        RenderSockets();
        
        base.OnInitialized();
    }
    
    private void RenderSockets()
    {
        var itemSockets = _itemInfo.Sockets;
        
        if (itemSockets == null || itemSockets.Count == 0)
        {
            ItemSocketImageRelativePanel.IsVisible = false;
            return;
        }

        ItemImagePanel.Width = StackedItemImage.Source?.Size.Width ?? 100;
        ItemImagePanel.Height = StackedItemImage.Source?.Size.Height ?? 200;
        
        ItemSocketImageRelativePanel.Children.Clear();
        ItemSocketImageRelativePanel.Width = GetContainerWidth(itemSockets);
        ItemSocketImageRelativePanel.Height = GetContainerHeight(itemSockets);
        var socketImages = ItemSocketImageFactory.CreateSockets(itemSockets);
        ItemSocketImageRelativePanel.Children.AddRange(socketImages);
    }
    
    private double GetContainerWidth(Sockets itemSockets, int socketSize = 35, int socketMargin = 6)
    {
        if (itemSockets.Count == 1 || itemSockets.IsVertical)
        {
            return socketSize + socketMargin * 2;
        }

        return StackedItemImage.Source?.Size.Width ?? 100;
    }

    private double GetContainerHeight(Sockets itemSockets, int socketSize = 35, int socketMargin = 6)
    {
        var defaultHeight = socketSize + socketMargin * 2;
        
        if (itemSockets.IsVertical)
        {
            return defaultHeight * itemSockets.Count;
        }
        
        switch (itemSockets.Count)
        {
            case 1:
            case 2:
                return defaultHeight;
            case 3:
            case 4:
                return defaultHeight * 2;
            default:
                return defaultHeight;
        }
    }

    private void ItemImagePanel_OnPointerEntered(object sender, PointerEventArgs e)
    {
        ItemSocketImageRelativePanel.IsVisible = false;
    }

    private void ItemImagePanel_OnPointerExited(object sender, PointerEventArgs e)
    {
        ItemSocketImageRelativePanel.IsVisible = true;
    }
}