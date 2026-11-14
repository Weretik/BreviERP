namespace Reference.Infrastructure.Persistence.Seeding.Suppliers;

public sealed record SupplierSeedRow(
    int Id,
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null,
    string? Notes = null);
