using Avalonia.Controls;
using Avalonia.Input;
using Poe.UI.Models;
using Poe.UI.Services;
using Poe.UI.ViewModels.OrderItemInfoViewModels;

namespace Poe.UI.Views.OrderItemInfoViews;

public partial class OrderItemInfoView : UserControl
{
    public OrderItemInfoView()
    {
        InitializeComponent();
    }
    
    protected override void OnInitialized()
    {
        RenderSocketsAndLinks();
        
        base.OnInitialized();
    }

    private void RenderSocketsAndLinks()
    {
        var orderItem = ((OrderItemInfoViewModel)DataContext!).OrderItem;
        var itemSockets = ((ItemInfo)orderItem.ItemInfo).Sockets;
        
        if (itemSockets == null || itemSockets.Count == 0)
        {
            ItemSocketImageRelativePanel.IsVisible = false;
            ItemLinkImageCanvas.IsVisible = false;
            return;
        }

        ItemImagePanel.Width = ItemBackImage.Source?.Size.Width ?? 100;
        ItemImagePanel.Height = ItemBackImage.Source?.Size.Height ?? 200;
        
        ItemSocketImageRelativePanel.Children.Clear();
        ItemSocketImageRelativePanel.Width = GetContainerWidth(itemSockets);
        ItemSocketImageRelativePanel.Height = GetContainerHeight(itemSockets);
        var socketImages = ItemSocketImageFactory.CreateSockets(itemSockets);
        ItemSocketImageRelativePanel.Children.AddRange(socketImages);

        ItemLinkImageCanvas.Children.Clear();
        ItemLinkImageCanvas.Width = GetContainerWidth(itemSockets);
        ItemLinkImageCanvas.Height = GetContainerHeight(itemSockets);
        var linkImages = ItemSocketImageFactory.CreateLinks(itemSockets);
        ItemLinkImageCanvas.Children.AddRange(linkImages);
    }
    
    private double GetContainerWidth(Sockets itemSockets, int socketSize = 35, int socketMargin = 6)
    {
        if (itemSockets.Count == 1 || itemSockets.IsVertical)
        {
            return socketSize + socketMargin * 2;
        }

        return ItemBackImage.Source?.Size.Width ?? 100;
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
            case 5:
            case 6:
                return defaultHeight * 3;
            default:
                return defaultHeight;
        }
    }

    private void ItemImagePanel_OnPointerEntered(object sender, PointerEventArgs e)
    {
        ItemLinkImageCanvas.IsVisible = false;
        ItemSocketImageRelativePanel.IsVisible = false;
    }

    private void ItemImagePanel_OnPointerExited(object sender, PointerEventArgs e)
    {
        ItemLinkImageCanvas.IsVisible = true;
        ItemSocketImageRelativePanel.IsVisible = true;

    }
}