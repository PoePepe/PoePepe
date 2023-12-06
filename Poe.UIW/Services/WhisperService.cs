using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Services;
using Poe.UIW.Models;
using Serilog;

namespace Poe.UIW.Services;

public class WhisperService
{
    private readonly ServiceState _serviceState;

    public WhisperService(ServiceState serviceState)
    {
        _serviceState = serviceState;
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
        
        await _serviceState.WhisperItemsChannel.Writer.WriteAsync(new WhisperRequestData(new WhisperRequest(orderItem.WhisperToken), cts, orderItem.OrderName));
    }

    public async Task WhisperAsync(ItemHistoryDto itemHistory)
    {
        if (itemHistory.WhisperMessage is null || itemHistory.WhisperToken is null)
        {
            Log.Error("Data for whisper of order {OrderName} is empty", itemHistory.OrderName);
        }

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(3));
        cts.Token.Register(() => InputKeyboardService.SendInputToPoe(itemHistory.WhisperMessage));

        await _serviceState.WhisperItemsChannel.Writer.WriteAsync(new WhisperRequestData(new WhisperRequest(itemHistory.WhisperToken), cts, itemHistory.OrderName));
    }
}