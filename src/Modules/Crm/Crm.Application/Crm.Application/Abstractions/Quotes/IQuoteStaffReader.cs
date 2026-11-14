namespace Crm.Application.Abstractions.Quotes;

/// <summary>Active staff who may be responsible for a КП (Manager, Director, Admin); implemented over Identity.</summary>
public interface IQuoteStaffReader
{
    Task<IReadOnlyList<QuoteStaffMember>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Null when the user does not exist, is blocked or has no staff role.</summary>
    Task<QuoteStaffMember?> FindAsync(int userId, CancellationToken cancellationToken);
}

public sealed record QuoteStaffMember(int UserId, string Name, string? Email, string? Phone = null);
