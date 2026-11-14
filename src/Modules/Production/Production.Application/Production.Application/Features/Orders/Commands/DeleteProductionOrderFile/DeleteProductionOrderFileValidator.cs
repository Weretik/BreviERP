using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.DeleteProductionOrderFile;

public sealed class DeleteProductionOrderFileValidator : AbstractValidator<DeleteProductionOrderFileCommand>
{
    public DeleteProductionOrderFileValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.FileId).GreaterThan(0);
    }
}
