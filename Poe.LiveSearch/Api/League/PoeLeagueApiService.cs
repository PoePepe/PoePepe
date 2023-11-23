using Skreet2k.Common.Models;

namespace Poe.LiveSearch.Api.League;

public class PoeLeagueApiService
{
    private readonly IPoeLeagueApi _poeLeagueApi;
    
    private const string Realm = "pc";

    public PoeLeagueApiService(IPoeLeagueApi poeLeagueApi)
    {
        _poeLeagueApi = poeLeagueApi;
    }

    /// <summary>
    /// Получить список актуальных лиг.
    /// </summary>
    public async Task<Result<LeaguesResponse>> GetLeaguesAsync(CancellationToken token = default)
    {
        var response = await _poeLeagueApi.GetLeaguesAsync(Realm, token);

        return response.GetResult();
    }
}