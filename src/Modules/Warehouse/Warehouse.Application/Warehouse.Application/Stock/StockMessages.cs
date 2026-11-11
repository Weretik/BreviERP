namespace Warehouse.Application.Stock;

internal static class StockMessages
{
    public const string CannotMove = "Рух складу записує начальник цеху або адміністратор.";
    public const string LineNotFound = "Рядок складу не знайдено.";
    public const string MovementNotFound = "Рух не знайдено.";
    public const string Stale = "Склад уже змінили. Оновіть сторінку.";
    public const string FabricNotFound = "Тканину не знайдено в «Тканини».";
    public const string FabricWithoutCode = "У тканини немає коду FAB-: додайте його в «Тканини».";
    public const string SupplierNotFound = "Постачальника не знайдено.";
    public const string AlreadyOpened =
        "Ця тканина з таким кольором уже є на складі. Оформіть прихід або «Уточнити».";
    public const string NothingToAdjust = "Фактичний залишок такий самий, як на складі.";
    public const string DeepensShortage =
        "Уточнення не може створити або збільшити від’ємний залишок. Зафіксуйте фактичну витрату.";
    public const string NotHeld = "Тканину з таким кольором на складі не знайдено.";
    public const string OutOfRange = "Метраж перевищує допустимий ліміт.";
    public const string FabricInactive = "Тканина неактивна в «Тканини». Оберіть чинну позицію.";
    public const string IdentityChanged =
        "Характеристики вибраної позиції змінилися в «Тканини». Уточніть тканину перед записом.";
    public const string OperationReused = "Цей номер операції вже використано з іншими даними. Оновіть склад.";
    public const string OperationDate = "Дата операції — реальна дата від 01.01.1900 до 31.12.2100.";
    public const string OperationId = "Оновіть сторінку складу перед збереженням операції.";
    public const string Reason = "Причина — від 3 до 500 символів.";
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
    public const string ItemNotFound = "Завдання не знайдено.";
    public const string OrderStale = "Завдання вже змінили. Оновіть сторінку.";
    public const string CannotImport = "Переносити склад може лише адміністратор.";
}
