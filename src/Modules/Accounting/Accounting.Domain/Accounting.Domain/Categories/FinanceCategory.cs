using Accounting.Domain.Errors;
using Accounting.Domain.Ledger;

namespace Accounting.Domain.Categories;

/// <summary>
/// Income or expense category of one area of the books. Two levels only: a root and its subcategories. Direction, area
/// and parent never change after creation; the name, note and order can.
/// </summary>
public sealed class FinanceCategory : IAggregateRoot
{
    public const int NameMaxLength = 120;
    public const int NoteMaxLength = 600;

    private FinanceCategory() { }

    private FinanceCategory(FinanceCategoryId id, string name, CategoryDirection direction, LedgerScope scope,
        FinanceCategoryId? parentId, string note, int sortOrder, DateTimeOffset now)
    {
        Id = id;
        Direction = direction;
        Scope = scope;
        ParentId = parentId;
        Name = CleanName(name);
        Note = CleanNote(note);
        SortOrder = sortOrder;
        Active = true;
        Version = 1;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public FinanceCategoryId Id { get; private set; }
    public string Name { get; private set; } = null!;
    public CategoryDirection Direction { get; private set; }
    public LedgerScope Scope { get; private set; }

    /// <summary>The root of a subcategory; null for a root.</summary>
    public FinanceCategoryId? ParentId { get; private set; }

    public string Note { get; private set; } = null!;
    public bool Active { get; private set; }
    public int SortOrder { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsRoot => ParentId is null;

    public static FinanceCategory CreateRoot(FinanceCategoryId id, string? name, CategoryDirection direction,
        LedgerScope scope, string? note, int sortOrder, DateTimeOffset now)
    {
        Guard(direction, scope);
        return new FinanceCategory(id, name ?? string.Empty, direction, scope, null, note ?? string.Empty, sortOrder, now);
    }

    /// <summary>A subcategory takes the direction and area of its root; the root must be an active root.</summary>
    public static FinanceCategory CreateChild(FinanceCategoryId id, string? name, FinanceCategory parent, string? note,
        int sortOrder, DateTimeOffset now)
    {
        if (!parent.IsRoot)
            throw AccountingDomainError.Fail("Accounting.Category.Level", "Підкатегорія може бути лише в кореневій категорії.");
        if (!parent.Active)
            throw AccountingDomainError.Fail("Accounting.Category.Parent", "Коренева категорія вже не використовується.");
        return new FinanceCategory(id, name ?? string.Empty, parent.Direction, parent.Scope, parent.Id, note ?? string.Empty,
            sortOrder, now);
    }

    /// <summary>Changes the name, note and order; returns false when nothing differs.</summary>
    public bool Update(string? name, string? note, int sortOrder, DateTimeOffset now)
    {
        var cleanName = CleanName(name ?? string.Empty);
        var cleanNote = CleanNote(note ?? string.Empty);
        if (cleanName == Name && cleanNote == Note && sortOrder == SortOrder) return false;
        Name = cleanName;
        Note = cleanNote;
        SortOrder = sortOrder;
        Version++;
        UpdatedAt = now;
        return true;
    }

    private static void Guard(CategoryDirection direction, LedgerScope scope)
    {
        if (!Enum.IsDefined(direction))
            throw AccountingDomainError.Fail("Accounting.Category.Direction", "Невідомий тип категорії.");
        if (!Enum.IsDefined(scope))
            throw AccountingDomainError.Fail("Accounting.Category.Scope", "Невідома область обліку.");
    }

    private static string CleanName(string name)
    {
        var clean = FinanceText.Name(name);
        if (clean.Length == 0) throw AccountingDomainError.Fail("Accounting.Category.Name", "Вкажіть назву категорії.");
        if (clean.Length > NameMaxLength)
            throw AccountingDomainError.Fail("Accounting.Category.Name", $"Назва категорії — не більше {NameMaxLength} символів.");
        return clean;
    }

    private static string CleanNote(string note)
    {
        var clean = FinanceText.Clean(note);
        if (clean.Length > NoteMaxLength)
            throw AccountingDomainError.Fail("Accounting.Category.Note", $"Примітка — не більше {NoteMaxLength} символів.");
        return clean;
    }
}
