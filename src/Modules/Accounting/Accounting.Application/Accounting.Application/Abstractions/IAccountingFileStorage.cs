namespace Accounting.Application.Abstractions;

/// <summary>
/// The private file storage of «Бухгалтерія» (a Google Cloud Storage bucket with no public access). Files are read only
/// through the API, by a Director.
/// </summary>
public interface IAccountingFileStorage
{
    /// <summary>False until a bucket (or a local folder) is set; uploads then answer 503 with a clear reason.</summary>
    bool IsConfigured { get; }

    /// <exception cref="AccountingFileStorageUnavailableException">The storage cannot be reached.</exception>
    Task UploadAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken);

    /// <returns>The content, or null when the storage has no such file.</returns>
    /// <exception cref="AccountingFileStorageUnavailableException">The storage cannot be reached.</exception>
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}

/// <summary>The storage did not answer or refused (no bucket, no access); the message is for the log.</summary>
public sealed class AccountingFileStorageUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
