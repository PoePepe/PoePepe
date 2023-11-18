using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Poe.UI.Models;
using Poe.UI.ViewModels;

namespace Poe.UI.Views;

public partial class OrderItemNotificationView : UserControl
{
    public OrderItemNotificationView()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        ((OrderItemNotificationViewModel)DataContext!).ClosingRequest += CloseNotificationFromInfoView;

        base.OnInitialized();
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