using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Poe.UIW.Models;
using UniversalWPF;
using Image = System.Windows.Controls.Image;

namespace Poe.UIW.Services;

public static class ItemSocketImageFactory
{
    private static CachedBitmap SocketCachedBitmap =
        new CachedBitmap(new BitmapImage(ItemSocketImagePosition.SocketUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);
    
    public static IEnumerable<Image> CreateSockets(Sockets sockets)
    {
        var socketImages = new List<Image>(sockets.Count);

        foreach (var socketsGroup in sockets.Groups)
        {
            socketImages.AddRange(socketsGroup.Sockets.Select(x => CreateSocket(x.Color)));

            foreach (var socket in socketsGroup.Sockets)
            {
                if (sockets.IsVertical)
                {
                    SetSocketPositionVertical(socket.OrdinalNumber, socketImages);
                    continue;
                }

                SetSocketPosition(socket.OrdinalNumber, socketImages);
            }
        }

        return socketImages;
    }
    
    public static IEnumerable<Image> CreateLinks(Sockets sockets)
    {
        var linkImages = new List<Image>(sockets.Count);

        foreach (var socketsGroup in sockets.Groups)
        {
            linkImages.AddRange(socketsGroup.Sockets.SkipLast(1).Select(x => CreateLink(x.OrdinalNumber, sockets.IsVertical)));
        }

        return linkImages;
    }
    
    private static Image CreateLink(int socketOrdinalNumber, bool isVertical = false)
    {
        var linkPosition = GetLinkPosition(socketOrdinalNumber, isVertical);

        var linkRect = GetLinkRect(socketOrdinalNumber, isVertical);
            
        var link = new Image
        {
            Source = new CroppedBitmap(SocketCachedBitmap, linkRect),
            Height = linkRect.Height,
            Width = linkRect.Width
        };
        
        Canvas.SetLeft(link, linkPosition.Left);
        Canvas.SetTop(link, linkPosition.Top);

        return link;
    }
    
    private static Image CreateSocket(SocketColor color)
    {
        var rect = color switch
        {
            SocketColor.Red => ItemSocketImagePosition.RedSocket,
            SocketColor.Green => ItemSocketImagePosition.GreenSocket,
            SocketColor.Blue => ItemSocketImagePosition.BlueSocket,
            SocketColor.White => ItemSocketImagePosition.WhiteSocket,
            SocketColor.Abyss => ItemSocketImagePosition.AbyssSocket,
            SocketColor.Delve => ItemSocketImagePosition.DelveSocket,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };

        var socket = new Image
        {
            Source = new CroppedBitmap(SocketCachedBitmap, rect),
            Margin = new Thickness(6),
            Width = ItemSocketImagePosition.SocketSizeWidth,
        };

        return socket;
    }
    
    private static void SetSocketPosition(int socketOrdinalNumber, IReadOnlyList<Image> sockets)
    {
        switch (socketOrdinalNumber)
        {
            case 2:
                RelativePanel.SetRightOf(sockets[1], sockets[0]);
                break;
            case 3:
                RelativePanel.SetBelow(sockets[2], sockets[1]);
                RelativePanel.SetAlignRightWith(sockets[2], sockets[1]);
                break;
            case 4:
                RelativePanel.SetBelow(sockets[3], sockets[0]);
                break;
            case 5:
                RelativePanel.SetBelow(sockets[4], sockets[3]);
                break;
            case 6:
                RelativePanel.SetBelow(sockets[5], sockets[2]);
                RelativePanel.SetRightOf(sockets[5], sockets[4]);
                break;
            default:
                return;
        }
    }
    
    private static void SetSocketPositionVertical(int socketOrdinalNumber, IReadOnlyList<Image> sockets)
    {
        switch (socketOrdinalNumber)
        {
            case 2:
                RelativePanel.SetBelow(sockets[1], sockets[0]);
                break;
            case 3:
                RelativePanel.SetBelow(sockets[2], sockets[1]);
                break;
            default:
                return;
        }
    }
    
    private static Int32Rect GetLinkRect(int socketOrdinalNumber, bool isVertical = false)
    {
        if (isVertical)
        {
            return ItemSocketImagePosition.VerticalLink;
        }

        return socketOrdinalNumber switch
        {
            2 => ItemSocketImagePosition.VerticalLink,
            4 => ItemSocketImagePosition.VerticalLink,
            _ => ItemSocketImagePosition.HorizontalLink
        };
    }
    
    private static ControlPosition GetLinkPosition(int socketOrdinalNumber, bool isVertical = false)
    {
        return socketOrdinalNumber switch
        {
            1 when isVertical => ItemSocketImagePosition.VerticalLink1,
            1 => ItemSocketImagePosition.Link1,
            2 when isVertical => ItemSocketImagePosition.VerticalLink2,
            2 => ItemSocketImagePosition.Link2,
            3 => ItemSocketImagePosition.Link3,
            4 => ItemSocketImagePosition.Link4,
            5 => ItemSocketImagePosition.Link5,
            _ => throw new ArgumentOutOfRangeException(nameof(socketOrdinalNumber), socketOrdinalNumber, null)
        };
    }
}