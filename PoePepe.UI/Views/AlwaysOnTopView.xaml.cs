using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using PoePepe.LiveSearch.Api.Trade.Models;
using PoePepe.UI.Helpers;
using PoePepe.UI.Mapping;
using PoePepe.UI.Properties;
using PoePepe.UI.Services;
using PoePepe.UI.ViewModels;
using Serilog;

namespace PoePepe.UI.Views;

public partial class AlwaysOnTopView
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_NOACTIVATE = 134217728;
    private const int LSFW_LOCK = 1;

    private readonly DispatcherTimer _dispatcherTimer;
    private readonly SoundService _soundService;

    public AlwaysOnTopView()
    {
        InitializeComponent();
        
        DataContext = App.Current.Services.GetService<AlwaysOnTopViewModel>();

        ((AlwaysOnTopViewModel)DataContext!).NotifyItem = async result =>
        {
            await Dispatcher.InvokeAsync(() => Notify(result));
        };
        
        _soundService = App.Current.Services.GetService<SoundService>();
        
        Loaded += OnLoaded;
        
        UserSettings.Default.SettingsSaving += DefaultOnSettingsSaving;

        _dispatcherTimer = new DispatcherTimer();
        _dispatcherTimer.Tick += CheckIfPoeIsForegroundWindow;
        _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        if (UserSettings.Default.HideIfPoeUnfocused)
        {
            _dispatcherTimer.Start();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SetNoActiveWindow();

        Top = UserSettings.Default.NotificationPositionTop;
        Left = UserSettings.Default.NotificationPositionLeft;
    }

    private void DefaultOnSettingsSaving(object sender, CancelEventArgs e)
    {
        if (UserSettings.Default.HideIfPoeUnfocused)
        {
            if (!_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Start();
            }

            return;
        }

        _dispatcherTimer.Stop();
        Show();
    }

    private void SetNoActiveWindow()
    {
        var helper = new WindowInteropHelper(this);
        WindowsInternalFeatureService.SetWindowLong(helper.Handle, GWL_EXSTYLE, WS_EX_NOACTIVATE);
        WindowsInternalFeatureService.LockSetForegroundWindow(LSFW_LOCK);
    }

    private void CheckIfPoeIsForegroundWindow(object sender, EventArgs eventArgs)
    {
        if (WindowsInternalFeatureService.GetForegroundWindow() != WindowsInternalFeatureService.FindPoeGameWindow())
            Hide();
        else
            Show();
    }

    private void Notify(FetchResponseResult result)
    {
        try
        {
            var vm = new OrderItemNotificationViewModel();

            var orderItem = result.ToOrderItemDto();
            vm.SetOrderItem(orderItem);
            vm.SetOwnerView(this);
            var uc = new OrderItemNotificationView(vm)
            {
                DataContext = vm
            };
            uc.Loaded += UcOnLoaded;
            uc.HorizontalAlignment = HorizontalAlignment.Left;
            NotificationStackPanel.Children.Add(uc);
            NotificationStackPanel.Height += 25;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error notifying of found item by order {OrderName}", result.OrderName);
        }
    }

    private void UcOnLoaded(object sender, RoutedEventArgs e)
    {
        _soundService.Play();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        UserSettings.Default.NotificationPositionTop = Top;
        UserSettings.Default.NotificationPositionLeft = Left;

        ModifyGrid.Visibility = Visibility.Collapsed;
    }

    private void ModifyGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}