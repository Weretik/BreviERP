using Reference.Application.Features.PpeMarkupRule.Create.DTOs;

namespace Reference.Application.Features.PpeMarkupRule.Create;

public sealed record CreatePpeMarkupRuleCommand(CreatePpeMarkupRuleCommandRequest Request) : ICommand<Result<int>>;
