using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.DependencyInjection;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Services;
using Poe.UIW.Helpers;
using Poe.UIW.Mapping;
using Poe.UIW.Services;
using Poe.UIW.ViewModels;
using Serilog;

namespace Poe.UIW.Views;

public partial class AlwaysOnTopView : Window
{
    const int GWL_EXSTYLE = -20;
    const int WS_EX_NOACTIVATE = 134217728;
    const int LSFW_LOCK = 1;

    private readonly IDialogService _dialogService;
    private readonly Service _service;
    
    public AlwaysOnTopView()
    {
        InitializeComponent();
        
        _service = App.Current.Services.GetRequiredService<Service>();
        _dialogService = App.Current.Services.GetRequiredService<IDialogService>();
        DataContext = App.Current.Services.GetService<AlwaysOnTopViewModel>();

        ((AlwaysOnTopViewModel)DataContext!).NotifyItem = async result =>
        {
            await Dispatcher.InvokeAsync(() => Notify(result));
        };
        
        Loaded += (_, _) => SetNoActiveWindow();
        
        var dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Tick += CheckIfPoeIsForegroundWindow;
        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        dispatcherTimer.Start();
    }
    
    private void SetNoActiveWindow()
    {
        var helper = new WindowInteropHelper(this);
        WindowsInternalFeatureService.SetWindowLong(helper.Handle, GWL_EXSTYLE, WS_EX_NOACTIVATE);
        WindowsInternalFeatureService.LockSetForegroundWindow(LSFW_LOCK);
    }

    private void CheckIfPoeIsForegroundWindow(object sender, EventArgs eventArgs)
    {
        // if (WindowsInternalFeatureService.GetForegroundWindow() != WindowsInternalFeatureService.FindPoeGameWindow())
            // Hide();
        // else
            // Show();
    }
    
    private void Notify(FetchResponseResult result)
    {
        try
        {
            var vm = new OrderItemNotificationViewModel(_dialogService);

            var orderItem = result.ToOrderItemDto();
            vm.SetOrderItem(orderItem);
            vm.SetMainViewModel(DataContext as INotifyPropertyChanged);
            var uc = new OrderItemNotificationView
            {
                DataContext = vm
            };
            stk_MainPnl.Children.Add(uc);
            stk_MainPnl.Height += 53;
            ResizablePanel.Height += 53;
            Show();
        }
        catch (Exception e)
        {
            Log.Error(e, "Error notifying of found item by order {OrderName}", result.OrderName);
        }
    }
}