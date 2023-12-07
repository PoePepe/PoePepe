using System;
using System.Threading;
using System.Threading.Tasks;
using PoePepe.LiveSearch.Api.Trade.Models;
using PoePepe.LiveSearch.Services;
using Serilog;

namespace PoePepe.UI.ViewModels;

public class AlwaysOnTopViewModel : ViewModelBase
{
    private readonly ServiceState _serviceState;
    private CancellationToken _globalCancellationToken;
    private CancellationTokenSource _workerCancellationTokenSource;

    public Func<FetchResponseResult, Task> NotifyItem;

    public AlwaysOnTopViewModel(ServiceState serviceState)
    {
        _serviceState = serviceState;
    }

    public AlwaysOnTopViewModel()
    {
    }

    public void Restart()
    {
        _workerCancellationTokenSource.Cancel();
        Start(_globalCancellationToken);
    }

    public void Start(CancellationToken token)
    {
        _globalCancellationToken = token;
        _workerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_globalCancellationToken);

        Task.Factory.StartNew(async () =>
        {
            try
            {
                while (await _serviceState.NotificationItemsChannel.Reader.WaitToReadAsync(token))
                {
                    while (_serviceState.NotificationItemsChannel.Reader.TryRead(out var fetchResponse) && !token.IsCancellationRequested)
                    {
                        await NotifyItem(fetchResponse);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Log.Error(e, "Error notify orders queue");
            }

        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        Log.Information("Started receiving data from notification channel");
    }
}