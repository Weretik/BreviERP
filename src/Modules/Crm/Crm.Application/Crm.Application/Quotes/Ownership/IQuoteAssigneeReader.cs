namespace Crm.Application.Quotes.Ownership;

public interface IQuoteAssigneeReader
{
    Task<bool> IsEligibleAsync(int userId, CancellationToken cancellationToken);
    Task<Ardalis.Result.PagedResult<IReadOnlyList<QuoteAssignee>>> SearchAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken);
}
