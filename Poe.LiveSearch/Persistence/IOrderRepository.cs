using Poe.LiveSearch.Models;

namespace Poe.LiveSearch.Persistence;

public interface IOrderRepository : IGenericRepository<Order>
{
    /// <summary>
    /// Получение данных.
    /// </summary>
    IEnumerable<Order> GetAll();
    Order? GetById(long id);
    void Clear();
}