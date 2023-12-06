namespace Poe.LiveSearch.Services;

public class WorkerManager
{
    private readonly ServiceState _serviceState;
    private readonly LiveSearchChannelWorker _liveSearchChannelWorker;
    private readonly FoundChannelWorker _foundChannelWorker;
    private readonly WhisperChannelWorker _whisperChannelWorker;
    private readonly HistoryChannelWorker _historyChannelWorker;
    private readonly OrderStartSearchWorker _orderStartSearchWorker;
    private CancellationTokenSource _workersCancellationTokenSource;
    private CancellationToken _globalCancellationToken;

    public WorkerManager(ServiceState serviceState, LiveSearchChannelWorker liveSearchChannelWorker, FoundChannelWorker foundChannelWorker, WhisperChannelWorker whisperChannelWorker, HistoryChannelWorker historyChannelWorker, OrderStartSearchWorker orderStartSearchWorker)
    {
        _serviceState = serviceState;
        _liveSearchChannelWorker = liveSearchChannelWorker;
        _foundChannelWorker = foundChannelWorker;
        _whisperChannelWorker = whisperChannelWorker;
        _historyChannelWorker = historyChannelWorker;
        _orderStartSearchWorker = orderStartSearchWorker;
    }

    public void StartWorkers(CancellationToken cancellationToken)
    {
        _globalCancellationToken = cancellationToken;
        _workersCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _liveSearchChannelWorker.Start(_workersCancellationTokenSource.Token);
        _foundChannelWorker.Start(_workersCancellationTokenSource.Token);
        _whisperChannelWorker.Start(_workersCancellationTokenSource.Token);
        _historyChannelWorker.Start(_workersCancellationTokenSource.Token);
        _orderStartSearchWorker.Start(_workersCancellationTokenSource.Token);
    }

    public void RestartWorkers()
    {
        _workersCancellationTokenSource.Cancel();
        _serviceState.ClearOrderProcessingChannels();
        _workersCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_globalCancellationToken);
        _liveSearchChannelWorker.Stop();
        _liveSearchChannelWorker.Start(_workersCancellationTokenSource.Token);
        _foundChannelWorker.Start(_workersCancellationTokenSource.Token);
        _whisperChannelWorker.Start(_workersCancellationTokenSource.Token);
        _orderStartSearchWorker.Start(_workersCancellationTokenSource.Token);
    }
}