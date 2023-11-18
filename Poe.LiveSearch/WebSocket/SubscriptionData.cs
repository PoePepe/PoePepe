using Poe.LiveSearch.Models;

namespace Poe.LiveSearch.WebSocket
{
    /// <summary>
    /// Данные\состояние подписки.
    /// </summary>
    public class SubscriptionData
    {
        /// <summary>
        /// Код экспотр-события.
        /// </summary>
        public Order Order { get; set; }

        /// <summary>
        /// Источник для токенов отмены подписки на топик и обработки полученных сообщений.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}