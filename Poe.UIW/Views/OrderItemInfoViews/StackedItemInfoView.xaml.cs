﻿using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Poe.UIW.Models;
using Poe.UIW.Services;
using Poe.UIW.Services.Currency;
using Poe.UIW.ViewModels.OrderItemInfoViewModels;

namespace Poe.UIW.Views.OrderItemInfoViews;

public partial class StackedItemInfoView : Window
{
    public StackedItemInfoViewModel ViewModel { get; }

    private BitmapImage _bitmapImage;

    public event EventHandler Whispered;

    public StackedItemInfoView(StackedItemInfoViewModel viewModel)
    {
        DataContext = viewModel;
        ViewModel = viewModel;
        _bitmapImage = new BitmapImage();
        Loaded += OnLoaded;

        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RenderPrice();
    }

    private void RenderPrice()
    {
        PriceImage.Source = ResourceCurrencyService.GetCurrencyImage(ViewModel.OrderItem.Price.Currency);
    }

    protected override void OnInitialized(EventArgs eventArgs)
    {
        ThreadPool.QueueUserWorkItem(async _ =>
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                _bitmapImage = await ViewModel.LoadItemImageAsync(_bitmapImage);
                RenderSockets();
            });
        });

        ItemLevelTextBlock.Visibility =
            ViewModel.StackedItemInfo.ItemLevel > 0 ? Visibility.Visible : Visibility.Collapsed;

        base.OnInitialized(eventArgs);
    }

    private void RenderSockets()
    {
        StackedItemImage.Source = _bitmapImage;
        StackedItemImage.Width = StackedItemImage.Source.Width;
        StackedItemImage.Height = StackedItemImage.Source.Height;

        var itemSockets = ViewModel.StackedItemInfo.Sockets;

        if (itemSockets == null || itemSockets.Count == 0)
        {
            ItemSocketImageRelativePanel.Visibility = Visibility.Collapsed;
            return;
        }

        ItemImagePanel.Width = StackedItemImage.Source?.Width ?? 100;
        ItemImagePanel.Height = StackedItemImage.Source?.Height ?? 200;

        ItemSocketImageRelativePanel.Children.Clear();
        ItemSocketImageRelativePanel.Width = GetContainerWidth(itemSockets);
        ItemSocketImageRelativePanel.Height = GetContainerHeight(itemSockets);
        var socketImages = ItemSocketImageFactory.CreateSockets(itemSockets);

        foreach (var socketImage in socketImages)
        {
            ItemSocketImageRelativePanel.Children.Add(socketImage);
        }
    }

    private double GetContainerWidth(Sockets itemSockets, int socketSize = 35, int socketMargin = 6)
    {
        if (itemSockets.Count == 1 || itemSockets.IsVertical)
        {
            return socketSize + socketMargin * 2;
        }

        return StackedItemImage.Source?.Width ?? 100;
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

    private void ItemImagePanel_OnMouseLeave(object sender, MouseEventArgs e)
    {
        ItemSocketImageRelativePanel.Visibility = Visibility.Visible;
    }

    private void ItemImagePanel_OnMouseEnter(object sender, MouseEventArgs e)
    {
        ItemSocketImageRelativePanel.Visibility = Visibility.Collapsed;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Whispered?.Invoke(sender, e);
        DialogResult = true;
    }
}