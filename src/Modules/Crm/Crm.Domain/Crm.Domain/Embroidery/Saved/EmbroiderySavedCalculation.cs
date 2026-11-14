using BuildingBlocks.Domain.Results;
using System.Globalization;
using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Settings;

namespace Crm.Domain.Embroidery.Saved;

/// <summary>
/// One saved «Розрахунок» (EMB-001 stage 2): the form, the frozen rates and the result as they were. It is never
/// recalculated; saving again makes a new record.
/// </summary>
public sealed class EmbroiderySavedCalculation
{
    public const int NameMaxLength = 120;
    public const int CustomerNameMaxLength = 300;
    public const int PersonNameMaxLength = 200;
    public const int LegacyIdMaxLength = 64;
    public const int CategoryMaxLength = 120;

    private EmbroiderySavedCalculation() { }

    public static DomainResult<EmbroiderySavedCalculation> Create(string? name, Guid? customerPublicId,
        string? customerName, EmbroideryInput input, EmbroideryRateSnapshot snapshot, EmbroideryResult result,
        int createdByUserId, string createdByName, DateTimeOffset now, string? category = null) =>
        category is not null && result.Prices.All(x => x.Name != category)
            ? Settings.EmbroiderySettingsErrors.CategoryNotInResult()
            : new EmbroiderySavedCalculation(name, customerPublicId, customerName, input, snapshot, result,
                createdByUserId, createdByName, now, category);

    private EmbroiderySavedCalculation(string? name, Guid? customerPublicId, string? customerName, EmbroideryInput input,
        EmbroideryRateSnapshot snapshot, EmbroideryResult result, int createdByUserId, string createdByName,
        DateTimeOffset now, string? category)
    {
        Category = category;
        Name = NormalizeName(name);
        CustomerPublicId = customerPublicId;
        CustomerName = customerPublicId is null || customerName is null ? null : Cut(customerName, CustomerNameMaxLength);
        Mode = input.Mode;
        Quantity = input.Quantity;
        Input = input;
        Snapshot = snapshot;
        Result = result;
        Incomplete = result.Incomplete;
        CreatedByUserId = createdByUserId;
        CreatedByName = Cut(createdByName, PersonNameMaxLength);
        CreatedAt = now;
    }

    public long Id { get; private set; }

    /// <summary>The old calculator's record id for a calculation moved from its archive; null for one saved here.</summary>
    public string? LegacyId { get; private set; }

    /// <summary>What the person called it; empty means «Без назви».</summary>
    public string Name { get; private set; } = "";

    public Guid? CustomerPublicId { get; private set; }
    public string? CustomerName { get; private set; }
    public EmbroideryMode Mode { get; private set; }
    public int Quantity { get; private set; }
    public EmbroideryInput Input { get; private set; } = null!;
    public EmbroideryRateSnapshot Snapshot { get; private set; } = null!;
    public EmbroideryResult Result { get; private set; } = null!;

    /// <summary>
    /// The one price category chosen when saving («Категорія 1» or «Категорія 2»), a name of <see cref="Result"/>
    /// prices; null for a calculation saved before the choice, which stands for «Стандарт» (shown as «Категорія 1»).
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>The saved price: the chosen category, else «Стандарт», else the first price; null without prices.</summary>
    public EmbroideryPrice? Price =>
        Result.Prices.FirstOrDefault(x => x.Name == (Category ?? EmbroideryCategories.LegacyChosen))
        ?? Result.Prices.FirstOrDefault();

    /// <summary>Some ticked costs had no rate: the prices were «Орієнтовна».</summary>
    public bool Incomplete { get; private set; }

    /// <summary>A photo or mockup of the design from the Media library, for the workshop; null when none.</summary>
    public int? MockupMediaFileId { get; private set; }

    public int CreatedByUserId { get; private set; }
    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// A calculation of the old calculator's archive: its own date and author, the rates it was saved with and the
    /// result those rates give, which the import checked against the old saved prices.
    /// </summary>
    public static EmbroiderySavedCalculation Transferred(string legacyId, string? name, EmbroideryInput input,
        EmbroideryRateSnapshot snapshot, EmbroideryResult result, int createdByUserId, string createdByName,
        DateTimeOffset createdAt) =>
        new(name, null, null, input, snapshot, result, createdByUserId, createdByName, createdAt, null)
        {
            LegacyId = Cut(legacyId.Trim(), LegacyIdMaxLength),
        };

    /// <summary>Sets or clears the mockup; the only thing that changes after saving.</summary>
    public void SetMockup(int? mediaFileId) => MockupMediaFileId = mediaFileId;

    /// <summary>«ВШ-0042»: the id with the prefix, never stored.</summary>
    public static string NumberOf(long id) => "ВШ-" + id.ToString("D4", CultureInfo.InvariantCulture);

    /// <summary>Trimmed and cut to <see cref="NameMaxLength"/>, as the old calculator did.</summary>
    public static string NormalizeName(string? name)
    {
        return Cut((name ?? "").Trim(), NameMaxLength);
    }

    private static string Cut(string text, int max) => text.Length <= max ? text : text[..max];
}
