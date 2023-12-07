using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using PoePepe.LiveSearch.Models;
using PoePepe.UI.Properties;

namespace PoePepe.UI.ViewModels;

public partial class ManageOrderViewModel : ViewModelBase, IModalDialogViewModel, ICloseable
{
    [ObservableProperty]
    private OrderViewModel _editOrderModel = new();

    [ObservableProperty]
    private string _headerText;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private OrderViewModel _manageOrderModel = new();

    public ManageOrderViewModel()
    {
        HeaderText = "Add new order";
    }

    public static IEnumerable<OrderMod> AvailableMods { get; } = new[] { OrderMod.Whisper, OrderMod.Notify };
    public event EventHandler RequestClose;

    public bool? DialogResult { get; private set; }

    public void SetOrder(OrderViewModel order)
    {
        if (EditOrderModel?.Id is null or 0L)
        {
            EditOrderModel = order;
            ManageOrderModel.Id = order.Id;
            ManageOrderModel.Name = order.Name;
            ManageOrderModel.Link = $"https://www.pathofexile.com/trade/search/{UserSettings.Default.LeagueName}/{order.QueryHash}";
            ManageOrderModel.Mod = order.Mod;
            IsEditing = true;
            HeaderText = "Edit order";
        }
    }

    [RelayCommand]
    private void Submit()
    {
        ManageOrderModel.ReValidateAllProperties();
        if (ManageOrderModel.HasErrors || ManageOrderModel.HasValidationErrors)
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
        var decodedUrl = HttpUtility.UrlDecode(ManageOrderModel.Link);

        var match = QueryLinkRegex().Match(decodedUrl);
        if (!match.Success)
        {
            return;
        }

        var hash = match.Groups[2].Value;

        ManageOrderModel.QueryHash = hash;
        ManageOrderModel.IsActive = true;

        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [GeneratedRegex(".*/trade/search/([^/]+)/([^/]+)$", RegexOptions.Compiled)]
    private static partial Regex QueryLinkRegex();
}