namespace Catalog.Api.Pricing.Ppe;

/// <summary>The PPE card supplier fields; every field is replaced.</summary>
public sealed record UpdatePpeSupplierOfferRequest(
    int SupplierId,
    string? SupplierSku,
    string? SupplierUrl,
    decimal? SupplierPrice,
    bool AllowManualPrice,
    string? RuleCode,
    string? Unit,
    Guid? ExpectedOfferVersionId);
