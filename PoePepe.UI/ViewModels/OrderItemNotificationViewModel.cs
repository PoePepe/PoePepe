using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using PoePepe.UI.Helpers;
using PoePepe.UI.Models;
using PoePepe.UI.Properties;
using PoePepe.UI.Views;

namespace PoePepe.UI.ViewModels;

public partial class OrderItemNotificationViewModel  : ViewModelBase
{
    [ObservableProperty] private OrderItemDto _orderItem;

    private AlwaysOnTopView _ownerView;
    [ObservableProperty] private int _timeOut;

    public OrderItemNotificationViewModel()
    {
        TimeOut = 100;
    }

    public void SetOrderItem(OrderItemDto orderItem)
    {
        if (string.IsNullOrEmpty(OrderItem?.Id))
        {
            OrderItem = orderItem;
        }
    }

    public void SetOwnerView(AlwaysOnTopView view)
    {
        _ownerView = view;
    }

    public event EventHandler ClosingRequest;
    public event EventHandler InfoClosed;

    private void OnClosingRequest()
    {
        ClosingRequest?.Invoke(this, EventArgs.Empty);
    }

    private void OnWhispered(object sender, WhisperEventArgs e)
    {
        ClosingRequest?.Invoke(sender, e);

        if (UserSettings.Default.HideIfPoeUnfocused)
        {
            WindowsInternalFeatureService.SetForegroundPoeGameWindow();
        }
    }

    private void OnClosed(object sender, EventArgs e)
    {
        InfoClosed?.Invoke(sender, e);

        if (UserSettings.Default.HideIfPoeUnfocused)
        {
            WindowsInternalFeatureService.SetForegroundPoeGameWindow();
        }
    }

    public void CloseNotification(object sender, EventArgs e)
    {
        OnClosingRequest();
    }

    [RelayCommand]
    private void ShowInfo()
    {
        DialogServiceExtensions.ShowOrderItemInfo(OrderItem, _ownerView, OnWhispered, OnClosed);
    }
}