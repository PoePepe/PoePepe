using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Poe.LiveSearch.Models;
using Poe.UI.Models;
using Poe.UI.Services;
using Poe.UI.ViewModels;

namespace Poe.UI.Views;

public partial class MainView : Window
{
    // private readonly AlwaysOnTopView _alwaysOnTopWindow;
    public MainView()
    {
        InitializeComponent();
        // _alwaysOnTopWindow = new AlwaysOnTopView();
        
        this.AttachDevTools();
    }

    private void SelectingItemsControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox cmb)
        {
            return;
        }
        
        if (!cmb.IsDropDownOpen)
        {
            return;
        }
        
        if (DataContext is not MainViewModel context)
        {
            return;
        }

        if (cmb.Parent?.Parent?.DataContext is not OrderViewModel orderViewModel)
        {
            return;
        }

        var newMod = Enum.Parse<OrderMod>(e.AddedItems[0].ToString());
        var oldMod = Enum.Parse<OrderMod>(e.RemovedItems[0].ToString());

        context.ChangeOrderMod(orderViewModel.Id, newMod, oldMod);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        
        // _alwaysOnTopWindow.Close();
        base.OnClosing(e);
    }
}