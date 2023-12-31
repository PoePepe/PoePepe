﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Serilog;

namespace PoePepe.UI.Services;

public class ResourceDownloadService
{
    private const string DivinationCardCdnUrl = "https://web.poecdn.com/image/divination-card/{0}.png";
    private readonly HashSet<string> _downloadedResources = new();
    private readonly HttpClient _httpClient;

    public ResourceDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        var path = @".\Resources\Images";
        var directoryInfo = new DirectoryInfo(path);

        if (!directoryInfo.Exists)
        {
            Directory.CreateDirectory(path);
            return;
        }
        
        var fileNames = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        foreach (var fileName in fileNames)
        {
            _downloadedResources.Add(fileName.Name);
        }
    }

    public async Task<BitmapImage> DownloadDivinationCardImageAsync(string cardName)
    {
        if (TryGetCachedImage(cardName, out var cachedBitMap))
        {
            return cachedBitMap;
        }

        var url = string.Format(DivinationCardCdnUrl, cardName);
        
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            Log.Error(response.ReasonPhrase);

            return null;
        }

        var stream = await response.Content.ReadAsStreamAsync();

        await using var file = File.Create($"Resources/Images/{cardName}");
        await stream.CopyToAsync(file);
        file.Close();

        stream.Position = 0;
        
        var bitMapImage = new BitmapImage();
        bitMapImage.BeginInit();
        bitMapImage.StreamSource = stream;
        bitMapImage.EndInit();

        return bitMapImage;
    }

    public async Task<BitmapImage> DownloadItemImageAsync(string url, BitmapImage image)
    {
        try
        {
            var lastSlashIndex = url.LastIndexOf('/') + 1;
            var imageName = url.Substring(lastSlashIndex);
            if (TryGetCachedImage(imageName, image, out var cachedBitMap))
            {
                return cachedBitMap;
            }
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error(response.ReasonPhrase);

                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();

            await using var file = File.Create($"Resources/Images/{imageName}");
            await stream.CopyToAsync(file);
            file.Close();

            stream.Position = 0;
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            return image;
        }
        catch (Exception e)
        {
            Log.Error(e, e.Message);

            return null;
        }
    }

    public async Task<BitmapImage> DownloadItemImageAsync(string url)
    {
        try
        {
            var lastSlashIndex = url.LastIndexOf('/') + 1;
            var imageName = url.Substring(lastSlashIndex);
            if (TryGetCachedImage(imageName, out var cachedBitMap))
            {
                return cachedBitMap;
            }
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error(response.ReasonPhrase);

                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();

            await using var file = File.Create($"Resources/Images/{imageName}");
            await stream.CopyToAsync(file);
            file.Close();

            stream.Position = 0;
            var bitMapImage = new BitmapImage();
            bitMapImage.BeginInit();
            bitMapImage.StreamSource = stream;
            bitMapImage.EndInit();

            return bitMapImage;
        }
        catch (Exception e)
        {
            Log.Error(e, e.Message);

            return null;
        }
    }

    private bool TryGetCachedImage(string value, BitmapImage bitmap, out BitmapImage outBitmap)
    {
        if (!_downloadedResources.Add(value))
        {
            var fileStream = File.OpenRead($"Resources/Images/{value}");
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = fileStream;
            bitmap.EndInit();
            outBitmap = bitmap;
            return true;
        }

        outBitmap = bitmap;
        return false;
    }

    private bool TryGetCachedImage(string value, out BitmapImage outBitmap)
    {
        if (!_downloadedResources.Add(value))
        {
            var fileStream = File.OpenRead($"Resources/Images/{value}");
            outBitmap = new BitmapImage();
            outBitmap.BeginInit();
            outBitmap.StreamSource = fileStream;
            outBitmap.EndInit();
            return true;
        }

        outBitmap = null;
        return false;
    }
}