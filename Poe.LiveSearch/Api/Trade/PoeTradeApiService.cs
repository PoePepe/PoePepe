using Microsoft.Extensions.Options;
using Poe.LiveSearch.Api.Trade.Models;
using Refit;
using Skreet2k.Common.Models;

namespace Poe.LiveSearch.Api.Trade;

public class PoeTradeApiService
{
    private readonly IPoeTradeApi _poeTradeApi;
    private readonly PoeApiOptions _poeApiOptions;

    public PoeTradeApiService(IPoeTradeApi poeTradeApi, IOptions<PoeApiOptions> poeApiOptions)
    {
        _poeTradeApi = poeTradeApi;
        _poeApiOptions = poeApiOptions.Value;
    }
    
    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    public async Task<Result<SearchResponse>> SearchItemsAsync(string queryHash)
    {
        var response = await _poeTradeApi.SearchItemsAsync(_poeApiOptions.LeagueName, queryHash);

        return GetResult(response);
    }

    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    public async Task<Result<SearchResponse>> SearchItemsAsync(SearchRequest request)
    {
        var response = await _poeTradeApi.SearchItemsAsync(_poeApiOptions.LeagueName, request);

        return GetResult(response);
    }

    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    public async Task<Result<FetchResponse>> FetchItemsAsync(string itemIds, string queryHash = null)
    {
        var response = await _poeTradeApi.FetchItemsAsync(itemIds, queryHash);

        return GetResult(response);
    }

    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    public async Task<Result<WhisperResponse>> SendWhisperOfferAsync(WhisperRequest request, CancellationToken token = default)
    {
        var response = await _poeTradeApi.SendWhisperOfferAsync(request, token);

        return GetResult(response);
    }

    private Result<T> GetResult<T>(ApiResponse<T> response)
    {
        if (response.IsSuccessStatusCode || response.Content is not null)
        {
            return new Result<T>(response.Content);
        }

        if (response.Error?.Content is not null)
        {
            return new Result<T>
            {
                ErrorMessage = response.Error.Content
            };
        }

        return new Result<T>
        {
            ErrorMessage = response.ReasonPhrase
        };
    }
}