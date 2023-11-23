using Refit;
using Skreet2k.Common.Models;

namespace Poe.LiveSearch.Api;

public static class ApiResponseExtensions
{
    public static Result<T> GetResult<T>(this ApiResponse<T> response)
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