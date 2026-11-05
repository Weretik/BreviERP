using System.Net;
using Accounting.Application.Abstractions;
using Google;
using Google.Cloud.Storage.V1;

namespace Accounting.Infrastructure.Files;

/// <summary>
/// A private Google Cloud Storage bucket. Application Default Credentials: on Cloud Run the runtime service account,
/// locally «gcloud auth application-default login». No key file is stored anywhere.
/// </summary>
internal sealed class GoogleCloudAccountingFileStorage(string bucket) : IAccountingFileStorage, IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private StorageClient? client;

    public bool IsConfigured => true;

    public async Task UploadAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            await (await ClientAsync(cancellationToken)).UploadObjectAsync(bucket, storageKey, contentType, content,
                cancellationToken: cancellationToken);
        }
        catch (Exception error) when (error is GoogleApiException or InvalidOperationException or IOException
                                          or HttpRequestException)
        {
            throw new AccountingFileStorageUnavailableException($"Upload to gs://{bucket} failed.", error);
        }
    }

    public async Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        var content = new MemoryStream();
        try
        {
            await (await ClientAsync(cancellationToken)).DownloadObjectAsync(bucket, storageKey, content,
                cancellationToken: cancellationToken);
        }
        catch (GoogleApiException error) when (error.HttpStatusCode == HttpStatusCode.NotFound)
        {
            await content.DisposeAsync();
            return null;
        }
        catch (Exception error) when (error is GoogleApiException or InvalidOperationException or IOException
                                          or HttpRequestException)
        {
            await content.DisposeAsync();
            throw new AccountingFileStorageUnavailableException($"Download from gs://{bucket} failed.", error);
        }
        content.Position = 0;
        return content;
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        try
        {
            await (await ClientAsync(cancellationToken)).DeleteObjectAsync(bucket, storageKey,
                cancellationToken: cancellationToken);
        }
        catch (GoogleApiException error) when (error.HttpStatusCode == HttpStatusCode.NotFound)
        {
            // Already gone.
        }
    }

    private async Task<StorageClient> ClientAsync(CancellationToken cancellationToken)
    {
        if (client is not null) return client;
        await gate.WaitAsync(cancellationToken);
        try
        {
            return client ??= await StorageClient.CreateAsync();
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose()
    {
        client?.Dispose();
        gate.Dispose();
    }
}
