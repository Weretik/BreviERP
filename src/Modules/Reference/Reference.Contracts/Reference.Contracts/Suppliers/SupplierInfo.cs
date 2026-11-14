namespace Reference.Contracts.Suppliers;

/// <summary>A row of «Постачальники» as other modules see it.</summary>
public sealed record SupplierInfo(
    int Id, string Name, bool IsActive, string? KeepinCrmId);
