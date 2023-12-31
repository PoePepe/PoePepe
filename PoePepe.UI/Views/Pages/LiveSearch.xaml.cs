﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PoePepe.LiveSearch.Models;
using PoePepe.UI.Helpers;
using PoePepe.UI.Properties;
using PoePepe.UI.Services;
using PoePepe.UI.ViewModels;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls.Interfaces;
using Button = Wpf.Ui.Controls.Button;

namespace PoePepe.UI.Views.Pages;

public partial class LiveSearch : INavigableView<LiveSearchViewModel>
{
    private readonly LeagueService _leagueService;

    public LiveSearch(LiveSearchViewModel viewModel, LeagueService leagueService)
    {
        ViewModel = viewModel;
        _leagueService = leagueService;
        DataContext = viewModel;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        _leagueService.LeagueNamesLoaded += LeagueNamesLoaded;

        InitializeComponent();
    }

    public LiveSearchViewModel ViewModel { get; }

    private void LeagueNamesLoaded(object sender, EventArgs e)
    {
        Dispatcher.Invoke(() => { LeagueNameComboBox.ItemsSource = _leagueService.ActualLeagueNames; });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.DialogControl.ButtonRightClick += DialogControlOnButtonRightClick;
        ViewModel.DialogControl.ButtonLeftClick += DialogControlOnButtonRightClick;

        ViewModel.FilteredOrdersChanged += OrderAdded;

        LeagueNameComboBox.ItemsSource = _leagueService.ActualLeagueNames;
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

        if (e.AddedItems.Count == 0 || e.RemovedItems.Count == 0)
        {
            return;
        }

        var newMod = Enum.Parse<OrderMod>(e.AddedItems[0].ToString());
        var oldMod = Enum.Parse<OrderMod>(e.RemovedItems[0].ToString());

        context.ChangeOrderMod(orderViewModel.Id, newMod, oldMod);
    }

    private void MoreButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
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

        ViewModel.ChangeLeagueSearchingOrders();

        UserSettings.Default.Save();
    }

    private void DataGridFilter()
    {
        var searchText = OrderNameAutoSuggestBox.Text;
        if (string.IsNullOrEmpty(searchText))
        {
            ViewModel.FilteredOrders = new ObservableCollection<OrderViewModel>(ViewModel.Orders.Sort(ViewModel.ActualSort));
        }

        var formattedText = searchText.ToLower().Trim();

        ViewModel.FilteredOrders = new ObservableCollection<OrderViewModel>(ViewModel.Orders
            .Where(order => order.Name.ToLower().Contains(formattedText))
            .Sort(ViewModel.ActualSort));
    }

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        DataGridFilter();
    }

    private void SortComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
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

        ViewModel.FilteredOrders = new ObservableCollection<OrderViewModel>(ViewModel.FilteredOrders.Sort(ViewModel.ActualSort));

        UserSettings.Default.LiveSearchSort = ViewModel.ActualSort.Kind.ToString();
        UserSettings.Default.Save();
    }

    private void SelectedModElement_OnSelected(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBlock { DataContext: OrderMod mod })
        {
            return;
        }

        var selectedOrders = ViewModel.Orders.Where(x => x.IsSelected);
        ViewModel.UpdateManyOrderMod(selectedOrders.Select(x => x.Id), mod);

        if (ViewModel.IsSelectedAllOrders)
        {
            ViewModel.UpdateAllOrderMod(mod);
        }
        
        foreach (var viewModelOrder in selectedOrders)
        {
            viewModelOrder.Mod = mod;
        }
    }

    private void DataGrid_LifeSearch_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid { SelectedValue: OrderViewModel order })
        {
            order.IsSelected = !order.IsSelected;
        }
    }

    private void ToggleRow_OnChecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { DataContext: OrderViewModel order })
        {
            order.IsSelected = true;

            ViewModel.IsRowsSelected = true;
        }
    }

    private void ToggleRow_OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { DataContext: OrderViewModel order })
        {
            order.IsSelected = false;

            ViewModel.IsRowsSelected = ViewModel.IsSelectedAllOrders || ViewModel.Orders.Any(x => x.IsSelected);
        }
    }

    private void ToggleHeader_OnChecked(object sender, RoutedEventArgs e)
    {
        foreach (var viewModelOrder in ViewModel.Orders)
        {
            viewModelOrder.IsSelected = true;
        }

        ViewModel.IsRowsSelected = true;
    }

    private void ToggleHeader_OnUnchecked(object sender, RoutedEventArgs e)
    {
        foreach (var viewModelOrder in ViewModel.Orders)
        {
            viewModelOrder.IsSelected = false;
        }

        ViewModel.IsRowsSelected = false;
    }

    private void OptionsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var contextMenu = button!.ContextMenu;
        contextMenu!.PlacementTarget = button;
        contextMenu.Placement = PlacementMode.Bottom;
        contextMenu.IsOpen = true;
    }
}