using Skreet2k.Common.Models;

namespace PoePepe.LiveSearch.Api.League;

public class PoeLeagueApiService
{
    private const string Realm = "pc";
    private readonly IPoeLeagueApi _poeLeagueApi;

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