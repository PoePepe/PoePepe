﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Persistence;
using Poe.UIW.Mapping;
using Poe.UIW.Models;
using Poe.UIW.Services;

namespace Poe.UIW.ViewModels;

public partial class OrderHistoryViewModel: ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<ItemHistoryDto> _historyItemViews;

    [ObservableProperty]
    private OrderViewModel _orderModel = new();
    
    private readonly WhisperService _whisperService;
    private readonly IItemHistoryRepository _itemHistoryRepository;

    public OrderHistoryViewModel(WhisperService whisperService, IItemHistoryRepository itemHistoryRepository)
    {
        _whisperService = whisperService;
        _itemHistoryRepository = itemHistoryRepository;
    }

    public void SetOrder(OrderViewModel order)
    {
        if (OrderModel?.Id is null or 0L)
        {
            OrderModel = order;
        }

        var history = _itemHistoryRepository.GetByOrderId(order.Id, 20, 0);
        HistoryItemViews =
            new ObservableCollection<ItemHistoryDto>(history.ToItemHistoryDto().OrderByDescending(x => x.FoundDate));
    }

    public IEnumerable<ItemHistory> LoadHistory(int top, int skip)
    {
        if (HistoryItemViews.Count >= skip + top)
        {
            return null;
        }
        
        return _itemHistoryRepository.GetByOrderId(OrderModel.Id, top, skip);
    }
    
    [RelayCommand]
    private void ShowInfo(string itemId)
    {
        var itemHistory = _itemHistoryRepository.GetFullByItemId(itemId);
        var orderItem = itemHistory.ItemData.ToOrderItemDto();
        DialogServiceExtensions.ShowOrderItemInfo(orderItem);
    }

    [RelayCommand]
    private async Task Whisper(string itemId)
    {
        var historyView = HistoryItemViews.FirstOrDefault(x => x.ItemId == itemId);

        if (historyView is null)
        {
            return;
        }

        await _whisperService.WhisperAsync(historyView);
    }
}