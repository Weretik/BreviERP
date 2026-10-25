using Reference.Application.Contracts.Pricing;

namespace Reference.Application.Features.Pricing.ReviseOtherCost;

public sealed class ReviseOtherCostCommandHandler(IOtherCostPricingVersionWriter writer)
    : ICommandHandler<ReviseOtherCostCommand, Result<long>>
{
    public async ValueTask<Result<long>> Handle(ReviseOtherCostCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Code) || command.Code.Length > 100 || command.ExpectedVersionId <= 0
            || command.RequestId == Guid.Empty || command.Value < 0 || command.Value > 999999999999.99999999m
            || decimal.Round(command.Value, 8) != command.Value)
            return Result.Invalid(new ValidationError("request", "Valid cost code, current version, request UUID and nonnegative value with at most 8 decimals are required."));
        return await writer.AppendAsync(command.Code, command.ExpectedVersionId, command.Value, command.RequestId, cancellationToken);
    }
}
