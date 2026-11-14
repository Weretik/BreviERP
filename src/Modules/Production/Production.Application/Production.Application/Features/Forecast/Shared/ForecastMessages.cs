namespace Production.Application.Features.Forecast.Shared;

internal static class ForecastMessages
{
    public const string NoSettings =
        "Немає параметрів прогнозу: заповніть кількість швачок, хвилини зміни й вартість хвилини в «Параметри ціноутворення».";

    public const string CannotReorder = "Змінювати черговість може начальник цеху або адміністратор.";
    public const string Stale = "Черговість щойно змінили. Оновіть сторінку й спробуйте ще раз.";
    public const string NotInQueue = "Завдання вже немає в черзі пошиття. Оновіть сторінку.";
    public const string Duplicate = "Завдання в черзі повторюється.";
}
