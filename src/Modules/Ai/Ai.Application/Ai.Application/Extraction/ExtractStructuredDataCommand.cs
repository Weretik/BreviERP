using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Application.Calls;
using Ai.Application.Contracts;
using Ai.Domain.Settings;

namespace Ai.Application.Extraction;

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

public sealed class ExtractStructuredDataCommandValidator : AbstractValidator<ExtractStructuredDataCommand>
{
    public const int MaxFileBytes = 20 * 1024 * 1024;

    public ExtractStructuredDataCommandValidator()
    {
        RuleFor(x => x.Tier).IsInEnum();
        RuleFor(x => x.Instructions).NotEmpty().MaximumLength(20_000);
        RuleFor(x => x.Text).NotNull().MaximumLength(20_000);
        RuleFor(x => x.Files).NotNull().Must(x => x.Count <= 5);
        RuleForEach(x => x.Files).Must(x => x.Content.Length is > 0 and <= MaxFileBytes && !string.IsNullOrWhiteSpace(x.FileName));
        RuleFor(x => x.SchemaName).Matches("^[a-zA-Z0-9_-]{1,64}$");
        RuleFor(x => x.SchemaJson).NotEmpty();
        RuleFor(x => x.PromptKey).Must(key => key is null || AiPromptKeys.IsKnown(key));
    }
}

public sealed class ExtractStructuredDataCommandHandler(AiCaller caller)
    : ICommandHandler<ExtractStructuredDataCommand, Result<AiExtraction>>
{
    public async ValueTask<Result<AiExtraction>> Handle(ExtractStructuredDataCommand command, CancellationToken cancellationToken) =>
        await caller.CallAsync(command.Tier, command.Instructions, command.Text, command.Files, command.SchemaName,
            command.SchemaJson, requireEnabled: true, cancellationToken, command.PromptKey);
}
