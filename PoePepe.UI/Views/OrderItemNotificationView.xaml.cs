using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PoePepe.UI.ViewModels;

namespace PoePepe.UI.Views;

public partial class OrderItemNotificationView
{
    private int _timeOut;

    private DispatcherTimer _timer;

    public OrderItemNotificationViewModel ViewModel;

    public OrderItemNotificationView(OrderItemNotificationViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += CloseTick;
        _timer.Start();
    }

    private void CloseTick(object sender, EventArgs e)
    {
        _timeOut += 1;
        if (_timeOut != 100)
        {
            ViewModel.TimeOut -= 1;
            return;
        }

        _timer.Stop();
        _timer.Tick -= CloseTick;
        Close();
    }

    protected override void OnInitialized(EventArgs eventArgs)
    {
        ViewModel.ClosingRequest += CloseNotificationFromInfoView;
        ViewModel.InfoClosed += ViewModelOnInfoClosed;

        base.OnInitialized(eventArgs);
    }

    private void ViewModelOnInfoClosed(object sender, EventArgs e)
    {
        _timer.Start();
    }

    private void Close()
    {
        if (Parent is not StackPanel stackPanel)
        {
            return;
        }

        stackPanel.Children.Remove(this);

        if (stackPanel.Parent is Panel panel)
        {
            panel.Height = panel.Children.Count * 25;
            panel.Width = 150;
        }
    }

    private void CloseNotificationFromInfoView(object sender, EventArgs e)
    {
        Close();
    }

    private void CloseNotificationFromButton(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void NotifyPanel_OnMouseEnter(object sender, MouseEventArgs e)
    {
        NotificationTitle.Visibility = Visibility.Collapsed;
        ActionButtonsPanel.Visibility = Visibility.Visible;
    }

    private void NotifyPanel_OnMouseLeave(object sender, MouseEventArgs e)
    {
        NotificationTitle.Visibility = Visibility.Visible;
        ActionButtonsPanel.Visibility = Visibility.Collapsed;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }
}