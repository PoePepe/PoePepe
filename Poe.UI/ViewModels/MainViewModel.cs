using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Services;
using Poe.LiveSearch.WebSocket;
using Poe.UI.Mapping;
using Serilog;

namespace Poe.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PoeTradeApiService _poeTradeApiService;

    
    private readonly ServiceState _serviceState;
    private readonly Service _service;
    private readonly IDialogService _dialogService;
    private readonly ChannelReader<OrderError> _orderErrorChannel;

    [ObservableProperty]
    private ObservableCollection<OrderViewModel> _orders;
    
    public static IEnumerable<OrderMod> AvailableMods { get; } = new[] { OrderMod.Whisper, OrderMod.Notify };

    public MainViewModel()
    {
    }

    public MainViewModel(IDialogService dialogService, Service service, ServiceState serviceState, PoeTradeApiService poeTradeApiService)
    {
        _serviceState = serviceState;
        _service = service;
        _poeTradeApiService = poeTradeApiService;
        _dialogService = dialogService;
        _orderErrorChannel = serviceState.OrderErrorChannel.Reader;
        var orders = _service.GetOrders().ToArray();
        _service.StartLiveSearchAsync(orders);
        Orders = new ObservableCollection<OrderViewModel>(orders.ToOrderModel());
        Start(CancellationToken.None);

    }

    private void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            while (await _orderErrorChannel.WaitToReadAsync(token))
            {
                while (_orderErrorChannel.TryRead(out var orderError) && !token.IsCancellationRequested)
                {
                    SetErrorToOrder(orderError);
                }
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        Log.Information("Started receiving data from error channel");
    }

    private void SetErrorToOrder(OrderError orderError)
    {
        var order = Orders.FirstOrDefault(x => x.Id == orderError.OrderId);
        if (order is null)
        {
            return;
        }
        order.ValidationError = orderError.ErrorMessage;
        order.HasValidationErrors = true;

        DisableOrder(orderError.OrderId);
    }

    private async Task Da()
    {
        var searchResponseResult = await _poeTradeApiService.SearchItemsAsync("Ancestor", "e60ZG4hL");
        if (!searchResponseResult.IsSuccess || !searchResponseResult.Content.Result.Any())
        {
            return;
        }
        
        var searchResponse = searchResponseResult.Content;
        
        var fetchResponse = await _poeTradeApiService.FetchItemsAsync(string.Join(',', searchResponse.Result.First()), searchResponse.Id);
        if (!fetchResponse.IsSuccess)
        {
            return;
        }

        var da = fetchResponse.Content.Results.FirstOrDefault();

        if (da is null)
        {
            return;
        }

        da.OrderId = Orders.FirstOrDefault()?.Id ?? 0L;
        da.OrderName = Orders.FirstOrDefault()?.Name;
        da.Orders = new []{new ItemLiveResponse(da.Id, da.OrderId, da.OrderName)};
        await _serviceState.FoundItemsChannel.Writer.WriteAsync(da);
    }

    [RelayCommand]
    public async Task ClearOrders()
    {

        await Da();
        return;

        
        
        if (!Orders.Any())
        {
            return;
        }

        var result = await DialogServiceExtensions.ShowMessageBoxAsync(_dialogService, this, "Are you sure?");
        if (result.HasValue && result.Value)
        {
            Orders.Clear();
            _service.ClearAllOrders();
        }
        
        Log.Information("Orders cleared");
    }

    [RelayCommand]
    public async Task AddNewOrder()
    {
        var id = !Orders.Any() ? 1 : Orders.Select(x => x.Id).Max() + 1;
        var order = await _dialogService.AddNewOrderAsync(this);
        if (order is null)
        {
            return;
        }

        order.Id = id;
        // order.HasValidationErrors = true;
        // order.ValidationError = "Invalid link";
        order.IsActive = true;
        order.Activity = OrderActivity.Enabled;

        Orders.Add(order);

        _service.CreateOrder(order.ToOrder());

        await _service.StartLiveSearchAsync(order.Id);
        
        Log.Information("Order {OrderName} has been created", order.Name);
    }
    
    [RelayCommand]
    public void EnableOrder(long id)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null || order.IsActive)
        {
            return;
        }

        order.Activity = OrderActivity.Enabled;
        order.IsActive = true;
        order.HasValidationErrors = false;
        order.ValidationError = null!;

        _service.EnableLiveSearchOrder(order.Id);
        
        Log.Information("Order {OrderName} has been enabled", order.Name);
    }
    
    [RelayCommand]
    public void DisableOrder(long id)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null || !order.IsActive)
        {
            return;
        }

        order.Activity = OrderActivity.Disabled;
        order.IsActive = false;

        _service.DisableLiveSearchOrder(order.Id);
            
        Log.Information("Order {OrderName} has been disabled", order.Name);
    }
    
    public void ChangeOrderMod(long id, OrderMod newMod, OrderMod oldMod)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null || newMod == oldMod)
        {
            return;
        }

        _service.UpdateOrder(order.ToOrder());

        Log.Information("Mod of order {OrderName} has been changed to {NewMod}", order.Name, newMod);
    }
    
    [RelayCommand]
    public Task CopyOrderLink(string orderQueryLink)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.Clipboard is not { } provider)
        {
            Log.Error(new NullReferenceException(), "Clipboard service unavailable");
            return Task.CompletedTask;
        }

        if (orderQueryLink is null)
        {
            return Task.CompletedTask;
        }

        return provider.SetTextAsync(orderQueryLink);
    }

    [RelayCommand]
    public async Task EditOrder(long id)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null)
        {
            return;
        }

        var updatedOrder = await _dialogService.EditOrderAsync(this, order);

        if (updatedOrder is null)
        {
            return;
        }
        
        order.HasValidationErrors = false;
        order.ValidationError = null!;

        _service.UpdateOrder(order.ToOrder());

        order = updatedOrder;

        Log.Information("Order {OrderName} has been updated", order.Name);
    }

    [RelayCommand]
    public async Task DeleteOrder(long id)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null)
        {
            Log.Warning("Order {OrderId} not found", id);

            return;
        }

        var deleteResult = await DialogServiceExtensions.ShowMessageBoxAsync(_dialogService, this, $"Delete order {order.Name}?");

        if (!deleteResult.HasValue || !deleteResult.Value)
        {
            return;
        }

        if (!Orders.Remove(order))
        {
            Log.Warning("Order {OrderName} hasn't been deleted", order.Name);
        }

        if (!_service.DeleteOrder(id))
        {
            Log.Warning("Order {OrderName} hasn't been deleted", order.Name);
        }

        Log.Information("Order {OrderName} has been deleted", order.Name);
    }
}