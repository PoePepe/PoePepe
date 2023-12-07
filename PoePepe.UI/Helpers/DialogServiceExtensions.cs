using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.DependencyInjection;
using PoePepe.UI;
using PoePepe.UI.Models;
using PoePepe.UI.ViewModels;
using PoePepe.UI.ViewModels.OrderItemInfoViewModels;
using PoePepe.UI.Views;
using PoePepe.UI.Views.OrderItemInfoViews;
using Syroot.Windows.IO;

// ReSharper disable once CheckNamespace

namespace HanumanInstitute.MvvmDialogs;

/// <summary>
/// Provides IDialogService extensions for fluent dialogs.
/// </summary>
public static class DialogServiceExtensions
{
    public static async Task<OrderViewModel> AddNewOrderAsync(this IDialogService service,
        INotifyPropertyChanged ownerViewModel)
    {
        var owner = App.Current.Services.GetRequiredService<ContainerViewModel>();

        var vm = service.CreateViewModel<ManageOrderViewModel>();
        await service.ShowDialogAsync(owner, vm);
        return vm.DialogResult == true ? vm.ManageOrderModel : null;
    }

    public static async Task<OrderViewModel> EditOrderAsync(this IDialogService service,
        INotifyPropertyChanged ownerViewModel, OrderViewModel order)
    {
        var owner = App.Current.Services.GetRequiredService<ContainerViewModel>();
        var vm = service.CreateViewModel<ManageOrderViewModel>();
        vm.SetOrder(order);

        await service.ShowDialogAsync(owner, vm);
        return vm.DialogResult == true ? vm.ManageOrderModel : null;
    }

    public static Task<bool?> ShowMessageBoxAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel,
        string text, string title = "")
    {
        var owner = App.Current.Services.GetRequiredService<ContainerViewModel>();

        return service.ShowMessageBoxAsync(
            owner,
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

    public static void ShowOrderItemInfo(OrderItemDto orderItem, AlwaysOnTopView ownerView,
        Action<object, WhisperEventArgs> onWhispered = null, Action<object, EventArgs> onClosed = null)
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

    public static IDialogStorageFile OpenSoundFile(this IDialogService service)
    {
        var owner = App.Current.Services.GetRequiredService<ContainerViewModel>();

        var settings = new OpenFileDialogSettings
        {
            Title = "Open sound file",
            InitialDirectory = KnownFolders.Downloads.Path,
            Filters = new List<FileFilter>
            {
                new("Media files", new[] { "wav", "mp3" })
            }
        };

        return service.ShowOpenFileDialog(owner, settings);
    }

    public static Task<IDialogStorageFile> OpenImportFileAsync(this IDialogService service)
    {
        var owner = App.Current.Services.GetRequiredService<ContainerViewModel>();

        var settings = new OpenFileDialogSettings
        {
            Title = "Open backup file",
            InitialDirectory = KnownFolders.Downloads.Path,
            Filters = new List<FileFilter>
            {
                new("Text file", new[] { "txt" })
            }
        };

        return service.ShowOpenFileDialogAsync(owner, settings);
    }

    public static Task<IDialogStorageFile> OpenExportFolderAsync(this IDialogService service, string fileName)
    {
        var owner = App.Current.Services.GetRequiredService<ContainerViewModel>();

        var settings = new SaveFileDialogSettings
        {
            Title = "Export as",
            InitialDirectory = KnownFolders.Downloads.Path,
            Filters = new List<FileFilter>
            {
                new("Text Documents", "txt"),
                new("All Files", "*")
            },
            DefaultExtension = ".txt",
            InitialFile = fileName
        };

        return service.ShowSaveFileDialogAsync(owner, settings);
    }

    public static async Task OpenImport(this IDialogService service, LiveSearchViewModel ownerViewModel)
    {
        var owner = App.Current.Services.GetRequiredService<ContainerViewModel>();

        var vm = service.CreateViewModel<ImportOrdersViewModel>();
        vm.SetOwnerViewModel(ownerViewModel);

        await service.ShowDialogAsync(owner, vm);
    }

    public static async Task OpenExport(this IDialogService service, LiveSearchViewModel ownerViewModel)
    {
        var owner = App.Current.Services.GetRequiredService<ContainerViewModel>();

        var vm = service.CreateViewModel<ExportOrdersViewModel>();
        vm.SetOwnerViewModel(ownerViewModel);

        await service.ShowDialogAsync(owner, vm);
    }
}