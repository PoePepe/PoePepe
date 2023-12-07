using PoePepe.LiveSearch.Models;

namespace PoePepe.LiveSearch.Persistence;

public interface IItemHistoryRepository
{
    /// <summary>
    /// Добавление данных
    /// </summary>
    ItemHistory Add(ItemHistory entity);

    ItemHistory Update(ItemHistory entity);

    void Clear();

    /// <summary>
    /// Clear history of order.
    /// </summary>
    void ClearByOrder(long orderId);

    IEnumerable<ItemHistory> GetByOrderId(long orderId, int top, int skip);
    ItemHistory GetByItemId(string itemId);
    ItemHistory GetFullByItemId(string itemId);
}