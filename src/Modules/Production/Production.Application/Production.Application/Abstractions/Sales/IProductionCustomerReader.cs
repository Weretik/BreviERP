namespace Production.Application.Abstractions.Sales;

/// <summary>Clients of Crm by public id; orders keep the internal id they copied from the КП (CARD-001 R-04).</summary>
public interface IProductionCustomerReader
{
    /// <returns>The internal id, or null when there is no such client.</returns>
    Task<long?> FindInternalIdAsync(Guid publicId, CancellationToken cancellationToken);
}
