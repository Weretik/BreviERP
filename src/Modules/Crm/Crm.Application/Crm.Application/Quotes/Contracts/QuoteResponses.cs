namespace Crm.Application.Quotes.Contracts;

public sealed record QuotePersonResponse(int UserId, string Name);

public sealed record QuoteSummaryResponse(
    long Id,
    string Number,
    string Kind,
    string Status,
    string InternalName,
    string? CompanyName,
    QuotePersonResponse Responsible,
    int ItemsCount,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastSentAt,
    DateTimeOffset? ArchivedAt);

public sealed record QuotePageResponse(IReadOnlyList<QuoteSummaryResponse> Items, int Page, int PageSize, int Total);

public sealed record QuoteResponse(
    long Id,
    string Number,
    string Kind,
    string Status,
    string InternalName,
    long? CustomerId,
    string? RecipientName,
    string? CompanyName,
    string? ClientComment,
    string? ManagerNote,
    QuotePersonResponse Author,
    QuotePersonResponse Responsible,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastSentAt,
    DateTimeOffset? ArchivedAt,
    string? ArchivedByName,
    int Version,
    IReadOnlyList<QuoteItemResponse> Items,
    QuoteIntroductionResponse? Introduction);

/// <summary>A line of «Історія змін» (R-22).</summary>
public sealed record QuoteEventResponse(long Id, string Kind, DateTimeOffset At, QuotePersonResponse Actor,
    string? Details);

/// <summary>The fields of a «КП для знайомства»; null for a КП «під запит».</summary>
public sealed record QuoteIntroductionResponse(string Layout, string? RecipientEmail, string? LetterName);

public sealed record QuoteItemResponse(
    long Id,
    string Kind,
    int? ProductId,
    string? Code,
    string Name,
    string? Subtitle,
    string? Description,
    string? SiteUrl,
    string? PhotoUrl,
    string? Season,
    string? SupplierUrl,
    IReadOnlyList<QuoteOptionResponse> Options,
    IReadOnlyList<QuoteItemLineResponse> Lines,
    long? EmbroideryCalculationId = null);

/// <summary>An accessory, operation, service or extra fabric of a «Виріб з нуля».</summary>
public sealed record QuoteItemLineResponse(
    long Id, string Kind, int ReferenceId, string Name, string Unit, decimal Quantity, decimal? ManualMinutes,
    string? CostCode);

public sealed record QuoteOptionResponse(
    long Id,
    long? ArticleId,
    int? FabricId,
    string Title,
    string? Details,
    bool IsRecommended,
    QuotePriceResponse Retail,
    QuotePriceResponse Wholesale,
    QuotePriceResponse Large,
    string? ManualReason,
    string? ManualByName,
    DateTimeOffset? ManualAt,
    DateTimeOffset? PricesTakenAt,
    decimal? FabricMeters,
    IReadOnlyList<QuoteOptionFabricResponse> Fabrics);

/// <summary>One fabric of a variant, summed in its price (R-26, R-27).</summary>
public sealed record QuoteOptionFabricResponse(int FabricId, string Name, decimal Meters);

/// <param name="Amount">What the client sees: the manual price, else the Catalog one.</param>
public sealed record QuotePriceResponse(decimal? Amount, decimal? Source, bool IsManual, bool Shown);

/// <summary>«Перерахувати за актуальними цінами»: what changes, what cannot be priced any more.</summary>
public sealed record QuoteRepriceReport(
    bool Applied,
    int Version,
    IReadOnlyList<QuoteRepriceChange> Changes,
    IReadOnlyList<QuoteRepriceProblem> Unavailable);

public sealed record QuoteRepriceChange(
    long OptionId,
    string ItemName,
    string OptionTitle,
    QuoteTierPricesResponse Before,
    QuoteTierPricesResponse After);

public sealed record QuoteRepriceProblem(long OptionId, string ItemName, string OptionTitle, string Reason);

public sealed record QuoteTierPricesResponse(decimal? Retail, decimal? Wholesale, decimal? Large);

public sealed record QuoteCatalogPageResponse(
    IReadOnlyList<QuoteCatalogProductResponse> Items, int Page, int PageSize, int Total);

/// <param name="Problem">Why the product cannot be added; null when it can.</param>
public sealed record QuoteCatalogProductResponse(
    int ProductId,
    string Kind,
    string? Code,
    string Name,
    string? Season,
    string? PhotoUrl,
    string? Description,
    QuoteTierPricesResponse? Prices,
    string? Problem,
    IReadOnlyList<QuoteCatalogVariantResponse> Variants);

/// <param name="Problem">Why the variant cannot be added; null when it can.</param>
public sealed record QuoteCatalogVariantResponse(
    long ArticleId,
    string Sku,
    string Title,
    string? FabricInfo,
    QuoteTierPricesResponse? Prices,
    string? Problem);

public sealed record QuoteStaffResponse(int UserId, string Name, string? Email);
