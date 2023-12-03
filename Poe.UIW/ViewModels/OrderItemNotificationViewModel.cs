using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Poe.UIW.Models;

namespace Poe.UIW.ViewModels;

public partial class OrderItemNotificationViewModel  : ViewModelBase
{
    [ObservableProperty] private int _timeOut;

    [ObservableProperty] private OrderItemDto _orderItem;

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
    
    public event EventHandler ClosingRequest;
    public event EventHandler InfoClosed;

    private void OnClosingRequest()
    {
        ClosingRequest?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnWhispered(object sender, WhisperEventArgs e)
    {
        ClosingRequest?.Invoke(sender, e);
    }
    
    private void OnClosed(object sender, EventArgs e)
    {
        InfoClosed?.Invoke(sender, e);
    }

    public void CloseNotification(object sender, EventArgs e)
    {
        OnClosingRequest();
    }
    
    [RelayCommand]
    private void ShowInfo()
    {
        DialogServiceExtensions.ShowOrderItemInfo(OrderItem, OnWhispered, OnClosed);
    }
}