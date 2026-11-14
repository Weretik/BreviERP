using Reference.Application.Abstractions;
using Reference.Application.Abstractions.Pricing;

namespace Reference.Application.Features.Pricing.Commands.ReviseOtherCost;

public sealed class ReviseOtherCostHandler(IOtherCostPricingVersionWriter writer)
    : ICommandHandler<ReviseOtherCostCommand, Result<long>>
{
    public async ValueTask<Result<long>> Handle(ReviseOtherCostCommand command, CancellationToken cancellationToken)
    {
        return await writer.AppendAsync(command.Code, command.ExpectedVersionId, command.Value, command.RequestId, cancellationToken);
    }
}
