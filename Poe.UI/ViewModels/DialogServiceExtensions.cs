using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs.Avalonia.DialogHost;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Poe.UI.Models;
using Poe.UI.ViewModels;
using Poe.UI.ViewModels.OrderItemInfoViewModels;
using OrderItemInfoViewModelBase = Poe.UI.ViewModels.OrderItemInfoViewModels.OrderItemInfoViewModelBase;

// ReSharper disable once CheckNamespace

namespace HanumanInstitute.MvvmDialogs;

/// <summary>
/// Provides IDialogService extensions for fluent dialogs.
/// </summary>
public static class DialogServiceExtensions
{
    public static async Task<OrderViewModel> AddNewOrderAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel)
    {
        var vm = service.CreateViewModel<ManageOrderViewModel>();
        var settings = new DialogHostSettings(vm);
        await service.ShowDialogHostAsync(ownerViewModel, settings);
        return vm.DialogResult == true ? vm.ManageOrderModel : null;
    }
    
    public static async Task<OrderViewModel> EditOrderAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel, OrderViewModel order)
    {
        var vm = service.CreateViewModel<ManageOrderViewModel>();
        vm.SetOrder(order);
        var settings = new DialogHostSettings(vm);
        await service.ShowDialogHostAsync(ownerViewModel, settings);
        return vm.DialogResult == true ? vm.ManageOrderModel : null;
    }
    
    public static Task<bool?> ShowMessageBoxAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel, string text, string title = "")
    {
        return service.ShowMessageBoxAsync(
            ownerViewModel,
            text,
            title,
            MessageBoxButton.YesNo);
    }

    public static async Task ShowOrderItemInfoAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel, OrderItemNotificationViewModel callingViewModel, OrderItemDto orderItem)
    {
        var vm = service.CreateViewModel<OrderItemInfoViewModel>();
        vm.SetOrderItem(orderItem);

        vm.RequestClose += callingViewModel.CloseNotification;

        var settings = new DialogHostSettings(vm)
        {
            CloseOnClickAway = true
        };

        await service.ShowDialogHostAsync(ownerViewModel, settings);
    }
    
    public static async Task ShowOrderItemInfoAsync<T>(this IDialogService service, INotifyPropertyChanged ownerViewModel, OrderItemNotificationViewModel callingViewModel, OrderItemDto orderItem) where T : OrderItemInfoViewModelBase
    {
        var vm = service.CreateViewModel<T>();
        vm.SetOrderItem(orderItem);

        vm.RequestClose += callingViewModel.CloseNotification;

        var settings = new DialogHostSettings(vm)
        {
            CloseOnClickAway = true
        };

        await service.ShowDialogHostAsync(ownerViewModel, settings);
    }
}
