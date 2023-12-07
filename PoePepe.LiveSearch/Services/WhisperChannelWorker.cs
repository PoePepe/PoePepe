using PoePepe.LiveSearch.Api.Trade;
using PoePepe.LiveSearch.Models;
using Serilog;

namespace PoePepe.LiveSearch.Services;

/// <summary>
/// Represents a worker class for processing whisper requests in a channel.
/// </summary>
public class WhisperChannelWorker
{
    private readonly PoeTradeApiService _poeTradeApiService;
    private readonly ServiceState _serviceState;

    public WhisperChannelWorker(ServiceState serviceState, PoeTradeApiService poeTradeApiService)
    {
        _serviceState = serviceState;
        _poeTradeApiService = poeTradeApiService;
    }

    /// <summary>
    /// Starts the process of receiving whisper requests from a channel.
    /// </summary>
    /// <param name="token">The cancellation token to stop the process.</param>
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

    /// <summary>
    /// Asynchronously processes a Whisper item request.
    /// </summary>
    /// <param name="request">The <see cref="WhisperRequestData"/> object containing the Whisper request details.</param>
    /// <param name="token">The <see cref="CancellationToken"/> used to cancel the request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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