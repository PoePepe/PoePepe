using System.Collections.Generic;
using System.Linq;
using Poe.LiveSearch.Models;
using Poe.UIW.Models;

namespace Poe.UIW.Mapping;

public static class ItemHistoryMapper
{
    public static ItemHistoryDto ToItemHistoryDto(this ItemHistory itemHistory)
    {
        return new ItemHistoryDto
        {
            OrderName = itemHistory.ItemData.OrderName,
            ItemId = itemHistory.ItemId,
            NameExists = !string.IsNullOrEmpty(itemHistory.ItemData.Item.Name),
            ItemName = itemHistory.ItemData.Item.Name,
            ItemTypeLine = itemHistory.ItemData.Item.TypeLine,
            IsWhispered = itemHistory.ItemData.IsWhispered,
            WhisperToken = itemHistory.ItemData.Listing.WhisperToken,
            WhisperMessage = itemHistory.ItemData.Listing.WhisperMessage,
            Price = new ItemPrice
            {
                Amount = itemHistory.ItemData.Listing.Price.Amount,
                Currency = itemHistory.ItemData.Listing.Price.Currency
            },
            FoundDate = itemHistory.FoundDate.LocalDateTime
        };
    }
    
    public static IEnumerable<ItemHistoryDto> ToItemHistoryDto(this IEnumerable<ItemHistory> itemHistory)
    {
        return itemHistory.Select(x => x.ToItemHistoryDto());
    }
}