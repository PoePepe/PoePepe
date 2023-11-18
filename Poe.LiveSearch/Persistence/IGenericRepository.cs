namespace Poe.LiveSearch.Persistence
{
    /// <summary>
    /// Репозиторий для работы с OData
    /// </summary>
    /// S
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Получение данных.
        /// </summary>
        IQueryable<T> Get();

        /// <summary>
        /// Добавление данных
        /// </summary>
        T Add(T entity);

        /// <summary>
        /// Обновление данных.
        /// </summary>
        T Update(T entity);

        /// <summary>
        /// Удаление данных.
        /// </summary>
        bool Delete(long id);
    }
}