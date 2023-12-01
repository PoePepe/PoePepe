namespace Poe.LiveSearch.Models.RateLimiter;

public class RateLimitState
{
    /// <summary>
    /// Название состояния правила.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Текущее количество запросов за указанный период.
    /// </summary>
    public int CurrentHitCount { get; set; }
    
    /// <summary>
    /// Проверенный период. 
    /// </summary>
    /// <remarks>
    /// Отрезок времени действия ограничения.
    /// </remarks>
    /// <remarks>В секундах.</remarks>
    public int TestedPeriod { get; set; }
    
    /// <summary>
    /// Длительность ограничения.
    /// </summary>
    /// <remarks>
    /// Равно 0, если правило не нарушено.
    /// </remarks>
    /// <remarks>В секундах.</remarks>
    public int DurationOfTheRestriction { get; set; }
    
    public LinkedList<DateTime> DateHistoryOfRequest { get; set; } = new();
}