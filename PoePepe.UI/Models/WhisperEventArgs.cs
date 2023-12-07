using System;

namespace PoePepe.UI.Models;

public class WhisperEventArgs : EventArgs
{
    public WhisperEventArgs(string itemId)
    {
        ItemId = itemId;
    }

    public string ItemId { get; set; }
}