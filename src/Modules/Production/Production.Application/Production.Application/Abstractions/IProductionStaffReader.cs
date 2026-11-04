namespace Production.Application.Abstractions;

/// <summary>Staff accounts (Identity) for «хто змінив» and the manager of an order; implemented in the host.</summary>
public interface IProductionStaffReader
{
    Task<ProductionStaffMember?> FindAsync(int userId, CancellationToken cancellationToken);

    Task<ProductionStaffMember?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}

public sealed record ProductionStaffMember(int UserId, string Name, string? Email);
