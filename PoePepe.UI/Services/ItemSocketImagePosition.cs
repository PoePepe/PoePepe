using System;
using System.Windows;
using PoePepe.UI.Models;

namespace PoePepe.UI.Services;

public static class ItemSocketImagePosition
{
    public const double SocketSizeWidth = 35;
    public static readonly Uri SocketUri = new("pack://application:,,,/Resources/Images/socket.png");

    public static readonly Int32Rect RedSocket = new(140, 0, 35, 35);

    public static readonly Int32Rect BlueSocket = new(105, 0, 35, 35);

    public static readonly Int32Rect GreenSocket = new(35, 105, 35, 35);

    public static readonly Int32Rect WhiteSocket = new(140, 35, 35, 35);

    public static readonly Int32Rect AbyssSocket = new(0, 70, 35, 35);

    public static readonly Int32Rect DelveSocket = new(105, 35, 35, 35);

    public static readonly Int32Rect HorizontalLink = new(70, 140, 38, 15);

    public static readonly Int32Rect VerticalLink = new(175, 0, 15, 38);

    public static readonly ControlPosition Link1 = new(28, 16);
    public static readonly ControlPosition Link2 = new(63, 28);
    public static readonly ControlPosition Link3 = new(28, 63);
    public static readonly ControlPosition Link4 = new(16, 75);
    public static readonly ControlPosition Link5 = new(28, 111);

    public static readonly ControlPosition VerticalLink1 = new(16, 28);
    public static readonly ControlPosition VerticalLink2 = new(16, 75);
}