using Refit;
using Skreet2k.Common.Models;

namespace PoePepe.LiveSearch.Api;

public static class ApiResponseExtensions
{
    public static Result<T> GetResult<T>(this ApiResponse<T> response)
    {
        if (response.Error is not null)
        {
            return new Result<T>
            {
                ErrorMessage = response.Error.Content ?? response.Error.InnerException?.Message ?? response.Error.Message
            };
        }

        if (response.IsSuccessStatusCode || response.Content is not null)
        {
            return new Result<T>(response.Content);
        }

        return new Result<T>
        {
            ErrorMessage = response.ReasonPhrase
        };
    }
}