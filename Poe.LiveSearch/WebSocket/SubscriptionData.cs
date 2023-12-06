using Poe.LiveSearch.Models;

namespace Poe.LiveSearch.WebSocket
{
    /// <summary>
    /// Данные\состояние подписки.
    /// </summary>
    public class SubscriptionData
    {
        public Order Order { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public LiveSearcherWebSocketClient Client { get; set; }
    }
}