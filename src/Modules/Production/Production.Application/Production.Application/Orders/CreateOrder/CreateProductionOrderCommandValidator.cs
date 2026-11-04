using Production.Application.Contracts;
using Production.Application.Orders.Validation;
using Production.Domain.Orders;

namespace Production.Application.Orders.CreateOrder;

public sealed class CreateProductionOrderCommandValidator : AbstractValidator<CreateProductionOrderCommand>
{
    public CreateProductionOrderCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull().SetValidator(new CreateProductionOrderInputValidator());
    }
}

public sealed class CreateProductionOrderInputValidator : AbstractValidator<CreateProductionOrderInput>
{
    public CreateProductionOrderInputValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.QuoteId).GreaterThan(0);
        RuleFor(x => x.ClientName).ClientName();
        RuleFor(x => x.OrderDate).NotEmpty().WithMessage("Вкажіть дату створення.");
        RuleFor(x => x.Comment).MaximumLength(ProductionOrder.CommentMaxLength);
        RuleFor(x => x.PriceTier).Must(x => Enum.TryParse<ReportPriceTier>(x, true, out var tier) && Enum.IsDefined(tier))
            .WithMessage("Оберіть ціну для звітів: роздріб, опт або великий опт.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Оберіть хоча б одну позицію.")
            .Must(x => x is null || x.Count <= ProductionOrder.MaxItems);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.QuoteItemId).GreaterThan(0);
            item.RuleFor(x => x.QuoteOptionId).GreaterThan(0);
            item.RuleFor(x => x.Details).NotNull().SetValidator(new ProductionItemDetailsInputValidator());
        });
    }
}
