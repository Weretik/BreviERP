
namespace Reference.Application.Features.PpeMarkupRule.Commands.CreatePpeMarkupRule;

public sealed record CreatePpeMarkupRuleCommand(CreatePpeMarkupRuleCommandRequest Request) : ICommand<Result<int>>;
