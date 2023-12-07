using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using PoePepe.UI.Models;
using PoePepe.UI.Services;

namespace PoePepe.UI.ViewModels.OrderItemInfoViewModels;

public partial class OrderItemInfoViewModel : OrderItemInfoViewModelBase
{
    private readonly ResourceDownloadService _resourceDownloadService;
    [ObservableProperty] private bool _hasMiscellaneous;
    [ObservableProperty] private ItemInfo _orderItemInfo;
    [ObservableProperty] private bool _requirementExists;
    [ObservableProperty] private bool _requirementPropertiesExists;

    public OrderItemInfoViewModel()
    {
    }

    public OrderItemInfoViewModel(WhisperService whisperService,
        ResourceDownloadService resourceDownloadService) : base(whisperService)
    {
        _resourceDownloadService = resourceDownloadService;
    }

    public async Task<BitmapImage> LoadItemImageAsync(BitmapImage image) => await _resourceDownloadService.DownloadItemImageAsync(OrderItem.ImageUrl, image);

    public override void SetOrderItem(OrderItemDto orderItem)
    {
        if (OrderItem?.Id is null)
        {
            OrderItem = orderItem;
            OrderItemInfo = orderItem.ItemInfo;

            RequirementPropertiesExists = OrderItemInfo.Requirements?.Length > 0;
            RequirementExists = RequirementPropertiesExists || OrderItemInfo.HasItemLevel;
            HasMiscellaneous = OrderItemInfo.IsCorrupted || OrderItemInfo.IsSplitted ||
                               OrderItemInfo.IsDuplicated || !OrderItemInfo.IsIdentified;
        }
    }
}