using System;
using System.Reflection;
using Avalonia;
using Poe.UI.Models;

namespace Poe.UI.Services;

public static class ItemSocketImagePosition
{
    public static readonly Uri SocketUri = new($"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Assets/socket.png");
    public static readonly PixelSize SocketSize = new(35, 35);
    
    private static readonly PixelPoint RedSocketPoint = new(140, 0);
    public static readonly PixelRect RedSocket = new(RedSocketPoint, SocketSize);
    
    private static readonly PixelPoint BlueSocketPoint = new(105, 0);
    public static readonly PixelRect BlueSocket = new(BlueSocketPoint, SocketSize);
    
    private static readonly PixelPoint GreenSocketPoint = new(35, 105);
    public static readonly PixelRect GreenSocket = new(GreenSocketPoint, SocketSize);
    
    private static readonly PixelPoint WhiteSocketPoint = new(140, 35);
    public static readonly PixelRect WhiteSocket = new(WhiteSocketPoint, SocketSize);
    
    private static readonly PixelPoint AbyssSocketPoint = new(0, 70);
    public static readonly PixelRect AbyssSocket = new(AbyssSocketPoint, SocketSize);
    
    private static readonly PixelPoint DelveSocketPoint = new(105, 35);
    public static readonly PixelRect DelveSocket = new(DelveSocketPoint, SocketSize);
    
    private static readonly PixelPoint HorizontalLinkPoint = new(70, 140);
    private static readonly PixelSize HorizontalLinkSize = new(38, 15);
    public static readonly PixelRect HorizontalLink = new(HorizontalLinkPoint, HorizontalLinkSize);
    
    private static readonly PixelPoint VerticalLinkPoint = new(175, 0);
    private static readonly PixelSize VerticalLinkSize = new(15, 38);
    public static readonly PixelRect VerticalLink = new(VerticalLinkPoint, VerticalLinkSize);

    public static readonly ControlPosition Link1 = new(28, 16);
    public static readonly ControlPosition Link2 = new(63, 28);
    public static readonly ControlPosition Link3 = new(28, 63);
    public static readonly ControlPosition Link4 = new(16, 75);
    public static readonly ControlPosition Link5 = new(28, 111);
    
    public static readonly ControlPosition VerticalLink1 = new(16, 28);
    public static readonly ControlPosition VerticalLink2 = new(16, 75);
}