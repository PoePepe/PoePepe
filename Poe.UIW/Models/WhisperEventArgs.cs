using System;

namespace Poe.UIW.Models;

public class WhisperEventArgs : EventArgs
{
    public WhisperEventArgs(string itemId)
    {
        ItemId = itemId;
    }

    public string ItemId { get; set; }
}