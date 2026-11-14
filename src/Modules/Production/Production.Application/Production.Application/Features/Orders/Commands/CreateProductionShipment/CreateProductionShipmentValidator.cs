using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.CreateProductionShipment;

public sealed class CreateProductionShipmentValidator : AbstractValidator<CreateProductionShipmentCommand>
{
    public CreateProductionShipmentValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
        RuleFor(x => x.Input.Quantity).InclusiveBetween(1, ProductionItem.MaxQuantity)
            .WithMessage("Кількість — ціле число від 1.").When(x => x.Input is not null);
        RuleFor(x => x.Input.ShippedOn).NotEmpty().WithMessage("Вкажіть дату відвантаження.")
            .When(x => x.Input is not null);
        RuleFor(x => x.Input.Comment).MaximumLength(ProductionShipment.CommentMaxLength)
            .When(x => x.Input is not null);
    }
}
