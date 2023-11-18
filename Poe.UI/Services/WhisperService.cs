using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia.Threading;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Services;
using Poe.UI.Models;
using Serilog;

namespace Poe.UI.Services;

public class WhisperService
{
    private readonly ChannelWriter<WhisperRequestData> _whisperChannel;

    public WhisperService(ServiceState serviceState)
    {
        _whisperChannel = serviceState.WhisperItemsChannel.Writer;
    }

    public async Task WhisperAsync(OrderItemDto orderItem)
    {
        if (orderItem.WhisperMessage is null || orderItem.WhisperToken is null)
        {
            Log.Error("Data for whisper of order {OrderName} is empty", orderItem.OrderName);
        }
        
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(3));
        cts.Token.Register(() => InputKeyboardService.SendInputToPoe(orderItem.WhisperMessage));
        
        await _whisperChannel.WriteAsync(new WhisperRequestData(new WhisperRequest(orderItem.WhisperToken), cts, orderItem.OrderName));

        // var dispatcherTimer = new DispatcherTimer();
        // dispatcherTimer.Tick += (_, _) =>
        // {
        //     dispatcherTimer.Stop();
        //     InputKeyboardService.SendInputToPoe(whisperMessage);
        // };
        //
        // dispatcherTimer.Interval = TimeSpan.FromSeconds(3);
        //
        // dispatcherTimer.Start();
    }
}