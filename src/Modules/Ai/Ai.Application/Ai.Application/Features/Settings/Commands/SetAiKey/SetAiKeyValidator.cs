using Ai.Application.Abstractions;
using Ai.Domain.Settings;

namespace Ai.Application.Features.Settings.Commands.SetAiKey;

public sealed class SetAiKeyValidator : AbstractValidator<SetAiKeyCommand>
{
    public const string Message = "Ключ OpenAI — від 20 до 300 символів без пробілів.";

    public SetAiKeyValidator()
    {
        RuleFor(x => x.ApiKey).Must(key => key is { Length: >= 20 and <= 300 } && !key.Any(char.IsWhiteSpace))
            .WithMessage(Message);
        RuleFor(x => x.By).NotEmpty().MaximumLength(AiSettings.UpdatedByMaxLength);
    }
}
