namespace Reference.Infrastructure.Seeders.Suppliers.Rows;

public sealed record SupplierSeedRow(
    int Id,
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null);
