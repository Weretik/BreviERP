using Ai.Domain.Settings;
using Ai.Application.Features.Calls.Shared;

namespace Ai.Application.Features.Settings.Commands.TestAiConnection;

public sealed class TestAiConnectionValidator : AbstractValidator<TestAiConnectionCommand>
{
    public TestAiConnectionValidator() => RuleFor(x => x.Tier).IsInEnum();
}
