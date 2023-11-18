using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.DependencyInjection;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Services;
using Poe.UI.Mapping;
using Poe.UI.Services;
using Poe.UI.ViewModels;
using Serilog;

namespace Poe.UI.Views;

public partial class AlwaysOnTopView : Window
{
    private MainView _mainView;

    
    // Needed to prevent window getting focus
    const int GWL_EXSTYLE = -20;
    // const int WS_EX_NOACTIVATE = 134217728;
    const int WS_EX_NOACTIVATE = 0x08000000;
    const int LSFW_LOCK = 1;
    
    private readonly IDialogService _dialogService;
    private readonly Service _service;
    private bool _isFocus;
    
    public AlwaysOnTopView()
    {
        InitializeComponent();
        this.AttachDevTools();


        _service = App.Current.Services.GetRequiredService<Service>();
        _dialogService = App.Current.Services.GetRequiredService<IDialogService>();
        DataContext = App.Current.Services.GetService<AlwaysOnTopViewModel>();

        ((AlwaysOnTopViewModel)DataContext!).NotifyItem = async result =>
        {
            await Dispatcher.UIThread.InvokeAsync(() => Notify(result));
        };

        // stk_MainPnl.PropertyChanged += (sender, args) =>
        // {
        //     if (stk_MainPnl.Children.Count != 0)
        //     {
        //         IsVisible = true;
        //         Show();
        //     }
        //     else
        //     {
        //         IsVisible = false;
        //         Hide();
        //     }
        // };
        // Show();
        Hide();
        Focusable = false;

        // SetNoActiveWindow();
        
        Loaded += (object sender, RoutedEventArgs e) =>
        {
            SetNoActiveWindow();
            
            _mainView = new MainView();
            _mainView.DataContext = App.Current.Services.GetService<MainViewModel>();
            _mainView.Show();
        };
        PointerEntered += (sender, args) =>
        {

            _isFocus = true;
        };

        PointerExited += (sender, args) =>
        {
            _isFocus = false;
        };
        


        // var dispatcherTimer = new DispatcherTimer();
        // dispatcherTimer.Tick += CheckIfPoeIsForegroundWindow;
        // dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        // dispatcherTimer.Start();
    }
    
    private void SetNoActiveWindow()
    {
        IntPtr hWnd = WindowsInternalFeatureService.GetForegroundWindow();
        // var className = new StringBuilder(256);
        // WindowsInternalFeatureService.GetClassName(hWnd, className, className.Capacity);
        var da1 = WindowsInternalFeatureService.GetWindowTitle(hWnd);
        Console.WriteLine(da1);
        
        var handle = TryGetPlatformHandle()?.Handle;
        if (!handle.HasValue)
        {
            return;
        }

        hWnd = handle.Value;

        var hWndProcHook = WindowsInternalFeatureService.GetWindowLong(hWnd, -4);
        WndProcHookDel del = WndProcHook;

        var newLong = Marshal.GetFunctionPointerForDelegate(del);
        WindowsInternalFeatureService.SetWindowLongPtr(hWnd, -4, newLong);
        
        // var longR = WindowsInternalFeatureService.SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_NOACTIVATE);
        // var error = Marshal.GetLastWin32Error();
        //
        // var lockR = WindowsInternalFeatureService.LockSetForegroundWindow(LSFW_LOCK);
        // Log.Information("SetNoActiveWindow Error {Error}", error);
        // Log.Information("SetNoActiveWindow {LongR}", longR);
        // Log.Information("SetNoActiveWindow {LockR}", lockR);
    }
    delegate IntPtr WndProcHookDel(IntPtr hwnd, int code, IntPtr wParam, IntPtr lParam, ref bool handled);
    private unsafe IntPtr WndProcHook(IntPtr hwnd, int code, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        int WM_MOUSEACTIVATE = 0x0021;
        if (code == WM_MOUSEACTIVATE )
        {
            // handled = true;
            // return new IntPtr(3); 
        }

        return WindowsInternalFeatureService.WndProc(hwnd, code, wParam, lParam);
    }

    private void CheckIfPoeIsForegroundWindow(object sender, EventArgs e)
    {
        if (stk_MainPnl.Children.Count == 0)
        {
            if (IsVisible)
            {
                Hide();
            }
            
            return;
        }
        
        var poeWindow = WindowsInternalFeatureService.FindPoeGameWindow();
        if (poeWindow == IntPtr.Zero)
        {
            Hide();
            return;
        }
        
        var currentWindow = WindowsInternalFeatureService.GetForegroundWindow();
        var notificationWindow = TryGetPlatformHandle()?.Handle;
        if (!notificationWindow.HasValue)
        {
            return;
        }

        if (currentWindow == poeWindow || currentWindow == notificationWindow)
        {
            if (stk_MainPnl.Children.Count == 0)
            {
                return;
            }
            
            Show();

            WindowsInternalFeatureService.SetForegroundWindow(_isFocus ? notificationWindow.Value : poeWindow);
        }
        else
        {
            Hide();
        }
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
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _mainView.Close();
        base.OnClosing(e);
    }
}