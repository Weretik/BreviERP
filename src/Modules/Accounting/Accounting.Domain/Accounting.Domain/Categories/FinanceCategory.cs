using Accounting.Domain;
using Accounting.Domain.Ledger;
using BuildingBlocks.Domain.Entity;

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

    /// <summary>Takes a name and note already checked by <see cref="CheckTexts"/>.</summary>
    private FinanceCategory(FinanceCategoryId id, string name, CategoryDirection direction, LedgerScope scope,
        FinanceCategoryId? parentId, string note, int sortOrder, DateTimeOffset now)
    {
        Id = id;
        Direction = direction;
        Scope = scope;
        ParentId = parentId;
        Name = FinanceText.Name(name);
        Note = FinanceText.Clean(note);
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

    public static DomainResult<FinanceCategory> CreateRoot(FinanceCategoryId id, string? name,
        CategoryDirection direction, LedgerScope scope, string? note, int sortOrder, DateTimeOffset now)
    {
        var check = DomainResult.Combine(Guard(direction, scope), CheckTexts(name, note));
        if (check.IsFailure) return check.Error!;
        return new FinanceCategory(id, name ?? string.Empty, direction, scope, null, note ?? string.Empty, sortOrder, now);
    }

    /// <summary>A subcategory takes the direction and area of its root; the root must be an active root.</summary>
    public static DomainResult<FinanceCategory> CreateChild(FinanceCategoryId id, string? name, FinanceCategory parent, string? note,
        int sortOrder, DateTimeOffset now)
    {
        if (!parent.IsRoot)
            return AccountingErrors.Rule("Accounting.Category.Level", "Підкатегорія може бути лише в кореневій категорії.");
        if (!parent.Active)
            return AccountingErrors.Rule("Accounting.Category.Parent", "Коренева категорія вже не використовується.");
        var texts = CheckTexts(name, note);
        if (texts.IsFailure) return texts.Error!;
        return new FinanceCategory(id, name ?? string.Empty, parent.Direction, parent.Scope, parent.Id, note ?? string.Empty,
            sortOrder, now);
    }

    /// <summary>
    /// A category of the old Calculator «Фінанси» with its id and state (stage 5); a subcategory sits in a root of the
    /// same direction and area.
    /// </summary>
    public static DomainResult<FinanceCategory> FromLegacy(FinanceCategoryId id, string? name,
        CategoryDirection direction, LedgerScope scope, FinanceCategory? parent, string? note, bool active,
        int sortOrder, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        var check = DomainResult.Combine(Guard(direction, scope), CheckTexts(name, note));
        if (check.IsFailure) return check.Error!;
        if (parent is not null && (!parent.IsRoot || parent.Direction != direction || parent.Scope != scope))
            return AccountingErrors.Rule("Accounting.Category.Level",
                $"Підкатегорія «{name}» не підходить до кореневої «{parent.Name}».");
        return new FinanceCategory(id, name ?? string.Empty, direction, scope, parent?.Id, note ?? string.Empty,
            sortOrder, createdAt) { Active = active, UpdatedAt = updatedAt };
    }

    /// <summary>Changes the name, note and order; returns false when nothing differs.</summary>
    public DomainResult<bool> Update(string? name, string? note, int sortOrder, DateTimeOffset now)
    {
        var check = CheckTexts(name, note);
        if (check.IsFailure) return check.Error!;
        var cleanName = FinanceText.Name(name);
        var cleanNote = FinanceText.Clean(note);
        if (cleanName == Name && cleanNote == Note && sortOrder == SortOrder) return false;
        Name = cleanName;
        Note = cleanNote;
        SortOrder = sortOrder;
        Version++;
        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// Switches off a category that is already used, instead of deleting it: old operations keep it, new ones cannot
    /// pick it. Returns false when it is already off.
    /// </summary>
    public bool Deactivate(DateTimeOffset now)
    {
        if (!Active) return false;
        Active = false;
        Version++;
        UpdatedAt = now;
        return true;
    }

    private static DomainResult Guard(CategoryDirection direction, LedgerScope scope)
    {
        if (!Enum.IsDefined(direction))
            return AccountingErrors.Rule("Accounting.Category.Direction", "Невідомий тип категорії.");
        if (!Enum.IsDefined(scope))
            return AccountingErrors.Rule("Accounting.Category.Scope", "Невідома область обліку.");
        return DomainResult.Success();
    }

    private static DomainResult CheckTexts(string? name, string? note)
    {
        var cleanName = FinanceText.Name(name);
        if (cleanName.Length == 0) return AccountingErrors.Rule("Accounting.Category.Name", "Вкажіть назву категорії.");
        if (cleanName.Length > NameMaxLength)
            return AccountingErrors.Rule("Accounting.Category.Name", $"Назва категорії — не більше {NameMaxLength} символів.");
        if (FinanceText.Clean(note).Length > NoteMaxLength)
            return AccountingErrors.Rule("Accounting.Category.Note", $"Примітка — не більше {NoteMaxLength} символів.");
        return DomainResult.Success();
    }
}
