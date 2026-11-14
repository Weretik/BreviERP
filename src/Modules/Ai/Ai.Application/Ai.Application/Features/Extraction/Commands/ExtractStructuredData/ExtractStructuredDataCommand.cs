using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Calls.Shared;

namespace Ai.Application.Features.Extraction.Commands.ExtractStructuredData;

/// <summary>
/// For other modules (through a host adapter): reads facts from documents into JSON of the given strict schema.
/// Unavailable when the AI is off, has no key or reached the monthly limit; Error when OpenAI failed.
/// </summary>
public sealed record ExtractStructuredDataCommand(
    AiModelTier Tier,
    string Instructions,
    string Text,
    IReadOnlyList<AiFile> Files,
    string SchemaName,
    string SchemaJson,
    string? PromptKey = null) : ICommand<Result<AiExtraction>>;
