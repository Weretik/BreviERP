namespace Reference.Api.Contracts.Suppliers;

public sealed record UpdateSupplierBreviFieldsRequest(bool IsActive, string? Notes = null);
