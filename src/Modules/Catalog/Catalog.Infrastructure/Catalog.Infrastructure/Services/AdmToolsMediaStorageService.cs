using Catalog.Application.Contracts.Storage;
using Catalog.Infrastructure.DependencyInjection;
using Catalog.Infrastructure.Exceptions;
using Catalog.Infrastructure.Options;

namespace Catalog.Infrastructure.Services;

public sealed class AdmToolsMediaStorageService(
    IHttpClientFactory httpClientFactory,
    IOptions<AdmToolsStorageOptions> options,
    ILogger<AdmToolsMediaStorageService> logger)
    : IMediaStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AdmToolsStorageOptions _options = options.Value;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _token;
    private DateTimeOffset _tokenValidUntilUtc = DateTimeOffset.MinValue;

    public async Task<StoredMediaFileResult> UploadAsync(
        Stream fileStream,
        string storageKey,
        string contentType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        logger.LogInformation(
            "Uploading media file to adm.tools. StorageKey: {StorageKey}, ContentType: {ContentType}",
            storageKey,
            contentType);

        await UploadInternalAsync(fileStream, storageKey, contentType, cancellationToken, retryOnUnauthorized: true);

        var publicUrl = BuildPublicUrl(storageKey);

        logger.LogInformation(
            "Media file uploaded to adm.tools. StorageKey: {StorageKey}, PublicUrl: {PublicUrl}",
            storageKey,
            publicUrl);

        return new StoredMediaFileResult(
            storageKey,
            publicUrl,
            contentType,
            "adm.tools",
            _options.Host);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting media file from adm.tools. StorageKey: {StorageKey}", storageKey);

        var client = httpClientFactory.CreateClient(AdmToolsHttpClientRegistrationExtensions.HttpClientName);
        var requestUri = BuildFileEndpoint(storageKey);

        using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetValidTokenAsync(cancellationToken));

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(
                "adm.tools delete returned 401. Refreshing token and retrying. StorageKey: {StorageKey}",
                storageKey);

            InvalidateToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetValidTokenAsync(cancellationToken));
            using var retryRequest = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            retryRequest.Headers.Authorization = request.Headers.Authorization;
            using var retryResponse = await client.SendAsync(retryRequest, cancellationToken);
            await EnsureSuccessAsync(retryResponse, "delete", storageKey, cancellationToken);
            logger.LogInformation("Media file deleted from adm.tools after retry. StorageKey: {StorageKey}", storageKey);
            return;
        }

        await EnsureSuccessAsync(response, "delete", storageKey, cancellationToken);
        logger.LogInformation("Media file deleted from adm.tools. StorageKey: {StorageKey}", storageKey);
    }

    private async Task UploadInternalAsync(
        Stream fileStream,
        string storageKey,
        string contentType,
        CancellationToken cancellationToken,
        bool retryOnUnauthorized)
    {
        if (fileStream.CanSeek)
            fileStream.Position = 0;

        var client = httpClientFactory.CreateClient(AdmToolsHttpClientRegistrationExtensions.HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Put, BuildFileEndpoint(storageKey));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetValidTokenAsync(cancellationToken));

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Content = content;

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && retryOnUnauthorized)
        {
            logger.LogWarning(
                "adm.tools upload returned 401. Refreshing token and retrying. StorageKey: {StorageKey}",
                storageKey);

            InvalidateToken();
            await UploadInternalAsync(fileStream, storageKey, contentType, cancellationToken, retryOnUnauthorized: false);
            return;
        }

        await EnsureSuccessAsync(response, "upload", storageKey, cancellationToken);
    }

    private async Task<string> GetValidTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_token) && _tokenValidUntilUtc > DateTimeOffset.UtcNow.AddSeconds(30))
            return _token;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_token) && _tokenValidUntilUtc > DateTimeOffset.UtcNow.AddSeconds(30))
                return _token;

            var client = httpClientFactory.CreateClient(AdmToolsHttpClientRegistrationExtensions.HttpClientName);

            logger.LogInformation("Requesting new adm.tools auth token.");

            using var request = new HttpRequestMessage(HttpMethod.Put, "/~/api/auth");
            using var formData = new MultipartFormDataContent();
            using var loginContent = CreateMultipartFieldContent(_options.Login);
            using var passwordContent = CreateMultipartFieldContent(_options.Password);
            formData.Add(loginContent, "login");
            formData.Add(passwordContent, "password");
            request.Content = formData;

            using var response = await client.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "adm.tools auth failed. StatusCode: {StatusCode}, Response: {Response}",
                    (int)response.StatusCode,
                    payload);

                throw new AdmToolsStorageException($"adm.tools auth failed with HTTP {(int)response.StatusCode}: {payload}");
            }

            var authResponse = JsonSerializer.Deserialize<AdmToolsAuthResponse>(payload, JsonOptions)
                               ?? throw new AdmToolsStorageException("adm.tools auth returned empty response.");

            if (!authResponse.Status || authResponse.Data is null || string.IsNullOrWhiteSpace(authResponse.Data.Token))
            {
                logger.LogError("adm.tools auth returned unsuccessful payload. Response: {Response}", payload);
                throw new AdmToolsStorageException("adm.tools auth returned unsuccessful status or empty token.");
            }

            _token = authResponse.Data.Token;
            _tokenValidUntilUtc = DateTimeOffset.FromUnixTimeSeconds(authResponse.Data.Till);

            logger.LogInformation("adm.tools auth token acquired. ValidUntilUtc: {ValidUntilUtc}", _tokenValidUntilUtc);

            return _token;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse adm.tools auth response.");
            throw new AdmToolsStorageException("Failed to parse adm.tools auth response.", ex);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        string storageKey,
        CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "adm.tools {Operation} failed. StorageKey: {StorageKey}, StatusCode: {StatusCode}, Response: {Response}",
                operation,
                storageKey,
                (int)response.StatusCode,
                payload);

            throw new AdmToolsStorageException($"adm.tools {operation} failed for '{storageKey}' with HTTP {(int)response.StatusCode}: {payload}");
        }

        if (string.IsNullOrWhiteSpace(payload))
            return;

        try
        {
            var apiResponse = JsonSerializer.Deserialize<AdmToolsApiResponse>(payload, JsonOptions);
            if (apiResponse is not null && !apiResponse.Status)
            {
                logger.LogError(
                    "adm.tools {Operation} returned status=false. StorageKey: {StorageKey}, Response: {Response}",
                    operation,
                    storageKey,
                    payload);

                throw new AdmToolsStorageException($"adm.tools {operation} failed for '{storageKey}': {payload}");
            }
        }
        catch (JsonException ex)
        {
            logger.LogDebug(ex, "adm.tools {Operation} response for {StorageKey} was not JSON. Treating HTTP success as success.", operation, storageKey);
        }
    }

    private string BuildFileEndpoint(string storageKey)
        => $"/~/api/file?path={Uri.EscapeDataString(NormalizeStorageKey(storageKey))}&overwrite=1";

    private string BuildPublicUrl(string storageKey)
        => $"{_options.PublicBaseUrl.TrimEnd('/')}/{NormalizeStorageKey(storageKey).TrimStart('/')}";

    private static string NormalizeStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new AdmToolsStorageException("Storage key must be provided.");

        return storageKey.Trim();
    }

    private void InvalidateToken()
    {
        _token = null;
        _tokenValidUntilUtc = DateTimeOffset.MinValue;
    }

    private static HttpContent CreateMultipartFieldContent(string value)
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(value));
        content.Headers.ContentType = null;
        return content;
    }

    private sealed record AdmToolsAuthResponse(bool Status, AdmToolsAuthData? Data);

    private sealed record AdmToolsAuthData(string Token, long Till);

    private sealed record AdmToolsApiResponse(bool Status);
}
