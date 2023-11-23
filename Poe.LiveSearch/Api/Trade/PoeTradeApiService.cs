using Microsoft.Extensions.Options;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Services;
using Refit;
using Skreet2k.Common.Models;

namespace Poe.LiveSearch.Api.Trade;

public class PoeTradeApiService
{
    private readonly IPoeTradeApi _poeTradeApi;
    private readonly ServiceState _serviceState;

    public PoeTradeApiService(IPoeTradeApi poeTradeApi, ServiceState serviceState)
    {
        _poeTradeApi = poeTradeApi;
        _serviceState = serviceState;
    }
    
    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    public async Task<Result<SearchResponse>> SearchItemsAsync(string queryHash)
    {
        var response = await _poeTradeApi.SearchItemsAsync(_serviceState.LeagueName, queryHash);

        return response.GetResult();
    }

    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    public async Task<Result<SearchResponse>> SearchItemsAsync(SearchRequest request)
    {
        var response = await _poeTradeApi.SearchItemsAsync(_serviceState.LeagueName, request);

        return response.GetResult();
    }

    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    public async Task<Result<FetchResponse>> FetchItemsAsync(string itemIds, string queryHash = null)
    {
        var response = await _poeTradeApi.FetchItemsAsync(itemIds, queryHash);

        return response.GetResult();
    }

    /// <summary>
    /// Создание связи по ключу.
    /// </summary>
    public async Task<Result<WhisperResponse>> SendWhisperOfferAsync(WhisperRequest request, CancellationToken token = default)
    {
        var response = await _poeTradeApi.SendWhisperOfferAsync(request, token);

        return response.GetResult();
    }
}