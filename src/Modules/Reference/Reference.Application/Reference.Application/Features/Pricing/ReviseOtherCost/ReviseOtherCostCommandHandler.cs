using Reference.Application.Contracts.Pricing;

namespace Reference.Application.Features.Pricing.ReviseOtherCost;

public sealed class ReviseOtherCostCommandHandler(IOtherCostPricingVersionWriter writer)
    : ICommandHandler<ReviseOtherCostCommand, Result<long>>
{
    public async ValueTask<Result<long>> Handle(ReviseOtherCostCommand command, CancellationToken cancellationToken)
    {
        return await writer.AppendAsync(command.Code, command.ExpectedVersionId, command.Value, command.RequestId, cancellationToken);
    }
}
