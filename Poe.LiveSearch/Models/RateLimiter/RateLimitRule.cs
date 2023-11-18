namespace Poe.LiveSearch.Models.RateLimiter;

public class RateLimitRule
{
    /// <summary>
    /// Название правила.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Максимальное количество запросов за указанный период.
    /// </summary>
    public int MaximumHits { get; set; }

    /// <summary>
    /// Проверенный период. 
    /// </summary>
    /// <remarks>
    /// Отрезок времени действия ограничения.
    /// </remarks>
    /// <remarks>В секундах.</remarks>
    public int TestedPeriod { get; set; }

    /// <summary>
    /// Время ограничения.
    /// </summary>
    /// <remarks>
    /// Если правильно нарушено.
    /// </remarks>
    /// <remarks>В секундах.</remarks>
    public int RestrictedTime { get; set; }
}