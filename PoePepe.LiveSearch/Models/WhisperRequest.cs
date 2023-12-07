using PoePepe.LiveSearch.Api.Trade.Models;

namespace PoePepe.LiveSearch.Models;

public class WhisperRequestData
{
    public WhisperRequestData(WhisperRequest whisperRequest, string orderName)
    {
        WhisperRequest = whisperRequest;
        OrderName = orderName;
    }

    public WhisperRequestData(WhisperRequest whisperRequest, CancellationTokenSource cancellationTokenSource, string orderName)
    {
        WhisperRequest = whisperRequest;
        CancellationTokenSource = cancellationTokenSource;
        OrderName = orderName;
    }

    public WhisperRequest WhisperRequest { get; set; }

    public CancellationTokenSource CancellationTokenSource { get; set; }
    public string OrderName { get; set; }
}