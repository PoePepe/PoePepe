namespace PoePepe.LiveSearch.Services;

/// <summary>
/// Manages the workers for various channels in the service.
/// </summary>
public class WorkerManager
{
    private readonly FoundChannelWorker _foundChannelWorker;
    private readonly HistoryChannelWorker _historyChannelWorker;
    private readonly LiveSearchChannelWorker _liveSearchChannelWorker;
    private readonly OrderStartSearchWorker _orderStartSearchWorker;
    private readonly ServiceState _serviceState;
    private readonly WhisperChannelWorker _whisperChannelWorker;
    private CancellationToken _globalCancellationToken;
    private CancellationTokenSource _workersCancellationTokenSource;

    public WorkerManager(ServiceState serviceState, LiveSearchChannelWorker liveSearchChannelWorker, FoundChannelWorker foundChannelWorker, WhisperChannelWorker whisperChannelWorker, HistoryChannelWorker historyChannelWorker, OrderStartSearchWorker orderStartSearchWorker)
    {
        _serviceState = serviceState;
        _liveSearchChannelWorker = liveSearchChannelWorker;
        _foundChannelWorker = foundChannelWorker;
        _whisperChannelWorker = whisperChannelWorker;
        _historyChannelWorker = historyChannelWorker;
        _orderStartSearchWorker = orderStartSearchWorker;
    }

    /// <summary>
    /// Starts the different workers for performing specific tasks.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to stop the workers.</param>
    /// <remarks>
    /// This method starts the different workers responsible for performing specific tasks in the application.
    /// The workers are run in parallel using the provided cancellation token, which can be used to stop all the workers.
    /// </remarks>
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

    /// <summary>
    /// Restarts all the workers that process various channels and cancel their current operations.
    /// This method is used to restart workers in case of an error or during system initialization.
    /// </summary>
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