namespace Reference.Application.Features.PpeMarkupRule.Delete;

public sealed record DeletePpeMarkupRuleCommand(int Id) : ICommand<Result>;
