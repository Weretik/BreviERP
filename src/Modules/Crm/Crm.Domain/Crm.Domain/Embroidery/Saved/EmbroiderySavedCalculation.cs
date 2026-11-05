using System.Globalization;
using Crm.Domain.Embroidery.Calculation;

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

    private EmbroiderySavedCalculation() { }

    public EmbroiderySavedCalculation(string? name, Guid? customerPublicId, string? customerName, EmbroideryInput input,
        EmbroideryRateSnapshot snapshot, EmbroideryResult result, int createdByUserId, string createdByName,
        DateTimeOffset now)
    {
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

    /// <summary>Some ticked costs had no rate: the prices were «Орієнтовна».</summary>
    public bool Incomplete { get; private set; }

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
        new(name, null, null, input, snapshot, result, createdByUserId, createdByName, createdAt)
        {
            LegacyId = Cut(legacyId.Trim(), LegacyIdMaxLength),
        };

    /// <summary>«ВШ-0042»: the id with the prefix, never stored.</summary>
    public static string NumberOf(long id) => "ВШ-" + id.ToString("D4", CultureInfo.InvariantCulture);

    /// <summary>Trimmed and cut to <see cref="NameMaxLength"/>, as the old calculator did.</summary>
    public static string NormalizeName(string? name)
    {
        return Cut((name ?? "").Trim(), NameMaxLength);
    }

    private static string Cut(string text, int max) => text.Length <= max ? text : text[..max];
}
