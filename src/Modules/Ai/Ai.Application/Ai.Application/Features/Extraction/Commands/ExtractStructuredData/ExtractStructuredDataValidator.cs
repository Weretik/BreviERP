using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Calls.Shared;

namespace Ai.Application.Features.Extraction.Commands.ExtractStructuredData;

public sealed class ExtractStructuredDataValidator : AbstractValidator<ExtractStructuredDataCommand>
{
    public const int MaxFileBytes = 20 * 1024 * 1024;

    public ExtractStructuredDataValidator()
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
