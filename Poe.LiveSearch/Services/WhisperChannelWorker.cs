using System.Threading.Channels;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Models;
using Serilog;

namespace Poe.LiveSearch.Services;

public class WhisperChannelWorker
{
    private readonly ChannelReader<WhisperRequestData> _whisperChannel;
    private readonly PoeTradeApiService _poeTradeApiService;

    public WhisperChannelWorker(ServiceState serviceState, PoeTradeApiService poeTradeApiService)
    {
        _poeTradeApiService = poeTradeApiService;
        _whisperChannel = serviceState.WhisperItemsChannel.Reader;
    }

    public void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            try
            {
                while (await _whisperChannel.WaitToReadAsync(token))
                {
                    while (_whisperChannel.TryRead(out var whisperRequest) && !token.IsCancellationRequested)
                    {
                        await ProcessWhisperItemAsync(whisperRequest, token);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error receiving from channel of whisper requests");
            }

        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        Log.Information("Started receiving data from channel of whisper requests");
    }
    
    private async Task ProcessWhisperItemAsync(WhisperRequestData request, CancellationToken token)
    {
        if (token.IsCancellationRequested || (request.CancellationTokenSource?.Token.IsCancellationRequested ?? false))
        {
            return;
        }

        try
        {
            var whisperResult = await _poeTradeApiService.SendWhisperOfferAsync(request.WhisperRequest,
                request.CancellationTokenSource?.Token ?? default);

            if (whisperResult.IsSuccess)
            {
                request.CancellationTokenSource?.Dispose();
                return;
            }

            Log.Warning("Unsuccessfully whisper request by order {OrderName}", request.OrderName);
        }
        catch (TaskCanceledException e)
        {
            Log.Error(e,"Whisper request by order {OrderName} has been canceled", request.OrderName);
        }
        catch (Exception e)
        {
            Log.Error(e,"Error whisper request by order {OrderName}", request.OrderName);
        }
        
        request.CancellationTokenSource?.Cancel();
    }
}