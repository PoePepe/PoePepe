using Poe.LiveSearch.Api.Trade.Models;
using Refit;

namespace Poe.LiveSearch.Api.Trade;

[Headers("x-requested-with: XMLHttpRequest", "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36")]
public interface IPoeTradeApi : IRefitApi
{
    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    [Post("/api/trade/search/{leagueName}/{queryHash}")]
    Task<ApiResponse<SearchResponse>> IsValidSessionAsync(string leagueName, string queryHash, [Header("Cookie")] string poeSessId);

    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    [Post("/api/trade/search/{leagueName}")]
    Task<ApiResponse<SearchResponse>> SearchItemsAsync(string leagueName, [Body] SearchRequest request);
    
    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    [Post("/api/trade/search/{leagueName}/{queryHash}")]
    Task<ApiResponse<SearchResponse>> SearchItemsAsync(string leagueName, string queryHash);
    
    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    [Get("/api/trade/fetch/{itemIds}")]
    Task<ApiResponse<FetchResponse>> FetchItemsAsync(string itemIds, [Query] string queryHash = null);
    
    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    [Post("/api/trade/whisper")]
    Task<ApiResponse<WhisperResponse>> SendWhisperOfferAsync([Body] WhisperRequest request, CancellationToken token);
}