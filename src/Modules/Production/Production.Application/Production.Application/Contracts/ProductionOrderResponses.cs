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

public sealed record ItemApplicationResponse(string Kind, string? Place, string? Description, string? Color);

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

/// <summary>A sent КП the person may pass to the workshop.</summary>
/// <param name="OrdersCount">Orders already made from it.</param>
public sealed record ProductionQuoteSourceResponse(
    long Id, string Number, string ClientName, string ResponsibleName, DateTimeOffset? LastSentAt, int OrdersCount);

/// <summary>The КП as the order form needs it: its sewing items with every fabric variant.</summary>
public sealed record ProductionQuoteDraftResponse(
    long QuoteId, string Number, string ClientName, string ManagerName, IReadOnlyList<ProductionQuoteDraftItem> Items);

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
