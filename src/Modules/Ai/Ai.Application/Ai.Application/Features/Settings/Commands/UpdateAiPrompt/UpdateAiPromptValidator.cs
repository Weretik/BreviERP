using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Domain.Prompts;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.UpdateAiPrompt;

public sealed class UpdateAiPromptValidator : AbstractValidator<UpdateAiPromptCommand>
{
    public UpdateAiPromptValidator()
    {
        RuleFor(x => x.Text).Must(text => text is null || text.Trim().Length <= AiPrompt.TextMaxLength)
            .WithMessage($"Умови — не більше {AiPrompt.TextMaxLength} символів.");
        RuleFor(x => x.By).NotEmpty().MaximumLength(AiSettings.UpdatedByMaxLength);
    }
}
