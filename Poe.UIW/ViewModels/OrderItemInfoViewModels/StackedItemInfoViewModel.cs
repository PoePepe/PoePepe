using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Poe.UIW.Models;
using Poe.UIW.Services;

namespace Poe.UIW.ViewModels.OrderItemInfoViewModels;

public partial class StackedItemInfoViewModel : OrderItemInfoViewModelBase
{
    [ObservableProperty] private StackedItemInfo _stackedItemInfo;
    private readonly ResourceDownloadService _resourceDownloadService;
    // public Task<BitmapImage> ItemImage => LoadItemImageAsync();
    
    public StackedItemInfoViewModel()
    {
    }

    public StackedItemInfoViewModel(WhisperService whisperService, ResourceDownloadService resourceDownloadService) : base(whisperService)
    {
        _resourceDownloadService = resourceDownloadService;
    }
    
    public async Task<BitmapImage> LoadItemImageAsync(BitmapImage image)
    {
        if (OrderItem.ItemType == ItemType.DivinationCard)
        {
            // return new Bitmap(AssetLoader.Open(DivinationCardImagePosition.DivinationCardUri));
        }

        return await _resourceDownloadService.DownloadItemImageAsync(OrderItem.ImageUrl, image);
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