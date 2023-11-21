using System;
using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.DependencyInjection;
using Poe.UIW;
using Poe.UIW.Models;
using Poe.UIW.ViewModels;
using Poe.UIW.ViewModels.OrderItemInfoViewModels;
using Poe.UIW.Views.OrderItemInfoViews;
using ManageOrderViewModel = Poe.UIW.ViewModels.ManageOrderViewModel;
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
    
    public static void ShowOrderItemInfo(OrderItemDto orderItem, Action<object, EventArgs> onWhispered)
    {
        var orderItemInfoViewModel = App.Current.Services.GetRequiredService<OrderItemInfoViewModel>();
        orderItemInfoViewModel.SetOrderItem(orderItem);

        var orderItemInfoView = new OrderItemInfoView(orderItemInfoViewModel);
        orderItemInfoView.Whispered += (sender, args) => onWhispered(sender, args);

        orderItemInfoView.ShowDialog();
    }
}
