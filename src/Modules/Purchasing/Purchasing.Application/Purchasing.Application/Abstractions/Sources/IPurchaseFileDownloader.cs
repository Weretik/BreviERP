namespace Purchasing.Application.Abstractions.Sources;

/// <summary>Downloads an original from a safe Dropbox link for recognition; the file is not stored.</summary>
public interface IPurchaseFileDownloader
{
    /// <exception cref="PurchaseFileUnavailableException">The file cannot be read or is not a PDF or picture.</exception>
    Task<PurchaseFileContent> DownloadAsync(string url, CancellationToken cancellationToken);
}

public sealed record PurchaseFileContent(string FileName, string MediaType, byte[] Content);

/// <summary><paramref name="Unsupported"/>: the type cannot be read by AI (Excel, Word), so retrying will not help.</summary>
public sealed class PurchaseFileUnavailableException(string message, bool unsupported = false) : Exception(message)
{
    public bool Unsupported { get; } = unsupported;
}
