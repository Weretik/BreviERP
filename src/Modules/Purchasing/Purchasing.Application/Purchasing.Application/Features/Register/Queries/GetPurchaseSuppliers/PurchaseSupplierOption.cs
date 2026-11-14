namespace Purchasing.Application.Features.Register.Queries.GetPurchaseSuppliers;

/// <summary>An active Brevi supplier that can be linked to a purchase invoice.</summary>
public sealed record PurchaseSupplierOption(int Id, string Name);
