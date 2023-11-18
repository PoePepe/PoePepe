using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Poe.UI.Models;
using Poe.UI.Services;

namespace Poe.UI.ViewModels.OrderItemInfoViewModels;

public partial class OrderItemInfoViewModel : OrderItemInfoViewModelBase
{
    private readonly ResourceDownloadService _resourceDownloadService;
    [ObservableProperty] private ItemInfo _orderItemInfo;
    [ObservableProperty] private bool _requirementsExists;
    [ObservableProperty] private bool _nameExists;

    public Task<Bitmap> ItemImage => LoadItemImageAsync();

    public OrderItemInfoViewModel()
    {
    }

    public OrderItemInfoViewModel(WhisperService whisperService,
        ResourceDownloadService resourceDownloadService) : base(whisperService)
    {
        _resourceDownloadService = resourceDownloadService;
    }

    private async Task<Bitmap> LoadItemImageAsync()
    {
        return await _resourceDownloadService.DownloadItemImageAsync(OrderItem.ImageUrl);
    }

    public override void SetOrderItem(OrderItemDto orderItem)
    {
        if (OrderItem?.Id is null)
        {
            OrderItem = orderItem;
            OrderItemInfo = orderItem.ItemInfo as ItemInfo;
            RequirementsExists = OrderItemInfo?.Requirements?.Length > 0;
            NameExists = !string.IsNullOrEmpty(OrderItem.Name);
        }
    }
}