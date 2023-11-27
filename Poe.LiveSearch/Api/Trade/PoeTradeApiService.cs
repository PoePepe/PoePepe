using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Models.RateLimiter;
using Skreet2k.Common.Models;

namespace Poe.LiveSearch.Api.Trade;

/// <summary>
/// This service is used for interacting with the PoeTrade API.
/// </summary>
public class PoeTradeApiService
{
    private readonly IPoeTradeApi _poeTradeApi;

    public PoeTradeApiService(IPoeTradeApi poeTradeApi)
    {
        _poeTradeApi = poeTradeApi;
    }
    
    /// <summary>
    /// Checks if a session is valid.
    /// </summary>
    /// <returns>Boolean value indicating if the session is valid.</returns>
    public async Task<bool> IsValidSessionAsync(string poeSessId)
    {
        var response = await _poeTradeApi.IsValidSessionAsync("Standard", "10w8Sg", poeSessId);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }
        
        if (!response.Headers.TryGetValues(HeaderRateLimitKeys.RateLimitRules, out var rules))
        {
            return false;
        }

        return rules.FirstOrDefault()?.Contains("Account") ?? false;
    }
    
    /// <summary>
    /// Searches for items in a specific league.
    /// </summary>
    /// <param name="leagueName">Name of the league.</param>
    /// <param name="queryHash">The query hash to search for items.</param>
    /// <returns>
    /// Result of SearchResponse that 
    /// represents the search results.
    /// </returns>
    public async Task<Result<SearchResponse>> SearchItemsAsync(string leagueName, string queryHash)
    {
        var response = await _poeTradeApi.SearchItemsAsync(leagueName, queryHash);

        return response.GetResult();
    }

    /// <summary>
    /// Searches for items in a specific league.
    /// </summary>
    /// <param name="leagueName">Name of the league.</param>
    /// <param name="request">The query hash to search for items.</param>
    /// <returns>
    /// Result of SearchResponse that 
    /// represents the search results.
    /// </returns>
    public async Task<Result<SearchResponse>> SearchItemsAsync(string leagueName, SearchRequest request)
    {
        var response = await _poeTradeApi.SearchItemsAsync(leagueName, request);

        return response.GetResult();
    }

    /// <summary>
    /// Fetches specific items.
    /// </summary>
    /// <param name="itemIds">Comma-separated list of item IDs.</param>
    /// <param name="queryHash">Optional: hash of the search query that resulted in the item IDs.</param>
    /// <returns>The fetched items.</returns>
    public async Task<Result<FetchResponse>> FetchItemsAsync(string itemIds, string queryHash = null)
    {
        var response = await _poeTradeApi.FetchItemsAsync(itemIds, queryHash);

        return response.GetResult();
    }

    /// <summary>
    /// Sends a whisper offer.
    /// </summary>
    /// <param name="request">The whisper request containing the offer details.</param>
    /// <param name="token">A handler for cancellation events.</param>
    /// <returns>Response about successfully of the sent whisper offer.</returns>
    public async Task<Result<WhisperResponse>> SendWhisperOfferAsync(WhisperRequest request, CancellationToken token = default)
    {
        var response = await _poeTradeApi.SendWhisperOfferAsync(request, token);

        return response.GetResult();
    }
}