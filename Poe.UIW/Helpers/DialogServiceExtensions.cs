using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.DependencyInjection;
using Poe.UIW;
using Poe.UIW.Models;
using Poe.UIW.ViewModels;
using Poe.UIW.ViewModels.OrderItemInfoViewModels;
using Poe.UIW.Views;
using Poe.UIW.Views.OrderItemInfoViews;
using Syroot.Windows.IO;
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
    
    public static void ShowOrderHistory(this OrderViewModel order, Action<object, EventArgs> onClosed)
    {
        var orderHistoryViewModel = App.Current.Services.GetRequiredService<OrderHistoryViewModel>();
        orderHistoryViewModel.SetOrder(order);

        var orderItemInfoView = new OrderHistoryView(orderHistoryViewModel);

        orderItemInfoView.Show();
        orderItemInfoView.Closed += (sender, args) => onClosed(sender, args);
    }

    public static void ShowOrderItemInfo(OrderItemDto orderItem)
    {
        var orderItemInfoViewModel = App.Current.Services.GetRequiredService<OrderItemInfoViewModel>();
        orderItemInfoViewModel.SetOrderItem(orderItem);

        var orderItemInfoView = new OrderItemInfoView(orderItemInfoViewModel);

        orderItemInfoView.ShowDialog();
    }

    public static void ShowOrderItemInfo(OrderItemDto orderItem, AlwaysOnTopView ownerView, Action<object, WhisperEventArgs> onWhispered = null, Action<object, EventArgs> onClosed = null)
    {
        var orderItemInfoViewModel = App.Current.Services.GetRequiredService<OrderItemInfoViewModel>();
        orderItemInfoViewModel.SetOrderItem(orderItem);

        var orderItemInfoView = new OrderItemInfoView(orderItemInfoViewModel, ownerView);
        if (onWhispered is not null)
        {
            orderItemInfoView.Whispered += (sender, args) => onWhispered(sender, args);
        }

        if (onClosed is not null)
        {
            orderItemInfoView.ClosedByButton += (sender, args) => onClosed(sender, args);
        }
        

        orderItemInfoView.ShowDialog();
    }

    public static IDialogStorageFile OpenSoundFile(this IDialogService service, INotifyPropertyChanged owner)
    {
        var settings = new OpenFileDialogSettings
        {
            Title = "Open sound file",
            InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources/Sounds"),
            Filters = new List<FileFilter>
            {
                new("Media files", new [] {"wav", "mp3"})
            }
        };

        return service.ShowOpenFileDialog(owner, settings);
    }

    public static Task<IDialogStorageFile> OpenImportFileAsync(this IDialogService service, INotifyPropertyChanged owner)
    {
        var settings = new OpenFileDialogSettings
        {
            Title = "Open import file",
            InitialDirectory = KnownFolders.Downloads.Path,
            Filters = new List<FileFilter>
            {
                new("Better trading backup", new [] {"txt"}),
                new("PoePepe backup", new [] {"json"})
            }
        };

        return service.ShowOpenFileDialogAsync(owner, settings);
    }

    public static async Task OpenImport(this IDialogService service, INotifyPropertyChanged ownerViewModel)
    {
        var da = App.Current.Services.GetRequiredService<ContainerViewModel>();
        
        var vm = service.CreateViewModel<ImportOrdersViewModel>();
        await service.ShowDialogAsync(da, vm);
    }

    public static async Task OpenExport(this IDialogService service, INotifyPropertyChanged ownerViewModel)
    {
        var da = App.Current.Services.GetRequiredService<ContainerViewModel>();
        
        var vm = service.CreateViewModel<ExportOrdersViewModel>();
        await service.ShowDialogAsync(da, vm);
    }
}
