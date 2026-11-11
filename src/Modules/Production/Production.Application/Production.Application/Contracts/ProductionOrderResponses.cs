namespace Production.Application.Contracts;

public sealed record ProductionOrderSummaryResponse(
    int Id,
    int Number,
    string Status,
    string ClientName,
    long? QuoteId,
    string? QuoteNumber,
    string ManagerName,
    DateOnly OrderDate,
    int ItemsCount,
    int TotalQuantity,
    DateOnly? NearestDueDate,
    bool IsTransferred,
    DateTimeOffset UpdatedAt,
    string Stage);

/// <summary>Counts by status for the filter chips; they ignore the status filter itself.</summary>
public sealed record ProductionOrderCounts(int All, int New, int Closed, int Cancelled);

public sealed record ProductionOrderPageResponse(
    IReadOnlyList<ProductionOrderSummaryResponse> Items, int Page, int PageSize, int Total,
    ProductionOrderCounts Counts, IReadOnlyList<string> Managers);

/// <summary>A model of «Зведення за моделями»: ordered pieces over the filtered orders.</summary>
/// <param name="ModelCode">Null for an item without a Catalog model (old orders); then the name groups it.</param>
/// <param name="Season">Catalog season of the model; null when unknown.</param>
/// <param name="Category">Read from the model name: «Куртка», «Костюм куртка + брюки»…, else «Інше».</param>
public sealed record ProductionModelSummaryRow(
    string? ModelCode, string ModelName, string? Season, string Category, int Quantity, int OrdersCount);

public sealed record ProductionModelSummaryResponse(IReadOnlyList<ProductionModelSummaryRow> Rows, int TotalQuantity);

/// <summary>A model or fabric of an order item: its code and name.</summary>
public sealed record ProductionFilterOption(string Code, string Name);

/// <summary>Values «Фільтри» of «Виробничі завдання» offer, taken from the orders that are not deleted.</summary>
public sealed record ProductionOrderFilterOptionsResponse(
    IReadOnlyList<string> Clients,
    IReadOnlyList<ProductionFilterOption> Models,
    IReadOnlyList<ProductionFilterOption> Fabrics,
    IReadOnlyList<string> Colors,
    IReadOnlyList<string> Seasons);

/// <summary>What the signed-in person may do with the order and which prices they see.</summary>
/// <param name="CanTrack">Tick етапи and record shipments: Admin and the head of the workshop on a «Нове» order.</param>
/// <param name="CanClose">«Закрити»: may track and everything is shipped.</param>
/// <param name="CanReopen">«Повернути в роботу»: Admin, on a closed or cancelled order.</param>
public sealed record ProductionOrderPermissions(
    bool CanEdit, bool CanCopy, bool CanDelete, bool SeesSewingRate, bool SeesReportPrice,
    bool CanTrack, bool CanClose, bool CanCancel, bool CanReopen, bool CanManageFiles, bool CanSendTelegram);

public sealed record ProductionShipmentResponse(
    int Id, int Quantity, DateOnly ShippedOn, string? Comment, string CreatedByName, DateTimeOffset CreatedAt);

/// <param name="Kind">Sizes, Application, Extra or Photo.</param>
/// <param name="IsOldLink">A Google Drive link moved from the old «Цех»; the file itself is not on the CDN.</param>
public sealed record ProductionFileResponse(
    int Id, int? ItemId, string Kind, string Name, string ContentType, long Bytes, string Url, bool IsOldLink,
    string CreatedByName, DateTimeOffset CreatedAt);

/// <param name="Status">NotSent, Sending, Sent or Failed.</param>
/// <param name="Ready">The workshop group is set and sending is on, so «Надіслати в цех» can work.</param>
public sealed record ProductionTelegramResponse(
    string Status, DateTimeOffset? SentAt, string? SentByName, string? Error, bool Ready);

/// <param name="EmbroideryNumber">«ВШ-0042», kept even when the calculation was removed.</param>
/// <param name="Embroidery">The calculation as it is now; null when none or it was removed.</param>
public sealed record ItemApplicationResponse(string Kind, string? Place, string? Description, string? Color,
    long? EmbroideryCalculationId, string? EmbroideryNumber, ProductionEmbroideryResponse? Embroidery);

/// <summary>The saved «Ціна вишивки» calculation of an embroidery, for the workshop; no prices.</summary>
/// <param name="Mode">`Garment` or `Chevron`.</param>
/// <param name="StabilizerMetres">Флізелін to cut; shown only, the stock is not changed.</param>
/// <param name="FabricMetres">Fabric under the chevrons; null for embroidery on a garment.</param>
public sealed record ProductionEmbroideryResponse(
    long Id, string Number, string Name, string? CustomerName, string Mode, int Quantity, int StitchesPerUnit, string HoopLabel,
    int Hoopings, decimal? StabilizerMetres, decimal? FabricMetres, string? MockupUrl, string? MockupThumbnailUrl)
{
    public ProductionEmbroideryChoiceResponse ToChoice() =>
        new(Id, Number, Name, CustomerName, Quantity, StitchesPerUnit, HoopLabel, MockupThumbnailUrl);
}

/// <summary>A saved calculation for «Розрахунок вишивки» in the order form.</summary>
public sealed record ProductionEmbroideryChoiceResponse(
    long Id, string Number, string Name, string? CustomerName, int Quantity, int StitchesPerUnit, string HoopLabel,
    string? MockupThumbnailUrl);

/// <param name="SewingRate">Null for a manager.</param>
/// <param name="ReportUnitPrice">Null for anyone but Admin and Director.</param>
public sealed record ProductionItemResponse(
    int Id,
    int Position,
    string? ModelCode,
    string ModelName,
    string? FabricCode,
    string? FabricName,
    int Quantity,
    DateOnly DueDate,
    string MainColor,
    IReadOnlyList<string> AdditionalColors,
    string? SizesText,
    string? TechnicalChanges,
    string? Comment,
    IReadOnlyList<ItemApplicationResponse> Applications,
    decimal? PlannedFabricMeters,
    decimal? SewingRate,
    decimal? ReportUnitPrice,
    IReadOnlyList<string> Stages,
    string CurrentStage,
    int ShippedQuantity,
    IReadOnlyList<ProductionShipmentResponse> Shipments);

/// <param name="PriceTier">Null for anyone but Admin and Director, and for transferred orders.</param>
public sealed record ProductionOrderResponse(
    int Id,
    int Number,
    string Status,
    string ClientName,
    long? QuoteId,
    string? QuoteNumber,
    string ManagerName,
    DateOnly OrderDate,
    string? Comment,
    string? PriceTier,
    bool IsTransferred,
    int? CopiedFromOrderId,
    DateTimeOffset CreatedAt,
    string CreatedByName,
    DateTimeOffset UpdatedAt,
    int Version,
    string? CancelReason,
    ProductionOrderPermissions Permissions,
    IReadOnlyList<ProductionItemResponse> Items,
    IReadOnlyList<ProductionFileResponse> Files,
    ProductionTelegramResponse Telegram);

/// <summary>A line of «Історія змін».</summary>
public sealed record ProductionOrderChangeResponse(
    long Id, int? ItemId, string? ItemName, string Field, string? Before, string? After, string ActorName,
    DateTimeOffset At);

/// <summary>A КП «під запит» the person may pass to the workshop.</summary>
/// <param name="OrdersCount">Orders already made from it.</param>
public sealed record ProductionQuoteSourceResponse(
    long Id, string Number, string ClientName, string ResponsibleName, DateTimeOffset? LastSentAt, int OrdersCount);

/// <summary>The КП as the order form needs it: its sewing items with every fabric variant.</summary>
/// <param name="Embroideries">«Вишивка» rows of the КП; the form puts them into the items' applications.</param>
public sealed record ProductionQuoteDraftResponse(
    long QuoteId, string Number, string ClientName, string ManagerName, IReadOnlyList<ProductionQuoteDraftItem> Items,
    IReadOnlyList<ProductionQuoteDraftEmbroidery> Embroideries);

/// <summary>A «Вишивка» row of the КП whose saved calculation still exists.</summary>
public sealed record ProductionQuoteDraftEmbroidery(long QuoteItemId, ProductionEmbroideryChoiceResponse Calculation);

/// <param name="Norm">The workshop norm of the model; null when there is none.</param>
public sealed record ProductionQuoteDraftItem(
    long QuoteItemId, string Name, string? ModelCode, FabricNormInput? Norm,
    IReadOnlyList<ProductionQuoteDraftOption> Options);

/// <param name="SewingRate">Null for a manager.</param>
public sealed record ProductionQuoteDraftOption(
    long OptionId, string Title, string? FabricCode, string? FabricName, decimal? Retail, decimal? Wholesale,
    decimal? Large, decimal? SewingRate);

public sealed record ProductionOrderImportResult(
    bool DryRun, int Created, int Unchanged, int Errors, IReadOnlyList<ProductionOrderImportLine> Lines);

/// <param name="Action">created, unchanged or error.</param>
public sealed record ProductionOrderImportLine(int Number, string ClientName, int Items, string Action, string? Error);

/// <summary>«Налаштування цеху».</summary>
/// <param name="BotConfigured">The server has the bot token.</param>
/// <param name="Chats">Groups the bot is in, to pick the workshop group from.</param>
public sealed record WorkshopSettingsResponse(
    bool TelegramEnabled, long? TelegramChatId, int? TelegramThreadId, bool BotConfigured,
    IReadOnlyList<WorkshopTelegramChatResponse> Chats, string? UpdatedByName, DateTimeOffset? UpdatedAt);

public sealed record WorkshopTelegramChatResponse(long ChatId, string Title);

public sealed record ProductionProgressImportResult(
    bool DryRun, int Updated, int Unchanged, int Errors, IReadOnlyList<ProductionProgressImportLine> Lines);

/// <param name="Action">updated, unchanged or error.</param>
public sealed record ProductionProgressImportLine(
    int? Number, string LegacyId, int Stages, int Files, string Action, string? Error);

public sealed record ProductionReportPriceImportResult(
    bool DryRun, int Updated, int Unchanged, int Errors, IReadOnlyList<ProductionReportPriceImportLine> Lines);

/// <param name="Prices">Items that take a price; an item that already has one keeps it.</param>
/// <param name="Action">updated, unchanged or error.</param>
public sealed record ProductionReportPriceImportLine(
    int? Number, string LegacyId, int Prices, string Action, string? Error);
