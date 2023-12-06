﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Services;
using Poe.UIW.Helpers;
using Poe.UIW.Mapping;
using Poe.UIW.Models;
using Poe.UIW.Properties;
using Serilog;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace Poe.UIW.ViewModels;

public partial class ImportOrdersViewModel : ViewModelValidatableBase, IModalDialogViewModel, ICloseable
{
    private readonly ISnackbarService _snackbarService;
    private readonly IDialogService _dialogService;
    private LiveSearchViewModel _liveSearchViewModel;
    private readonly Service _service;

    [ObservableProperty] private string _importText;

    public ImportOrdersViewModel(IDialogService dialogService, Service service, ISnackbarService snackbarService)
    {
        _dialogService = dialogService;
        _service = service;
        _snackbarService = snackbarService;
    }

    public void SetOwnerViewModel(LiveSearchViewModel viewModel)
    {
        _liveSearchViewModel = viewModel;
    }

    [RelayCommand]
    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(ImportText))
        {
            return;
        }

        try
        {
            if (!TryParseImportData(ImportText, out var orders))
            {
                SetCommonValidationError("Data parsing failed. Please check the format and try again.");

                return;
            }

            if (!orders.Any())
            {
                SetCommonValidationError("No items found for import");

                return;
            }

            ImportOrders(orders.ToArray());
        }
        catch (Exception e)
        {
            SetCommonValidationError("Data importing failed.");

            Log.Error(e, "Data importing failed. ImportText: {ImportText}", ImportText);
        }
    }

    [RelayCommand]
    private async Task ImportFromFile()
    {
        var importFile = await _dialogService.OpenImportFileAsync(this);

        await using var fileStream = await importFile.OpenReadAsync();
        using var streamReader = new StreamReader(fileStream);
        var importData = await streamReader.ReadLineAsync();

        ImportText = importData ?? "";
        
        ClearCommonValidationError();
    }

    private bool TryParseImportData(string data, out OrderImportDto[] orders)
    {
        if (data.StartsWith("poepepe:"))
        {
            var poePepeData = data[8..];

            var encodedDataBytes = Convert.FromBase64String(poePepeData);
            poePepeData = System.Text.Encoding.UTF8.GetString(encodedDataBytes);

            orders = JsonSerializer.Deserialize<OrderImportDto[]>(poePepeData);

            return true;
        }

        if (data.StartsWith("2:"))
        {
            var lastIndexBase64 = data.LastIndexOf('=') + 1;
            var otherFormatData = data[2..lastIndexBase64];

            var encodedDataBytes = Convert.FromBase64String(otherFormatData);
            otherFormatData = System.Text.Encoding.UTF8.GetString(encodedDataBytes);

            var betterTradingItems = JsonSerializer.Deserialize<BetterTradingItems>(otherFormatData);

            orders = betterTradingItems.Items.Select(x => new OrderImportDto
            {
                QueryHash = x.Query[7..],
                Name = x.Title
            }).ToArray();

            return true;
        }

        orders = null;

        return false;
    }

    private void ImportOrders(OrderImportDto[] items)
    {
        var lastId = !_liveSearchViewModel.Orders.Any() ? 1 : _liveSearchViewModel.Orders.MaxBy(x => x.Id).Id;
        var uniqueItems = items
            .Where(i => _liveSearchViewModel.Orders.All(o => o.Name != i.Name || o.QueryHash != i.QueryHash)).ToArray();

        foreach (var item in uniqueItems)
        {
            var orderViewModel = new OrderViewModel
            {
                Id = ++lastId,
                CreatedAt = DateTimeOffset.UtcNow,
                QueryHash = item.QueryHash,
                Name = item.Name,
                Activity = OrderActivity.Disabled,
                Mod = OrderMod.Notify,
                IsActive = false
            };

            _liveSearchViewModel.Orders.Add(orderViewModel);
            _liveSearchViewModel.FilteredOrders.Add(orderViewModel);
            _service.CreateOrder(orderViewModel.ToOrder());
        }

        var uniqueItemsCount = uniqueItems.Length;

        if (uniqueItemsCount > 0)
        {
            _liveSearchViewModel.FilteredOrders =
                new ObservableCollection<OrderViewModel>(
                    _liveSearchViewModel.FilteredOrders.Sort(_liveSearchViewModel.ActualSort));
        }

        var importItemsCount = items.Length;

        var skipped = importItemsCount - uniqueItems.Length;

        OpenSnackbarOrderImported(uniqueItemsCount, skipped);

        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void OpenSnackbarOrderImported(int importedCount, int skippedCount)
    {
        var tittle = importedCount > 0
            ? $"Successfully imported {importedCount} orders."
            : "No new orders found.";

        var message = skippedCount > 0
            ? $"Skipped {skippedCount} orders due to a non-unique name or query."
            : "";

        _snackbarService.Show(
            tittle,
            message,
            SymbolRegular.CheckmarkCircle24,
            ControlAppearance.Success
        );
    }

    public bool? DialogResult { get; private set; }
    public event EventHandler RequestClose;
}