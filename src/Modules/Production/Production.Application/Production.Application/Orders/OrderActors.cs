using Production.Application.Abstractions;

namespace Production.Application.Orders;

internal static class OrderActors
{
    /// <summary>The name kept in «Історія змін» and «Створив».</summary>
    public static async Task<string?> NameAsync(IProductionStaffReader staff, int userId,
        CancellationToken cancellationToken) =>
        (await staff.FindAsync(userId, cancellationToken))?.Name;
}
