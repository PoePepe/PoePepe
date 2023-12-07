using PoePepe.LiveSearch.Models;

namespace PoePepe.LiveSearch.Persistence;

public interface IOrderRepository
{
    /// <summary>
    /// Добавление данных
    /// </summary>
    Order Add(Order entity);

    /// <summary>
    /// Обновление данных.
    /// </summary>
    Order Update(Order entity);

    void UpdateManyMod(IEnumerable<long> orderIds, OrderMod mod);
    void UpdateAllMod(OrderMod mod);

    /// <summary>
    /// Удаление данных.
    /// </summary>
    bool Delete(long id);

    void Clear();

    /// <summary>
    /// Получение данных.
    /// </summary>
    IEnumerable<Order> GetAll();

    Order GetById(long id);
}