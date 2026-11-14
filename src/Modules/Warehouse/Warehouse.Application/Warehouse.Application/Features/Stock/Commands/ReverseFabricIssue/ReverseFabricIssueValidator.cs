using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;

using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.ReverseFabricIssue;

public sealed class ReverseFabricIssueValidator : AbstractValidator<ReverseFabricIssueCommand>
{
    public ReverseFabricIssueValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.IssueId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OrderVersion).GreaterThan(0);
            RuleFor(x => x.Input.Reason).Reason();
        });
    }
}
