namespace Ai.Application.Abstractions;

/// <summary>Encrypts the OpenAI key at rest (ASP.NET Data Protection, keys kept in PostgreSQL).</summary>
public interface IAiKeyProtector
{
    string Protect(string apiKey);

    /// <summary>Null when the stored value can no longer be decrypted (the protection keys were lost).</summary>
    string? Unprotect(string protectedApiKey);
}
