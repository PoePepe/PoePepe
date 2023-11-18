using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.DependencyInjection;
using Poe.UIW;
using Poe.UIW.Models;
using Poe.UIW.ViewModels;
using Poe.UIW.ViewModels.OrderItemInfoViewModels;
using Wpf.Ui.Mvvm.Contracts;
using ManageOrderViewModel = Poe.UIW.ViewModels.ManageOrderViewModel;
using OrderItemInfoViewModelBase = Poe.UIW.ViewModels.OrderItemInfoViewModels.OrderItemInfoViewModelBase;
using OrderItemNotificationViewModel = Poe.UIW.ViewModels.OrderItemNotificationViewModel;
using OrderViewModel = Poe.UIW.ViewModels.OrderViewModel;

// ReSharper disable once CheckNamespace

namespace HanumanInstitute.MvvmDialogs;

/// <summary>
/// Provides IDialogService extensions for fluent dialogs.
/// </summary>
public static class DialogServiceExtensions
{
    public static async Task<OrderViewModel> AddNewOrderAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel)
    {
        var da = App.Current.Services.GetRequiredService<ContainerViewModel>();
        
        var vm = service.CreateViewModel<ManageOrderViewModel>();
        await service.ShowDialogAsync(da, vm);
        return vm.DialogResult == true ? vm.ManageOrderModel : null;
    }
    
    public static async Task<OrderViewModel> EditOrderAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel, OrderViewModel order)
    {
        var da = App.Current.Services.GetRequiredService<ContainerViewModel>();

        var vm = service.CreateViewModel<ManageOrderViewModel>();
        vm.SetOrder(order);
        // var settings = new DialogHostSettings(vm);
        await service.ShowDialogAsync(da, vm);
        return vm.DialogResult == true ? vm.ManageOrderModel : null;
    }
    
    public static Task<bool?> ShowMessageBoxAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel, string text, string title = "")
    {
        var da = App.Current.Services.GetRequiredService<ContainerViewModel>();

        return service.ShowMessageBoxAsync(
            da,
            text,
            title,
            MessageBoxButton.YesNo);
    }

    public static async Task ShowOrderItemInfoAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel, OrderItemNotificationViewModel callingViewModel, OrderItemDto orderItem)
    {
        var vm = service.CreateViewModel<OrderItemInfoViewModel>();
        vm.SetOrderItem(orderItem);

        vm.RequestClose += callingViewModel.CloseNotification;

        // var settings = new DialogHostSettings(vm)
        // {
        //     CloseOnClickAway = true
        // };
        //
        // await service.ShowDialogHostAsync(ownerViewModel, settings);
    }
    
    public static async Task ShowOrderItemInfoAsync<T>(this IDialogService service, INotifyPropertyChanged ownerViewModel, OrderItemNotificationViewModel callingViewModel, OrderItemDto orderItem) where T : OrderItemInfoViewModelBase
    {
        var vm = service.CreateViewModel<T>();
        vm.SetOrderItem(orderItem);

        vm.RequestClose += callingViewModel.CloseNotification;

        // var settings = new DialogHostSettings(vm)
        // {
        //     CloseOnClickAway = true
        // };
        //
        // await service.ShowDialogHostAsync(ownerViewModel, settings);
        
        var da = App.Current.Services.GetRequiredService<ContainerViewModel>();

        await service.ShowDialogAsync(da, vm);
    }
}
