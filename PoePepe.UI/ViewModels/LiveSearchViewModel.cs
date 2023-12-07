using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.DependencyInjection;
using PoePepe.LiveSearch.Api.Trade;
using PoePepe.LiveSearch.Models;
using PoePepe.LiveSearch.Services;
using PoePepe.LiveSearch.WebSocket;
using PoePepe.UI.Helpers;
using PoePepe.UI.Mapping;
using PoePepe.UI.Models;
using PoePepe.UI.Properties;
using Serilog;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Clipboard = System.Windows.Clipboard;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace PoePepe.UI.ViewModels;

public partial class LiveSearchViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    private readonly EventHandler _ordersChanged;
    private readonly PoeTradeApiService _poeTradeApiService;
    private readonly Service _service;
    private readonly ServiceState _serviceState;
    private readonly ISnackbarService _snackbarService;
    private readonly WorkerManager _workerManager;
    public readonly IDialogControl DialogControl;
    [ObservableProperty] private int _activeOrders;

    [ObservableProperty] private OrderSort _actualSort;
    [ObservableProperty] private int _failedOrders;
    [ObservableProperty] private ObservableCollection<OrderViewModel> _filteredOrders = new();

    [ObservableProperty] private bool _isOrdersEnabling;
    [ObservableProperty] private bool _isRowsSelected;
    [ObservableProperty] private bool _isSelectedAllOrders;
    [ObservableProperty] private OrderMod _modForSelectedOrders;

    private string _openedHistoryForOrder;
    [ObservableProperty] private ObservableCollection<OrderViewModel> _orders;

    [ObservableProperty] private int _totalOrders;
    public EventHandler FilteredOrdersChanged;

    public LiveSearchViewModel()
    {
    }

    public LiveSearchViewModel(IDialogService customDialogService, Service service, ServiceState serviceState,
        PoeTradeApiService poeTradeApiService, Wpf.Ui.Mvvm.Contracts.IDialogService dialogService,
        ISnackbarService snackbarService, WorkerManager workerManager)
    {
        _dialogService = customDialogService;
        DialogControl = dialogService.GetDialogControl();
        _serviceState = serviceState;
        _service = service;
        _poeTradeApiService = poeTradeApiService;
        _snackbarService = snackbarService;
        _workerManager = workerManager;

        var sortKind = Enum.TryParse<OrderSortKind>(UserSettings.Default.LiveSearchSort, out var sort)
            ? sort
            : OrderSortKind.CreationDateDesc;
        ActualSort = AvailableSorting.First(x => x.Kind == sortKind);
        
        _ordersChanged += OrdersChangedHandler;

        LoadOrders();
        Start(CancellationToken.None);
    }

    public static IEnumerable<OrderMod> AvailableMods { get; } = new[] { OrderMod.Whisper, OrderMod.Notify };

    public static IEnumerable<OrderSort> AvailableSorting { get; } = new[]
    {
        new OrderSort(OrderSortKind.NameAsc, "Name A-Z"),
        new OrderSort(OrderSortKind.NameDesc, "Name Z-A"),
        new OrderSort(OrderSortKind.CreationDateAsc, "Oldest"),
        new OrderSort(OrderSortKind.CreationDateDesc, "Newest"),
        new OrderSort(OrderSortKind.Notify, "Notify mod"),
        new OrderSort(OrderSortKind.Whisper, "Whisper mod"),
        new OrderSort(OrderSortKind.Enabled, "Enabled status"),
        new OrderSort(OrderSortKind.Disabled, "Disabled status")
    };

    private void OrdersChangedHandler(object sender, EventArgs e)
    {
        TotalOrders = Orders.Count;
        ActiveOrders = Orders.Count(x => x.IsActive);
        FailedOrders = Orders.Count(x => x.HasErrors || x.HasValidationErrors);
    }

    private void LoadOrders()
    {
        var orders = _service.GetOrders().ToArray();
        ThreadPool.QueueUserWorkItem(async _ => { await _service.StartLiveSearchAsync(orders); });
        Orders = new ObservableCollection<OrderViewModel>(orders.ToOrderModel());
        FilteredOrders = new ObservableCollection<OrderViewModel>(Orders.Sort(ActualSort));

        _ordersChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeLeagueSearchingOrders()
    {
        _service.StopSearchingForOrders();
        _serviceState.LeagueName = UserSettings.Default.LeagueName;

        _workerManager.RestartWorkers();
        App.Current.Services.GetRequiredService<AlwaysOnTopViewModel>().Restart();

        var orders = _service.GetOrders().ToArray();
        ThreadPool.QueueUserWorkItem(async _ => { await _service.StartLiveSearchAsync(orders); });
    }

    private void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            while (await _serviceState.OrderErrorChannel.Reader.WaitToReadAsync(token))
            {
                while (_serviceState.OrderErrorChannel.Reader.TryRead(out var orderError) && !token.IsCancellationRequested)
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

        order.SetCommonValidationError(orderError.ErrorMessage);

        DisableOrder(orderError.OrderId);

        _ordersChanged?.Invoke(this, EventArgs.Empty);

        _snackbarService.Show(
            $"Error {orderError.ErrorMessage} occurred in order {order.Name}.",
            "The search has been stopped.",
            SymbolRegular.Alert24,
            ControlAppearance.Danger
        );

        Log.Error("Error {Error} occurred in order {OrderName}", orderError.ErrorMessage, order.Name);
    }

    private async Task Test_GetOneItem()
    {
        var searchResponseResult = await _poeTradeApiService.SearchItemsAsync("Standard", "Ab3LSL");
        if (!searchResponseResult.IsSuccess || !searchResponseResult.Content.Result.Any())
        {
            return;
        }

        var searchResponse = searchResponseResult.Content;

        var fetchResponse =
            await _poeTradeApiService.FetchItemsAsync(string.Join(',', searchResponse.Result.First()),
                searchResponse.Id);
        if (!fetchResponse.IsSuccess)
        {
            return;
        }

        var fetchResponseResult = fetchResponse.Content?.Results?.FirstOrDefault();

        if (fetchResponseResult is null)
        {
            Console.WriteLine("null");
            return;
        }

        fetchResponseResult.OrderId = Orders.FirstOrDefault()?.Id ?? 0L;
        fetchResponseResult.OrderName = Orders.FirstOrDefault()?.Name;
        fetchResponseResult.Orders = new[] { new ItemLiveResponse(fetchResponseResult.Id, fetchResponseResult.OrderId, fetchResponseResult.OrderName) };
        await _serviceState.FoundItemsChannel.Writer.WriteAsync(fetchResponseResult);
    }

    [RelayCommand]
    private async Task ImportOrders()
    {
        await _dialogService.OpenImport(this);
        _ordersChanged?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task ExportOrders()
    {
        await _dialogService.OpenExport(this);
    }

    [RelayCommand]
    private async Task ClearOrders()
    {
        await Test_GetOneItem();
        return;

        if (!Orders.Any())
        {
            return;
        }

        var result = await DialogControl.ShowAndWaitAsync("",
            "Clear orders?", true);

        switch (result)
        {
            case IDialogControl.ButtonPressed.Left:
                break;

            case IDialogControl.ButtonPressed.Right:
            case IDialogControl.ButtonPressed.None:
            default:
                return;
        }

        Orders.Clear();
        FilteredOrders.Clear();
        FilteredOrdersChanged.Invoke(this, EventArgs.Empty);
        _ordersChanged?.Invoke(this, EventArgs.Empty);

        _service.ClearAllOrders();

        Log.Information("Orders cleared");
    }

    [RelayCommand]
    private async Task AddNewOrder()
    {
        var id = !Orders.Any() ? 1 : Orders.Select(x => x.Id).Max() + 1;
        var order = await _dialogService.AddNewOrderAsync(this);
        if (order is null)
        {
            return;
        }

        order.Id = id;

        var activeOrderCount = _service.GetOrders().Count(x => x.Activity == OrderActivity.Enabled);
        if (activeOrderCount >= 20)
        {
            order.IsActive = false;
            order.Activity = OrderActivity.Disabled;
        }
        else
        {
            order.IsActive = true;
            order.Activity = OrderActivity.Enabled;
        }

        order.CreatedAt = DateTimeOffset.UtcNow;
        _service.CreateOrder(order.ToOrder());

        Orders.Add(order);
        FilteredOrders.Add(order);
        FilteredOrders = new ObservableCollection<OrderViewModel>(FilteredOrders.Sort(ActualSort));

        if (order.IsActive)
        {
            await _service.StartLiveSearchAsync(order.Id);
        }

        OpenSnackbarOrderAdded(order);

        FilteredOrdersChanged?.Invoke(this, EventArgs.Empty);
        _ordersChanged?.Invoke(this, EventArgs.Empty);

        Log.Information("Order {OrderName} has been created", order.Name);
    }

    private void OpenSnackbarOrderAdded(OrderViewModel order)
    {
        var title = order.IsActive
            ? $"Order {order.Name} added."
            : $"Order {order.Name} added, but live search hasn't been run.";

        var message = order.IsActive
            ? "Live search is up and running!"
            : "Live search hasn't been run. You exceed limit in 20 active orders. Please disable other orders to release slots.";

        _snackbarService.Show(
            title,
            message,
            SymbolRegular.CheckmarkCircle24,
            ControlAppearance.Success
        );
    }

    private void OpenSnackbarOrderEdited(string orderName)
    {
        _snackbarService.Show(
            $"Order {orderName} updated.",
            null,
            SymbolRegular.CheckmarkCircle24,
            ControlAppearance.Success
        );
    }

    private void OpenSnackbarOrderEnabled(OrderViewModel order)
    {
        var title = order.IsActive
            ? $"Order {order.Name} enabled."
            : $"Order {order.Name} hasn't been enabled.";

        var message = order.IsActive
            ? "Live search is up and running!"
            : "Live search hasn't been run. You exceed limit in 20 active orders. Please disable other orders to release slots.";

        var color = order.IsActive
            ? ControlAppearance.Success
            : ControlAppearance.Caution;

        var icon = order.IsActive
            ? SymbolRegular.CheckmarkCircle24
            : SymbolRegular.Warning24;

        _snackbarService.Show(
            title,
            message,
            icon,
            color
        );
    }

    [RelayCommand]
    private async Task EnableOrder(long id)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null || order.IsActive)
        {
            return;
        }

        var activeOrderCount = _service.GetOrders().Count(x => x.Activity == OrderActivity.Enabled);
        if (activeOrderCount >= 20)
        {
            order.IsActive = false;
            order.Activity = OrderActivity.Disabled;
        }
        else
        {
            order.IsActive = true;
            order.Activity = OrderActivity.Enabled;

            order.ClearCommonValidationError();

            await _service.EnableLiveSearchOrder(order.Id);
        }

        _ordersChanged?.Invoke(this, EventArgs.Empty);
        OpenSnackbarOrderEnabled(order);

        Log.Information("Order {OrderName} has been enabled", order.Name);
    }

    [RelayCommand]
    private void DisableOrder(long id)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null || !order.IsActive)
        {
            return;
        }

        order.Activity = OrderActivity.Disabled;
        order.IsActive = false;

        _service.DisableLiveSearchOrder(order.Id);

        _ordersChanged?.Invoke(this, EventArgs.Empty);

        Log.Information("Order {OrderName} has been disabled", order.Name);
    }

    public void UpdateManyOrderMod(IEnumerable<long> ids, OrderMod newMod)
    {
        _service.UpdateManyMod(ids, newMod);

        Log.Information("Mod of orders {OrderIds} has been changed to {NewMod}", string.Join(',', ids), newMod);
    }

    public void UpdateAllOrderMod(OrderMod newMod)
    {
        _service.UpdateAllMod(newMod);

        Log.Information("Mod of all orders has been changed to {NewMod}", newMod);
    }

    public void ChangeOrderMod(long id, OrderMod newMod, OrderMod oldMod)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null || newMod == oldMod)
        {
            return;
        }

        order.Mod = newMod;

        _service.UpdateOrder(order.ToOrder());

        Log.Information("Mod of order {OrderName} has been changed to {NewMod}", order.Name, newMod);
    }

    private void OnHistoryClosed(object sender, EventArgs e)
    {
        _openedHistoryForOrder = null;
    }

    [RelayCommand]
    private void ShowOrderHistory(long id)
    {
        if (_openedHistoryForOrder is not null)
        {
            return;
        }

        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null)
        {
            return;
        }

        _openedHistoryForOrder = order.Name;

        order.ShowOrderHistory(OnHistoryClosed);
    }

    [RelayCommand]
    private void CopyOrderLink(string queryHash)
    {
        if (queryHash is null)
        {
            return;
        }

        Clipboard.SetText($"https://www.pathofexile.com/trade/search/{UserSettings.Default.LeagueName}/{queryHash}");
    }

    [RelayCommand]
    private async Task EditOrder(long id)
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

        order.ClearCommonValidationError();

        _service.UpdateOrder(order.ToOrder());

        order = updatedOrder;

        _ordersChanged?.Invoke(this, EventArgs.Empty);

        OpenSnackbarOrderEdited(order.Name);

        Log.Information("Order {OrderName} has been updated", order.Name);
    }

    [RelayCommand]
    private async Task DeleteOrder(long id)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);

        if (order is null)
        {
            Log.Warning("Order {OrderId} not found", id);

            return;
        }

        var result = await DialogControl.ShowAndWaitAsync("",
            $"Delete order {order.Name}?", true
        );

        switch (result)
        {
            case IDialogControl.ButtonPressed.Left:
                break;

            case IDialogControl.ButtonPressed.Right:
            case IDialogControl.ButtonPressed.None:
            default:
                return;
        }

        if (!Orders.Remove(order))
        {
            Log.Warning("Order {OrderName} hasn't been deleted from presentation", order.Name);
        }

        if (!_service.DeleteOrder(id))
        {
            Log.Warning("Order {OrderName} hasn't been deleted from store", order.Name);
        }

        FilteredOrdersChanged.Invoke(this, EventArgs.Empty);
        _ordersChanged?.Invoke(this, EventArgs.Empty);

        Log.Information("Order {OrderName} has been deleted", order.Name);
    }

    [RelayCommand]
    private async Task EnableSelectedOrders()
    {
        try
        {
            IsOrdersEnabling = true;
            await InternalEnableSelectedOrders();
        }
        finally
        {
            IsOrdersEnabling = false;
        }
    }

    private async Task InternalEnableSelectedOrders()
    {
        var selectedDisabledOrders = Orders.Where(x => x.IsSelected && !x.IsActive);

        var availableSlots = 20 - Orders.Count(x => x.Activity == OrderActivity.Enabled);
        var ordersForEnable = selectedDisabledOrders.Take(availableSlots);

        foreach (var orderViewModel in ordersForEnable)
        {
            await _service.EnableLiveSearchOrder(orderViewModel.Id);

            orderViewModel.IsActive = true;
            orderViewModel.Activity = OrderActivity.Enabled;

            orderViewModel.ClearCommonValidationError();
        }

        if (selectedDisabledOrders.Any())
        {
            await _snackbarService.ShowAsync(
                $"{selectedDisabledOrders.Count()} orders hasn't been enabled.",
                "You exceed limit in 20 active orders. Please disable other orders to release slots.",
                SymbolRegular.Warning24,
                ControlAppearance.Caution
            );
        }

        _ordersChanged?.Invoke(this, EventArgs.Empty);

        Log.Information("Selected orders has been enabled");
    }

    [RelayCommand]
    private void DisableSelectedOrders()
    {
        var selectedOrders = Orders.Where(x => x.IsSelected).Select(x => x.Id);

        foreach (var selectedOrderId in selectedOrders)
        {
            DisableOrder(selectedOrderId);
        }

        _ordersChanged?.Invoke(this, EventArgs.Empty);

        Log.Information("Selected orders has been disabled");
    }

    [RelayCommand]
    private async Task DeleteSelectedOrders()
    {
        var result = await DialogControl.ShowAndWaitAsync("",
            "Delete selected orders?", true);

        switch (result)
        {
            case IDialogControl.ButtonPressed.Left:
                break;

            case IDialogControl.ButtonPressed.Right:
            case IDialogControl.ButtonPressed.None:
            default:
                return;
        }

        if (IsSelectedAllOrders)
        {
            await ClearOrders();

            FilteredOrdersChanged.Invoke(this, EventArgs.Empty);
            Log.Information("Selected orders has been deleted");

            return;
        }

        var selectedOrders = Orders.Where(x => x.IsSelected).Select(x => x.Id);

        var unselectedOrders = Orders.Where(x => !x.IsSelected);
        Orders = new ObservableCollection<OrderViewModel>(unselectedOrders);
        FilteredOrders = new ObservableCollection<OrderViewModel>(unselectedOrders);

        foreach (var id in selectedOrders)
        {
            _service.DeleteOrder(id);
        }

        FilteredOrdersChanged.Invoke(this, EventArgs.Empty);
        _ordersChanged?.Invoke(this, EventArgs.Empty);

        Log.Information("Selected orders has been deleted");
    }
}