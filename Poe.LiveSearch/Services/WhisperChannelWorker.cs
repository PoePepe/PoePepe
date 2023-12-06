using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Models;
using Serilog;

namespace Poe.LiveSearch.Services;

public class WhisperChannelWorker
{
    private readonly ServiceState _serviceState;
    private readonly PoeTradeApiService _poeTradeApiService;

    public WhisperChannelWorker(ServiceState serviceState, PoeTradeApiService poeTradeApiService)
    {
        _serviceState = serviceState;
        _poeTradeApiService = poeTradeApiService;
    }

    public void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            try
            {
                while (await _serviceState.WhisperItemsChannel.Reader.WaitToReadAsync(token))
                {
                    while (_serviceState.WhisperItemsChannel.Reader.TryRead(out var whisperRequest) && !token.IsCancellationRequested)
                    {
                        await ProcessWhisperItemAsync(whisperRequest, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
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

            if (whisperResult.IsSuccess && whisperResult.Content.Success)
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