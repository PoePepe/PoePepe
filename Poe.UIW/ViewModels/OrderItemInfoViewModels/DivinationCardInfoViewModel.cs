using CommunityToolkit.Mvvm.ComponentModel;
using Poe.UIW.Models;
using Poe.UIW.Services;

namespace Poe.UIW.ViewModels.OrderItemInfoViewModels;

public partial class DivinationCardInfoViewModel : OrderItemInfoViewModelBase
{
    [ObservableProperty] private DivinationCardInfo _cardInfo;

    public DivinationCardInfoViewModel()
    {
    }

    public DivinationCardInfoViewModel(WhisperService whisperService) : base(whisperService)
    {
    }

    public override void SetOrderItem(OrderItemDto orderItem)
    {
        if (OrderItem?.Id is null)
        {
            OrderItem = orderItem;
            CardInfo = orderItem.ItemInfo as DivinationCardInfo;
        }
    }
}