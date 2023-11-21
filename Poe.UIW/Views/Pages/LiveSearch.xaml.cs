using System;
using System.Windows;
using System.Windows.Controls;
using Poe.LiveSearch.Models;
using Poe.UIW.ViewModels;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls.Interfaces;

namespace Poe.UIW.Views.Pages;

public partial class LiveSearch : INavigableView<LiveSearchViewModel>
{
    public LiveSearch(LiveSearchViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.DialogControl.ButtonRightClick += DialogControlOnButtonRightClick;
        ViewModel.DialogControl.ButtonLeftClick += DialogControlOnButtonRightClick;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.DialogControl.ButtonRightClick -= DialogControlOnButtonRightClick;
        ViewModel.DialogControl.ButtonLeftClick -= DialogControlOnButtonRightClick;
    }
    
    private static void DialogControlOnButtonRightClick(object sender, RoutedEventArgs e)
    {
        var dialogControl = (IDialogControl)sender;
        dialogControl.Hide();
    }

    public LiveSearchViewModel ViewModel { get; }
    
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
        
        if (DataContext is not LiveSearchViewModel context)
        {
            return;
        }

        if (cmb.DataContext is not OrderViewModel orderViewModel)
        {
            return;
        }

        var newMod = Enum.Parse<OrderMod>(e.AddedItems[0].ToString());
        var oldMod = Enum.Parse<OrderMod>(e.RemovedItems[0].ToString());

        context.ChangeOrderMod(orderViewModel.Id, newMod, oldMod);
    }
    
    protected override void OnInitialized(EventArgs eventArgs)
    {
        ItemImagePanel.Visibility = Visibility.Collapsed;

        base.OnInitialized(eventArgs);
    }
}