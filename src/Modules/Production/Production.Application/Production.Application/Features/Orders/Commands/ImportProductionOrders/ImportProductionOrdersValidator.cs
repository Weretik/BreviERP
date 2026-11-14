using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.ImportProductionOrders;

public sealed class ImportProductionOrdersValidator : AbstractValidator<ImportProductionOrdersCommand>
{
    public const int MaxRows = 2000;

    public ImportProductionOrdersValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Rows).NotEmpty().Must(x => x is null || x.Count <= MaxRows);
        RuleForEach(x => x.Rows).SetValidator(new ProductionOrderImportRowValidator());
    }
}

public sealed class ProductionOrderImportRowValidator : AbstractValidator<ProductionOrderImportRow>
{
    public ProductionOrderImportRowValidator()
    {
        RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(ProductionOrder.LegacyIdMaxLength);
        RuleFor(x => x.Number).GreaterThan(0);
        RuleFor(x => x.QuoteNumber).MaximumLength(ProductionOrder.QuoteNumberMaxLength);
        RuleFor(x => x.ClientName).ClientName();
        RuleFor(x => x.ManagerEmail).MaximumLength(256);
        RuleFor(x => x.ManagerName).NotEmpty().MaximumLength(ProductionOrder.PersonMaxLength);
        RuleFor(x => x.Status).Must(x => Enum.TryParse<ProductionOrderStatus>(x, true, out var s) && Enum.IsDefined(s))
            .WithMessage("Невідомий статус.");
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(ProductionOrder.CommentMaxLength);
        RuleFor(x => x.Items).NotEmpty().Must(x => x is null || x.Count <= ProductionOrder.MaxItems);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ModelCode).MaximumLength(ProductionItem.CodeMaxLength);
            item.RuleFor(x => x.ModelName).NotEmpty().MaximumLength(ProductionItem.NameMaxLength);
            item.RuleFor(x => x.FabricCode).MaximumLength(ProductionItem.CodeMaxLength);
            item.RuleFor(x => x.FabricName).MaximumLength(ProductionItem.NameMaxLength);
            item.RuleFor(x => x.SewingRate).InclusiveBetween(0m, 100_000m).When(x => x.SewingRate is not null);
            item.RuleFor(x => x.Details).NotNull().SetValidator(new ProductionItemDetailsInputValidator());
        });
    }
}
