using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Shared;

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
