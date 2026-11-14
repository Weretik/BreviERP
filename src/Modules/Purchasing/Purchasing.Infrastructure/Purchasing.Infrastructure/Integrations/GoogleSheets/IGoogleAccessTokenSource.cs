namespace Purchasing.Infrastructure.Integrations.GoogleSheets;

/// <summary>A read-only Google Sheets access token of the service's own account.</summary>
public interface IGoogleAccessTokenSource
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}
