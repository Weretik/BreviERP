using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.ImportQuotes;

/// <summary>
/// One-time transfer of КП from the old calculator (R-25). A dry run only returns the report; otherwise every new КП is
/// saved in one transaction. A КП that was already transferred is skipped, so the same file can be sent again.
/// </summary>
public sealed record ImportQuotesCommand(QuoteActor Actor, bool DryRun, IReadOnlyList<ImportQuoteInput> Quotes)
    : ICommand<Result<QuoteImportReport>>;

/// <param name="Number">The calculator number `n-month-year`; kept unless a new Brevi КП already has it.</param>
/// <param name="Kind">`Request` or `Introduction`.</param>
/// <param name="Status">`Draft` or `Sent`.</param>
/// <param name="KeepinClientId">The KeepinCRM client, linked when Brevi has that customer.</param>
public sealed record ImportQuoteInput(
    string? SourceId,
    string Number,
    string Kind,
    string Status,
    string InternalName,
    string? RecipientName,
    string? CompanyName,
    string? ClientComment,
    string? ManagerNote,
    string? KeepinClientId,
    ImportPersonInput Author,
    ImportPersonInput Responsible,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastSentAt,
    ImportIntroductionInput? Introduction,
    IReadOnlyList<ImportItemInput> Items,
    IReadOnlyList<ImportEventInput>? Events);

/// <summary>A calculator user: found among Brevi staff by e-mail.</summary>
public sealed record ImportPersonInput(string? Email, string Name);

/// <param name="Layout">`General`, `Summer` or `Winter`.</param>
public sealed record ImportIntroductionInput(string Layout, string? RecipientEmail);

/// <param name="Kind">`Sewing`, `Ppe`, `OneOffPpe` or `CustomSewing`.</param>
/// <param name="Code">The Catalog code: a model `MOD-…` or a PPE site article.</param>
/// <param name="Lines">The composition of a «Виріб з нуля» by Reference codes.</param>
public sealed record ImportItemInput(
    string Kind,
    string? Code,
    string Name,
    string? Description,
    string? SiteUrl,
    string? PhotoUrl,
    string? Season,
    string? SupplierUrl,
    IReadOnlyList<ImportOptionInput> Options,
    IReadOnlyList<ImportLineInput>? Lines);

/// <param name="FabricCode">The Reference fabric `FAB-…` of a sewing row.</param>
/// <param name="FabricMeters">Metres of that fabric in one «Виріб з нуля».</param>
public sealed record ImportOptionInput(
    string? FabricCode,
    decimal? FabricMeters,
    string Title,
    string? Details,
    bool IsRecommended,
    ImportPricesInput Prices,
    string? ManualReason,
    string? ManualByEmail,
    string? ManualByName,
    DateTimeOffset? ManualAt);

public sealed record ImportPricesInput(ImportPriceInput Retail, ImportPriceInput Wholesale, ImportPriceInput Large);

/// <param name="Source">The price the calculator took from its catalogue.</param>
/// <param name="Manual">The manager's own price.</param>
public sealed record ImportPriceInput(decimal? Source, decimal? Manual, bool Shown);

/// <param name="Kind">`Fabric`, `Accessory` or `Operation`.</param>
/// <param name="Code">`FAB-…`, `FUR-…` or `OP-…`.</param>
public sealed record ImportLineInput(string Kind, string Code, decimal Quantity);

/// <param name="Kind">`Sent` or `ResponsibleChanged`.</param>
public sealed record ImportEventInput(string Kind, DateTimeOffset At, ImportPersonInput Actor, string? Details);
