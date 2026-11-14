namespace Catalog.Api.Endpoints.Overview.CreateCatalogProduct;

/// <summary>The supplier block of a new PPE product.</summary>
public sealed record CreatePpeSupplierRequest(
    int SupplierId,
    string? SupplierSku,
    string SupplierUrl,
    decimal SupplierPrice,
    bool AllowManualPrice,
    string RuleCode,
    string Unit);
