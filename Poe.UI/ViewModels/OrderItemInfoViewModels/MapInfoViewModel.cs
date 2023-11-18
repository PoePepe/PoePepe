using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Poe.UI.Models;
using Poe.UI.Services;

namespace Poe.UI.ViewModels.OrderItemInfoViewModels;

public partial class MapInfoViewModel : OrderItemInfoViewModelBase
{
    private readonly ResourceDownloadService _resourceDownloadService;
    [ObservableProperty] private MapInfo _orderMapInfo;

    public Task<Bitmap> MapImage => LoadItemImageAsync();

    public MapInfoViewModel()
    {
    }

    public MapInfoViewModel(WhisperService whisperService, ResourceDownloadService resourceDownloadService) : base(whisperService)
    {
        _resourceDownloadService = resourceDownloadService;
    }

    private async Task<Bitmap> LoadItemImageAsync()
    {
        var bitmap = await _resourceDownloadService.DownloadItemImageAsync(OrderItem.ImageUrl);

        return bitmap;
    }

    public override void SetOrderItem(OrderItemDto orderItem)
    {
        if (OrderItem?.Id is null)
        {
            OrderItem = orderItem;
            OrderMapInfo = orderItem.ItemInfo as MapInfo;
        }
    }
}