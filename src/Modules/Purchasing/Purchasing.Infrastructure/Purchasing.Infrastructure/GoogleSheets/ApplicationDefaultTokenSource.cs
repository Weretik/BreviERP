using Google.Apis.Auth.OAuth2;
using Purchasing.Application.Abstractions;

namespace Purchasing.Infrastructure.GoogleSheets;

/// <summary>
/// Application Default Credentials: on Cloud Run the runtime service account, locally «gcloud auth
/// application-default login». No key file is stored anywhere.
/// </summary>
internal sealed class ApplicationDefaultTokenSource : IGoogleAccessTokenSource
{
    private const string ReadOnlyScope = "https://www.googleapis.com/auth/spreadsheets.readonly";
    private readonly SemaphoreSlim gate = new(1, 1);
    private ITokenAccess? credential;

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await (await CredentialAsync(cancellationToken)).GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);
        }
        catch (Exception error) when (error is InvalidOperationException or IOException or HttpRequestException
                                          or Google.Apis.Auth.OAuth2.Responses.TokenResponseException)
        {
            throw new PurchaseSheetUnavailableException("Немає облікових даних Google для читання таблиці.", error);
        }
    }

    private async Task<ITokenAccess> CredentialAsync(CancellationToken cancellationToken)
    {
        if (credential is not null) return credential;
        await gate.WaitAsync(cancellationToken);
        try
        {
            return credential ??= (await GoogleCredential.GetApplicationDefaultAsync(cancellationToken)).CreateScoped(ReadOnlyScope);
        }
        finally
        {
            gate.Release();
        }
    }
}
