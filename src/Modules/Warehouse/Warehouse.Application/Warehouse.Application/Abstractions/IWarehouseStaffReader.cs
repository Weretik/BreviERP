namespace Warehouse.Application.Abstractions;

/// <summary>Staff accounts (Identity) for «хто змінив»; implemented in the host.</summary>
public interface IWarehouseStaffReader
{
    Task<WarehouseStaffMember?> FindAsync(int userId, CancellationToken cancellationToken);

    Task<WarehouseStaffMember?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}

public sealed record WarehouseStaffMember(int UserId, string Name, string? Email);
