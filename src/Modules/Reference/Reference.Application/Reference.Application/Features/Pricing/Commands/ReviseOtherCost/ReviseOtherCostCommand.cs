namespace Reference.Application.Features.Pricing.Commands.ReviseOtherCost;

public sealed record ReviseOtherCostCommand(string Code, long ExpectedVersionId, decimal Value, Guid RequestId)
    : ICommand<Result<long>>;
