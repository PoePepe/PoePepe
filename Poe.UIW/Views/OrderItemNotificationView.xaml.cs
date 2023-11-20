using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Poe.UIW.ViewModels;

namespace Poe.UIW.Views;

public partial class OrderItemNotificationView
{
    public OrderItemNotificationView(OrderItemNotificationViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    public OrderItemNotificationViewModel ViewModel;
    
    protected override void OnInitialized(EventArgs eventArgs)
    {
        ViewModel.ClosingRequest += CloseNotificationFromInfoView;

        base.OnInitialized(eventArgs);
    }
    
    private void CloseNotificationFromInfoView(object sender, EventArgs e)
    {
        if (Parent is not StackPanel stackPanel)
        {
            return;
        }

        stackPanel.Children.Remove(this);
            
        if (stackPanel.Parent is Panel panel)
        {
            panel.Height = panel.Children.Count * 53;
            panel.Width = 400;
        }
    }
    
    private void CloseNotificationFromButton(object sender, RoutedEventArgs e)
    {
        if (Parent is not StackPanel stackPanel)
        {
            return;
        }

        stackPanel.Children.Remove(this);
            
        if (stackPanel.Parent is Panel panel)
        {
            panel.Height = stackPanel.Children.Count * 53;
            panel.Width = 400;
        }
    }

    private void NotifyPanel_OnMouseEnter(object sender, MouseEventArgs e)
    {
        NotificationTitle.Visibility = Visibility.Collapsed;
        ActionButtonsPanel.Visibility= Visibility.Visible;
    }

    private void NotifyPanel_OnMouseLeave(object sender, MouseEventArgs e)
    {
        NotificationTitle.Visibility = Visibility.Visible;
        ActionButtonsPanel.Visibility = Visibility.Collapsed;
    }
}