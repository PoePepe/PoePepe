using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Poe.UIW.Models;

namespace Poe.UIW.ViewModels;

public partial class ExportOrdersViewModel : ViewModelBase, IModalDialogViewModel, ICloseable
{
    private readonly IDialogService _dialogService;
    private LiveSearchViewModel _liveSearchViewModel;

    [ObservableProperty] private string _exportText;

    public ExportOrdersViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public void SetOwnerViewModel(LiveSearchViewModel viewModel)
    {
        _liveSearchViewModel = viewModel;
        if (!_liveSearchViewModel.Orders.Any())
        {
            return;
        }

        var exportOrders = _liveSearchViewModel.Orders.Select(x => new OrderExportDto
        {
            QueryHash = x.QueryHash,
            Name = x.Name
        });

        var exportOrdersJson = JsonSerializer.Serialize(exportOrders);
        var exportOrdersBytes = System.Text.Encoding.UTF8.GetBytes(exportOrdersJson);
        var exportOrdersBase64 = Convert.ToBase64String(exportOrdersBytes);

        ExportText = $"poepepe:{exportOrdersBase64}";
    }

    [RelayCommand]
    private void Close()
    {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void CopyExportText()
    {
        if (string.IsNullOrWhiteSpace(ExportText))
        {
            return;
        }

        Clipboard.SetText(ExportText);
    }

    [RelayCommand]
    private async Task ExportToFile()
    {
        if (string.IsNullOrWhiteSpace(ExportText))
        {
            return;
        }

        var exportFolder = await _dialogService.OpenExportFolderAsync("PoePepe_backup");

        var path = exportFolder?.Path?.LocalPath;

        if (path == null)
        {
            return;
        }

        await using var outputFile = new StreamWriter(path);
        await outputFile.WriteLineAsync(ExportText);
        await outputFile.FlushAsync();
    }

    public bool? DialogResult { get; private set; }
    public event EventHandler RequestClose;
}