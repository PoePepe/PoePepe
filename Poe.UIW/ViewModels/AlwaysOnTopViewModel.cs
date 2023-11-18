using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Services;
using Serilog;

namespace Poe.UIW.ViewModels;

public class AlwaysOnTopViewModel : ViewModelBase
{
    public AlwaysOnTopViewModel(ServiceState serviceState)
    {
        _notificationItemsChannel = serviceState.NotificationItemsChannel.Reader;
        Start(CancellationToken.None);
    }

    public AlwaysOnTopViewModel()
    {
    }

    private readonly ChannelReader<FetchResponseResult> _notificationItemsChannel;
    
    public Func<FetchResponseResult, Task> NotifyItem;

    private void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            while (await _notificationItemsChannel.WaitToReadAsync(token))
            {
                while (_notificationItemsChannel.TryRead(out var fetchResponse) && !token.IsCancellationRequested)
                {
                    await NotifyItem(fetchResponse);
                }
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        Log.Information("Started receiving data from notification channel");
    }
}