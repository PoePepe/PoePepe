using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Serilog;

namespace Poe.UI.Services;

public class ResourceDownloadService
{
    private readonly HashSet<string> _downloadedResources = new();
    private readonly HttpClient _httpClient;

    private const string DivinationCardCdnUrl = "https://web.poecdn.com/image/divination-card/{0}.png";
    public ResourceDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        var path = @".\images";
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

    public async Task<Bitmap> DownloadDivinationCardImageAsync(string cardName)
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

        var bytes = await response.Content.ReadAsStreamAsync();

        await using var file = File.Create($"images/{cardName}");
        await bytes.CopyToAsync(file);
        file.Close();

        bytes.Position = 0;
        var bitMapImage = new Bitmap(bytes);

        return bitMapImage;
    }

    public async Task<Bitmap> DownloadItemImageAsync(string url)
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

            var bytes = await response.Content.ReadAsStreamAsync();

            await using var file = File.Create($"images/{imageName}");
            await bytes.CopyToAsync(file);
            file.Close();

            bytes.Position = 0;
            var bitMapImage = new Bitmap(bytes);

            return bitMapImage;
        }
        catch (Exception e)
        {
            Log.Error(e, e.Message);

            return null;
        }
    }
    
    private bool TryGetCachedImage(string value, out Bitmap bitmap)
    {
        if (!_downloadedResources.Add(value))
        {
            var fileStream = File.OpenRead($"images/{value}");
            bitmap = new Bitmap(fileStream);
            return true;
        }

        bitmap = null;
        return false;
    }
}