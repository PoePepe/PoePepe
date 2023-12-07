using System;

namespace PoePepe.UI.Models;

public class ItemHistoryDto
{
    public string OrderName { get; set; }
    public string ItemId { get; set; }
    public bool NameExists { get; set; }
    public string ItemName { get; set; }
    public string ItemTypeLine { get; set; }
    public bool IsWhispered { get; set; }
    public string WhisperToken { get; set; }
    public string WhisperMessage { get; set; }
    public ItemPrice Price { get; set; }
    public DateTime FoundDate { get; set; }
}