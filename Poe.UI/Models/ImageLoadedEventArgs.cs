using System;
using Avalonia;

namespace Poe.UI.Models;

public class ImageLoadedEventArgs : EventArgs
{
    public ImageLoadedEventArgs(Size size)
    {
        Size = size;
    }

    public Size Size { get; set; }
}