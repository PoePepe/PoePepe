﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Poe.UI.Models;
using Poe.UI.ViewModels.OrderItemInfoViewModels;

namespace Poe.UI.ViewModels;

public partial class OrderItemNotificationViewModel  : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private INotifyPropertyChanged _mainViewModel;

    [ObservableProperty]
    public OrderItemDto _orderItem;

    public string OrderItemNotificationTitles { get; set; }


    public OrderItemNotificationViewModel()
    {
    }
    
    public OrderItemNotificationViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }
    
    public void SetOrderItem(OrderItemDto orderItem)
    {
        if (string.IsNullOrEmpty(OrderItem?.Id))
        {
            OrderItem = orderItem;
            
            //todo refactor to text format in xaml
            OrderItemNotificationTitles = $"Order {OrderItem.OrderName} has been found";
        }
    }
    
    public void SetMainViewModel(INotifyPropertyChanged mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }
    
    public event EventHandler ClosingRequest;

    private void OnClosingRequest()
    {
        ClosingRequest?.Invoke(this, EventArgs.Empty);
    }
    
    public void CloseNotification(object sender, EventArgs e)
    {
        OnClosingRequest();
    }
    
    [RelayCommand]
    public async Task ShowInfo()
    {
        switch (OrderItem.ItemType)
        {
            case ItemType.DivinationCard:
            case ItemType.Incubator:
            case ItemType.Resonator:
                await _dialogService.ShowOrderItemInfoAsync<StackedItemInfoViewModel>(_mainViewModel, this, OrderItem);
                break;
            
            case ItemType.Map:
            case ItemType.Other:
            default:
                await _dialogService.ShowOrderItemInfoAsync<OrderItemInfoViewModel>(_mainViewModel, this, OrderItem);
                break;
        }
    }
}