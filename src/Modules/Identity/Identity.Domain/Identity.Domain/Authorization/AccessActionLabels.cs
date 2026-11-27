namespace Identity.Domain.Authorization;

/// <summary>
/// What each action means on one page of «Права доступу», in Ukrainian (ACC-002 R-05), such as «Запускати зараз» for
/// «Створення» on «Фонові задачі». An action without its own name gets the general one.
/// </summary>
public readonly record struct AccessActionLabels(
    string? View = null,
    string? Create = null,
    string? Edit = null,
    string? Delete = null)
{
    public const string GeneralView = "Бачить";
    public const string GeneralCreate = "Створювати";
    public const string GeneralEdit = "Редагувати";
    public const string GeneralDelete = "Видаляти";

    public string For(AccessAction action) => action switch
    {
        AccessAction.View => View ?? GeneralView,
        AccessAction.Create => Create ?? GeneralCreate,
        AccessAction.Edit => Edit ?? GeneralEdit,
        AccessAction.Delete => Delete ?? GeneralDelete,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "One action at a time.")
    };
}
