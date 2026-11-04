namespace Crm.Application.Quotes.Import;

/// <summary>
/// What the transfer does (dry run) or did. <see cref="Warnings"/> are transferred anyway: the КП keeps the calculator
/// prices, but «Перерахувати» cannot update the named rows.
/// </summary>
/// <param name="Created">КП that are (or would be) added.</param>
/// <param name="AlreadyImported">Calculator numbers transferred before; skipped.</param>
/// <param name="Renumbered">КП whose number a new Brevi КП already has; they get the next free number of that month.</param>
/// <param name="UnknownPeople">Calculator users not found among Brevi staff; their КП go to the person who imports.</param>
public sealed record QuoteImportReport(
    bool DryRun,
    bool Applied,
    int Total,
    int Created,
    int Requests,
    int Introductions,
    int Items,
    IReadOnlyList<string> AlreadyImported,
    IReadOnlyList<QuoteImportRenumbered> Renumbered,
    IReadOnlyList<QuoteImportPerson> UnknownPeople,
    IReadOnlyList<QuoteImportWarning> Warnings);

public sealed record QuoteImportRenumbered(string From, string To);

public sealed record QuoteImportPerson(string? Email, string Name);

/// <param name="Item">The item name, or null for the whole КП.</param>
public sealed record QuoteImportWarning(string Number, string? Item, string Message);
