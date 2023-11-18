using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Poe.UI.Models;
using Poe.UI.Services;

namespace Poe.UI.ViewModels.OrderItemInfoViewModels;

public abstract partial class OrderItemInfoViewModelBase : ViewModelBase, IModalDialogViewModel, ICloseable
{
    [ObservableProperty] protected OrderItemDto _orderItem;
    private readonly WhisperService _whisperService;

    public OrderItemInfoViewModelBase()
    {
    }
    
    public OrderItemInfoViewModelBase(WhisperService whisperService)
    {
        _whisperService = whisperService;
    }

    public abstract void SetOrderItem(OrderItemDto orderItem);
    
    [RelayCommand]
    public async Task Whisper()
    {
        await _whisperService.WhisperAsync(OrderItem);

        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void Close()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public bool? DialogResult { get; set; }
    public event EventHandler RequestClose;
}
