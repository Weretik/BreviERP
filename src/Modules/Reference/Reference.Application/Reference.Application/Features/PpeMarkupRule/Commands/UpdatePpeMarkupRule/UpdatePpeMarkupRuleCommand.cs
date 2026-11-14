
namespace Reference.Application.Features.PpeMarkupRule.Commands.UpdatePpeMarkupRule;

public sealed record UpdatePpeMarkupRuleCommand(int Id, UpdatePpeMarkupRuleCommandRequest Request) : ICommand<Result>;
