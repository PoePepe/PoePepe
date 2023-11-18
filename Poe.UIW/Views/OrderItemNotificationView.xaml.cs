using System;
using System.Windows;
using System.Windows.Controls;
using Poe.UIW.ViewModels;

namespace Poe.UIW.Views;

public partial class OrderItemNotificationView
{
    public OrderItemNotificationView()
    {
        InitializeComponent();
    }
    
    protected override void OnInitialized(EventArgs eventArgs)
    {
        ((OrderItemNotificationViewModel)DataContext!).ClosingRequest += CloseNotificationFromInfoView;

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
}