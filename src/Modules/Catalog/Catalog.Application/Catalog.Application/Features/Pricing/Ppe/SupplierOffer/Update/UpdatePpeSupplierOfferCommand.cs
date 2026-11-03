using Catalog.Application.Features.Pricing.Ppe.SupplierOffer.Shared;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierOffer.Update;

/// <summary>
/// Replaces the PPE card supplier fields. When the supplier price or the rule changes, a new approved offer version
/// is appended so the three prices follow; <paramref name="ExpectedOfferVersionId"/> guards against a stale card.
/// </summary>
public sealed record UpdatePpeSupplierOfferCommand(
    int ProductId,
    int SupplierId,
    string? SupplierSku,
    string? SupplierUrl,
    decimal? SupplierPrice,
    bool AllowManualPrice,
    string? RuleCode,
    string? Unit,
    Guid? ExpectedOfferVersionId,
    string Actor) : ICommand<Result<PpeSupplierOffer>>;
