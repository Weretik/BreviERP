using Ai.Domain.Settings;
using Ai.Application.Features.Calls.Shared;

namespace Ai.Application.Features.Settings.Commands.TestAiConnection;

/// <summary>«Перевірити»: a tiny request to the model of the tier proves the key and the model name.</summary>
public sealed record TestAiConnectionCommand(AiModelTier Tier) : ICommand<Result<AiTestResponse>>;

public sealed class TestAiConnectionValidator : AbstractValidator<TestAiConnectionCommand>
{
    public TestAiConnectionValidator() => RuleFor(x => x.Tier).IsInEnum();
}

public sealed class TestAiConnectionHandler(AiCaller caller)
    : ICommandHandler<TestAiConnectionCommand, Result<AiTestResponse>>
{
    private const string Schema = """{"type":"object","properties":{"ok":{"type":"boolean"}},"required":["ok"],"additionalProperties":false}""";

    public async ValueTask<Result<AiTestResponse>> Handle(TestAiConnectionCommand command, CancellationToken cancellationToken)
    {
        var result = await caller.CallAsync(command.Tier, "Answer with ok = true.", "ping", [], "connection_check", Schema,
            requireEnabled: false, cancellationToken);
        return result.Map(x => new AiTestResponse(command.Tier, x.Model, x.InputTokens, x.OutputTokens, x.CostUsd));
    }
}
