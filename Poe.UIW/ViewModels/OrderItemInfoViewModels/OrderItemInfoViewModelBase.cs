using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Poe.UIW.Models;
using Poe.UIW.Services;

namespace Poe.UIW.ViewModels.OrderItemInfoViewModels;

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
        Whispered?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void Close()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public bool? DialogResult { get; set; }
    public event EventHandler RequestClose;
    public event EventHandler Whispered;
}
