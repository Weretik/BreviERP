namespace Scheduling.Domain.Tasks;

public static class ScheduledTaskErrors
{
    public const int MaxMessageLength = 500;

    public static Error AlreadyRunning { get; } =
        Error.Conflict("Scheduling.Task.AlreadyRunning", "Задача вже виконується.");

    public static Error AlreadyRequested { get; } =
        Error.Conflict("Scheduling.Task.AlreadyRequested", "Запуск уже заплановано.");

    public static Error StaleRun { get; } =
        Error.Conflict("Scheduling.Task.StaleRun", "Цей запуск задачі вже не поточний.");

    public static Error NextRunInPast { get; } = Error.Validation("Scheduling.Task.NextRunInPast",
        "Наступний запуск не може бути в минулому.", "nextRunAt");

    /// <summary>A message kept for the admin: the first line, at most <see cref="MaxMessageLength"/> characters.</summary>
    public static string Shorten(string message)
    {
        var text = string.IsNullOrWhiteSpace(message) ? "Невідома помилка." : message.Trim();
        return text.Length <= MaxMessageLength ? text : text[..(MaxMessageLength - 1)] + "…";
    }
}
