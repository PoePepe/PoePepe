using Poe.LiveSearch.Models;

namespace Poe.LiveSearch.Persistence;

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