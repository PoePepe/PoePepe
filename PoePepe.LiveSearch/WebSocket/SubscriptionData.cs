using PoePepe.LiveSearch.Models;

namespace PoePepe.LiveSearch.WebSocket;

/// <summary>
/// Represents a subscription data object.
/// </summary>
public class SubscriptionData
{
    /// <summary>
    /// Gets or sets the order for the property.
    /// </summary>
    /// <value>The order.</value>
    public Order Order { get; set; }

    /// <summary>
    /// Gets or sets the cancellation token source associated with this object.
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; set; }

    /// <summary>
    /// Represents a LiveSearcherWebSocketClient instance.
    /// </summary>
    public LiveSearcherWebSocketClient Client { get; set; }
}