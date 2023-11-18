using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Poe.LiveSearch.Models;
using Poe.UIW.Models;
using Poe.UIW.Services;
using Poe.UIW.ViewModels;
using Poe.UIW.ViewModels.OrderItemInfoViewModels;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls.Interfaces;

namespace Poe.UIW.Views.Pages;

public partial class LiveSearch : INavigableView<LiveSearchViewModel>
{
    public LiveSearch(LiveSearchViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.DialogControl.ButtonRightClick += DialogControlOnButtonRightClick;
        ViewModel.DialogControl.ButtonLeftClick += DialogControlOnButtonRightClick;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.DialogControl.ButtonRightClick -= DialogControlOnButtonRightClick;
        ViewModel.DialogControl.ButtonLeftClick -= DialogControlOnButtonRightClick;
    }
    
    private static void DialogControlOnButtonRightClick(object sender, RoutedEventArgs e)
    {
        var dialogControl = (IDialogControl)sender;
        dialogControl.Hide();
    }

    public LiveSearchViewModel ViewModel { get; }
    
    private void SelectingItemsControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox cmb)
        {
            return;
        }
        
        if (!cmb.IsDropDownOpen)
        {
            return;
        }
        
        if (DataContext is not LiveSearchViewModel context)
        {
            return;
        }

        if (cmb.DataContext is not OrderViewModel orderViewModel)
        {
            return;
        }

        var newMod = Enum.Parse<OrderMod>(e.AddedItems[0].ToString());
        var oldMod = Enum.Parse<OrderMod>(e.RemovedItems[0].ToString());

        context.ChangeOrderMod(orderViewModel.Id, newMod, oldMod);
    }
    
    protected override void OnInitialized(EventArgs eventArgs)
    {
        ItemImagePanel.Visibility = Visibility.Collapsed;
        // RenderSocketsAndLinks();
        // ItemBackImage.Source = ViewModel.ItemImage.GetAwaiter().GetResult();
        // ItemBackImage.Width = ItemBackImage.Source?.Width ?? 100;
        // ItemBackImage.Height = ItemBackImage.Source?.Height ?? 200;
        base.OnInitialized(eventArgs);
    }
    
    private void RenderSocketsAndLinks()
    {
        // var orderItem = ((OrderItemInfoViewModel)DataContext!).OrderItem;
        // var itemSockets = ((ItemInfo)orderItem.ItemInfo).Sockets;

        var itemSockets = new Sockets
        {
            IsVertical = false,
            Count = 4,
            Groups = new SocketGroup[]
            {
                new SocketGroup
                {
                    Sockets = new Socket[]
                    {
                        new Socket
                        {
                            OrdinalNumber = 1,
                            Color = SocketColor.Red
                        },
                        new Socket
                        {
                            OrdinalNumber = 2,
                            Color = SocketColor.Red
                        },
                        new Socket
                        {
                            OrdinalNumber = 3,
                            Color = SocketColor.Red
                        },
                        new Socket
                        {
                            OrdinalNumber = 4,
                            Color = SocketColor.Red
                        }
                    }
                }
            }
        };
        
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
}