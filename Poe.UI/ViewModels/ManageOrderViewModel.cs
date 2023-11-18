using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Services;

namespace Poe.UI.ViewModels;

public partial class ManageOrderViewModel : ViewModelBase, IModalDialogViewModel, ICloseable
{
    [ObservableProperty]
    private OrderViewModel _editOrderModel = new();
    
    [ObservableProperty]
    private OrderViewModel _manageOrderModel = new();
    
    [ObservableProperty]
    private bool _isEditing;
    
    [ObservableProperty]
    private string _headerText;


    public ManageOrderViewModel()
    {
        HeaderText = "Add new order";
    }
    
    public void SetOrder(OrderViewModel order)
    {
        if (EditOrderModel?.Id is null or 0L)
        {
            EditOrderModel = order;
            ManageOrderModel.Id = order.Id;
            ManageOrderModel.Name = order.Name;
            ManageOrderModel.Link = order.Link;
            ManageOrderModel.Mod = order.Mod;
            IsEditing = true;
            HeaderText = "Edit order";
        }
    }

    public static IEnumerable<OrderMod> AvailableMods { get; } = new[] { OrderMod.Whisper, OrderMod.Notify };

    [RelayCommand]
    public void Submit()
    {
        ManageOrderModel.ReValidateAllProperties();
        if (ManageOrderModel.HasErrors)
        {
            return;
        }
        
        if (IsEditing)
        {
            EditOrder();

            return;
        }

        AddNewOrder();
    }

    private void EditOrder()
    {
        EditOrderModel.Name = ManageOrderModel.Name;
        EditOrderModel.Mod = ManageOrderModel.Mod;
        
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void AddNewOrder()
    {
        var hash = ManageOrderModel.Link[(ManageOrderModel.Link.LastIndexOf('/') + 1)..];

        ManageOrderModel.QueryHash = hash;
        ManageOrderModel.IsActive = true;

        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    public void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;
}