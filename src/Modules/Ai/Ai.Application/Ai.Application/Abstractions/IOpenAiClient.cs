namespace Ai.Application.Abstractions;

/// <summary>OpenAI Responses API with a strict JSON schema answer. The key is passed per call and never logged.</summary>
public interface IOpenAiClient
{
    /// <exception cref="OpenAiException">OpenAI refused the call or did not answer.</exception>
    Task<OpenAiAnswer> RespondAsync(OpenAiRequest request, CancellationToken cancellationToken);
}

/// <summary>A document or picture sent with the request: PDF as a file, images as pictures.</summary>
public sealed record AiFile(string FileName, string MediaType, byte[] Content);

public sealed record OpenAiRequest(
    string ApiKey,
    string Model,
    string Instructions,
    string Text,
    IReadOnlyList<AiFile> Files,
    string SchemaName,
    string SchemaJson);

public sealed record OpenAiAnswer(string OutputJson, long InputTokens, long OutputTokens);

public enum OpenAiFailure
{
    InvalidKey,
    ModelNotFound,
    QuotaOrRateLimit,
    Rejected,
    Unavailable,
}

public sealed class OpenAiException(OpenAiFailure failure, string message) : Exception(message)
{
    public OpenAiFailure Failure { get; } = failure;
}
