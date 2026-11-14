using Ardalis.Result;

namespace Ai.Contracts.Extraction;

/// <summary>
/// Reads structured data (JSON by a schema) from text and files with the AI of «Налаштування ШІ». The answer is
/// <c>Unavailable</c> when the AI is switched off, has no key or reached its monthly limit.
/// </summary>
public interface IStructuredDataExtractor
{
    Task<Result<StructuredDataAnswer>> ExtractAsync(StructuredDataRequest request, CancellationToken cancellationToken);
}

/// <summary>Which of the two configured models answers.</summary>
public enum AiModelChoice
{
    Fast,
    Strong
}

/// <param name="PromptKey">One of <see cref="Prompts.AiPromptKeys"/>; adds the admin's extra rules for that kind.</param>
public sealed record StructuredDataRequest(
    AiModelChoice Model,
    string Instructions,
    string Text,
    IReadOnlyList<AiInputFile> Files,
    string SchemaName,
    string SchemaJson,
    string? PromptKey = null);

public sealed record AiInputFile(string FileName, string MediaType, byte[] Content);

public sealed record StructuredDataAnswer(string Json, string Model, decimal CostUsd);
