using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Commands.CreateProductionOrder;

/// <summary>«Передати в цех». <paramref name="RequestId"/> is new for every form; a repeat returns the same order.</summary>
/// <param name="PriceTier">Retail, Wholesale or Large: the КП price kept for the owner's reports.</param>
public sealed record CreateProductionOrderInput(
    Guid RequestId,
    long QuoteId,
    string ClientName,
    DateOnly OrderDate,
    string? Comment,
    string PriceTier,
    IReadOnlyList<CreateProductionItemInput> Items);
