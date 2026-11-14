using Ai.Application.Abstractions;
using Ai.Domain.Settings;

namespace Ai.Application.Features.Settings.Commands.RemoveAiKey;

public sealed class RemoveAiKeyValidator : AbstractValidator<RemoveAiKeyCommand>
{
    public RemoveAiKeyValidator() => RuleFor(x => x.By).NotEmpty().MaximumLength(AiSettings.UpdatedByMaxLength);
}
