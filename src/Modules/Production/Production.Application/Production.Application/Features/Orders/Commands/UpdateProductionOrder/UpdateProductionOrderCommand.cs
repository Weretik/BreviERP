using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.UpdateProductionOrder;

/// <summary>Saves the header and the item details; every changed field goes to «Історія змін».</summary>
public sealed record UpdateProductionOrderCommand(ProductionActor Actor, int Id, UpdateProductionOrderInput Input)
    : ICommand<Result<ProductionOrderResponse>>;

public sealed class UpdateProductionOrderValidator : AbstractValidator<UpdateProductionOrderCommand>
{
    public UpdateProductionOrderValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
        RuleFor(x => x.Input.ClientName).ClientName().When(x => x.Input is not null);
        RuleFor(x => x.Input.OrderDate).NotEmpty().When(x => x.Input is not null);
        RuleFor(x => x.Input.Comment).MaximumLength(ProductionOrder.CommentMaxLength).When(x => x.Input is not null);
        RuleFor(x => x.Input.Items).NotNull().When(x => x.Input is not null);
        RuleForEach(x => x.Input.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Id).GreaterThan(0);
            item.RuleFor(x => x.Details).NotNull().SetValidator(new ProductionItemDetailsInputValidator());
            item.RuleFor(x => x.SewingRate).InclusiveBetween(0m, 100_000m).When(x => x.SewingRate is not null)
                .WithMessage("Розцінка — сума від 0 грн.");
        }).When(x => x.Input?.Items is not null);
    }
}
