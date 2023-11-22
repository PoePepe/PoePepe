using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Poe.UIW.Helpers;
using Poe.UIW.Models;
using Poe.UIW.Services;
using Poe.UIW.Services.Currency;
using Poe.UIW.Services.Separator;
using Poe.UIW.ViewModels.OrderItemInfoViewModels;

namespace Poe.UIW.Views.OrderItemInfoViews;

public partial class OrderItemInfoView : Window
{
    public OrderItemInfoViewModel ViewModel { get; }
    private BitmapImage _bitmapImage;
    public event EventHandler Whispered;

    public OrderItemInfoView()
    {
        
    }
    
    public OrderItemInfoView(OrderItemInfoViewModel orderItemInfoViewModel)
    {
        DataContext = orderItemInfoViewModel;
        ViewModel = orderItemInfoViewModel;
        
        _bitmapImage = new BitmapImage();
        Loaded += OnLoaded;
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RenderSeparators();
        RenderPrice();
    }

    protected override void OnInitialized(EventArgs eventArgs)
    {
        ThreadPool.QueueUserWorkItem(async _ =>
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                _bitmapImage = await ViewModel.LoadItemImageAsync(_bitmapImage);
                RenderSocketsAndLinks();
            });
        });

        base.OnInitialized(eventArgs);
    }

    private void RenderSeparators()
    {
        var separatorImage = ResourceItemSeparatorService.GetItemSeparatorImage(ViewModel.OrderItem);
        var separators = this.FindVisualChildren<ItemInfoSeparator>();

        foreach (var itemInfoSeparator in separators)
        {
            itemInfoSeparator.ItemInfoSeparatorImage.Source = separatorImage;
        }
    }

    private void RenderPrice()
    {
        PriceImage.Source = ResourceCurrencyService.GetCurrencyImage(ViewModel.OrderItem.Price.Currency);
    }

    private void RenderSocketsAndLinks()
    {
        ItemBackImage.Source = _bitmapImage;
        ItemBackImage.Width = ItemBackImage.Source.Width;
        ItemBackImage.Height = ItemBackImage.Source.Height;

        var itemSockets = ViewModel.OrderItemInfo.Sockets;
        
        if (itemSockets == null || itemSockets.Count == 0)
        {
            ItemSocketImageRelativePanel.Visibility = Visibility.Collapsed;
            ItemLinkImageCanvas.Visibility = Visibility.Collapsed;
            return;
        }

        ItemImagePanel.Width = ItemBackImage.Source?.Width ?? 100;
        ItemImagePanel.Height = ItemBackImage.Source?.Height ?? 200;
        
        ItemSocketImageRelativePanel.Children.Clear();
        ItemSocketImageRelativePanel.Width = GetContainerWidth(itemSockets);
        ItemSocketImageRelativePanel.Height = GetContainerHeight(itemSockets);
        var socketImages = ItemSocketImageFactory.CreateSockets(itemSockets);
        
        foreach (var socketImage in socketImages)
        {
            ItemSocketImageRelativePanel.Children.Add(socketImage);
        }
        
        ItemLinkImageCanvas.Children.Clear();
        ItemLinkImageCanvas.Width = GetContainerWidth(itemSockets);
        ItemLinkImageCanvas.Height = GetContainerHeight(itemSockets);
        var linkImages = ItemSocketImageFactory.CreateLinks(itemSockets);
        foreach (var linkImage in linkImages)
        {
            ItemLinkImageCanvas.Children.Add(linkImage);
        }
    }
    
    private double GetContainerWidth(Sockets itemSockets, int socketSize = 35, int socketMargin = 6)
    {
        if (itemSockets.Count == 1 || itemSockets.IsVertical)
        {
            return socketSize + socketMargin * 2;
        }

        return ItemBackImage.Source?.Width ?? 100;
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

    private void ItemImagePanel_OnMouseEnter(object sender, MouseEventArgs e)
    {
        ItemLinkImageCanvas.Visibility = Visibility.Collapsed;
        ItemSocketImageRelativePanel.Visibility = Visibility.Collapsed;
    }

    private void ItemImagePanel_OnMouseLeave(object sender, MouseEventArgs e)
    {
        ItemLinkImageCanvas.Visibility = Visibility.Visible;
        ItemSocketImageRelativePanel.Visibility = Visibility.Visible;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Whispered?.Invoke(sender, e);
        DialogResult = true;
    }

    private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}