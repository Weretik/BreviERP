using System.Security.Cryptography;
using Ai.Application.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace Ai.Infrastructure.Security;

/// <summary>ASP.NET Data Protection; its keys are kept in PostgreSQL (Identity), so every Cloud Run instance can read the key.</summary>
internal sealed class AiKeyProtector(IDataProtectionProvider provider) : IAiKeyProtector
{
    private readonly IDataProtector _protector = provider.CreateProtector("Ai.OpenAi.ApiKey.v1");

    public string Protect(string apiKey) => _protector.Protect(apiKey);

    public string? Unprotect(string protectedApiKey)
    {
        try
        {
            return _protector.Unprotect(protectedApiKey);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
