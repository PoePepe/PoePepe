using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Poe.UI.Models;
using Poe.UI.Services;

namespace Poe.UI.ViewModels.OrderItemInfoViewModels;

public partial class StackedItemInfoViewModel : OrderItemInfoViewModelBase
{
    [ObservableProperty] private StackedItemInfo _stackedItemInfo;
    private readonly ResourceDownloadService _resourceDownloadService;
    public Task<Bitmap> ItemImage => LoadItemImageAsync();
    
    public StackedItemInfoViewModel()
    {
    }

    public StackedItemInfoViewModel(WhisperService whisperService, ResourceDownloadService resourceDownloadService) : base(whisperService)
    {
        _resourceDownloadService = resourceDownloadService;
    }
    
    private async Task<Bitmap> LoadItemImageAsync()
    {
        if (OrderItem.ItemType == ItemType.DivinationCard)
        {
            return new Bitmap(AssetLoader.Open(DivinationCardImagePosition.DivinationCardUri));
        }

        return await _resourceDownloadService.DownloadItemImageAsync(OrderItem.ImageUrl);
    }

    public override void SetOrderItem(OrderItemDto orderItem)
    {
        if (OrderItem?.Id is null)
        {
            OrderItem = orderItem;
            StackedItemInfo = orderItem.ItemInfo as StackedItemInfo;
        }
    }
}