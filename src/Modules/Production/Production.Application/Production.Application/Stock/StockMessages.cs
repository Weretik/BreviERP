namespace Production.Application.Stock;

internal static class StockMessages
{
    public const string CannotMove = "Рух складу записує начальник цеху або адміністратор.";
    public const string LineNotFound = "Рядок складу не знайдено.";
    public const string MovementNotFound = "Рух не знайдено.";
    public const string Stale = "Склад уже змінили. Оновіть сторінку.";
    public const string FabricNotFound = "Тканину не знайдено в «Тканини».";
    public const string FabricWithoutCode = "У тканини немає коду FAB-: додайте його в «Тканини».";
    public const string SupplierNotFound = "Постачальника не знайдено.";
    public const string OpeningNotFirst = "Початковий залишок — лише перший рух рядка. Використайте «Коригування».";
    public const string NothingToCorrect = "Пораховано стільки ж, скільки на складі.";
    public const string CannotReverse = "Цей рух не можна сторнувати: видачу сторнують у завданні.";
    public const string AlreadyReversed = "Рух уже сторновано.";
    public const string CannotIssue = "Видає тканину начальник цеху або адміністратор у новому завданні.";
    public const string PlanPartNotFound = "У плані позиції немає такого кольору.";
    public const string PlannedMismatch = "Планова видача — це планова тканина й колір. Для іншої оберіть «Заміна».";
    public const string IssueNotFound = "Видачу не знайдено.";
    public const string IssueAlreadyReversed = "Видачу вже сторновано.";
    public const string TooMuch = "Забагато метрів.";
    public const string Meters = "Метри — число з двома знаками після коми, більше нуля.";
    public const string Note = "Вкажіть причину.";
    public const string CannotImport = "Переносити склад може лише адміністратор.";
}
