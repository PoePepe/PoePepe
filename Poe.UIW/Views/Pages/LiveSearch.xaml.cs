﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Poe.LiveSearch.Models;
using Poe.UIW.Properties;
using Poe.UIW.Services;
using Poe.UIW.ViewModels;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls.Interfaces;

namespace Poe.UIW.Views.Pages;

public partial class LiveSearch : INavigableView<LiveSearchViewModel>
{
    public LiveSearch(LiveSearchViewModel viewModel, LeagueService leagueService)
    {
        ViewModel = viewModel;
        _leagueService = leagueService;
        DataContext = viewModel;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        InitializeComponent();

        _leagueService.LeagueNamesLoaded += LeagueNamesLoaded;
    }

    private void LeagueNamesLoaded(object sender, EventArgs e)
    {
        Dispatcher.Invoke(() => { LeagueNameComboBox.ItemsSource = _leagueService.ActualLeagueNames; });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.DialogControl.ButtonRightClick += DialogControlOnButtonRightClick;
        ViewModel.DialogControl.ButtonLeftClick += DialogControlOnButtonRightClick;

        ViewModel.OrdersChanged += OrderAdded;
    }

    private void OrderAdded(object sender, EventArgs e)
    {
        DataGridFilter();
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
    private readonly LeagueService _leagueService;

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
        base.OnInitialized(eventArgs);
    }

    private void MoreButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Wpf.Ui.Controls.Button;
        var contextMenu = button!.ContextMenu;
        contextMenu!.PlacementTarget = button;
        contextMenu.Placement = PlacementMode.Bottom;
        contextMenu.IsOpen = true;
    }

    private void LeagueNameComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox cmb)
        {
            return;
        }

        if (!cmb.IsDropDownOpen)
        {
            return;
        }

        if (e.AddedItems.Count == 0 || e.RemovedItems.Count == 0)
        {
            return;
        }

        var newLeague = e.AddedItems[0].ToString();
        var oldLeague = e.RemovedItems[0].ToString();

        ViewModel.StopSearchingForOrders(oldLeague);
        ViewModel.LoadOrders(newLeague);

        UserSettings.Default.Save();
        UserSettings.Default.Reload();
    }

    private void DataGridFilter()
    {
        var searchText = OrderNameAutoSuggestBox.Text;
        if (string.IsNullOrEmpty(searchText))
        {
            ViewModel.FilteredOrders = ViewModel.Orders.ToList();
        }
        var formattedText = searchText.ToLower().Trim();

        ViewModel.FilteredOrders = ViewModel.Orders
            .Where(order => order.Name.ToLower().Contains(formattedText));
    }

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        DataGridFilter();
    }
}