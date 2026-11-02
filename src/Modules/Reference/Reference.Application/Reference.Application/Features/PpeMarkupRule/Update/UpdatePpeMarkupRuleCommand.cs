using Reference.Application.Features.PpeMarkupRule.Update.DTOs;

namespace Reference.Application.Features.PpeMarkupRule.Update;

public sealed record UpdatePpeMarkupRuleCommand(int Id, UpdatePpeMarkupRuleCommandRequest Request) : ICommand<Result>;
