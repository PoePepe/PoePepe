using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using PoePepe.UI.Models;
using PoePepe.UI.Services;

namespace PoePepe.UI.ViewModels.OrderItemInfoViewModels;

public abstract partial class OrderItemInfoViewModelBase : ViewModelBase, IModalDialogViewModel, ICloseable
{
    private readonly WhisperService _whisperService;
    [ObservableProperty] private OrderItemDto _orderItem;

    public OrderItemInfoViewModelBase()
    {
    }

    public OrderItemInfoViewModelBase(WhisperService whisperService)
    {
        _whisperService = whisperService;
    }

    public event EventHandler RequestClose;

    public bool? DialogResult { get; set; }

    public abstract void SetOrderItem(OrderItemDto orderItem);

    [RelayCommand]
    private async Task Whisper()
    {
        await _whisperService.WhisperAsync(OrderItem);

        DialogResult = true;
        Whispered?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Close()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler Whispered;
}
