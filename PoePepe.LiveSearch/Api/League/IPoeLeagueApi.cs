using Refit;

namespace PoePepe.LiveSearch.Api.League;

[Headers("x-requested-with: XMLHttpRequest", "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36")]
public interface IPoeLeagueApi
{
    /// <summary>
    /// Получить список актуальных лиг.
    /// </summary>
    [Get("/league")]
    Task<ApiResponse<LeaguesResponse>> GetLeaguesAsync([Query] string realm, CancellationToken token);
}