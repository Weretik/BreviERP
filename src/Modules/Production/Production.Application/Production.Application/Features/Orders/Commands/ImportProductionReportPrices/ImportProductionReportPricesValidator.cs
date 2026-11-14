using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.ImportProductionReportPrices;

public sealed class ImportProductionReportPricesValidator
    : AbstractValidator<ImportProductionReportPricesCommand>
{
    public ImportProductionReportPricesValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Rows).NotNull().Must(x => x.Count is > 0 and <= 1000)
            .WithMessage("У файлі має бути від 1 до 1000 завдань.");
        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(ProductionOrder.LegacyIdMaxLength);
            row.RuleFor(x => x.Items).NotNull();
            row.RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.Position).GreaterThan(0);
                item.RuleFor(x => x.UnitPrice).GreaterThan(0).LessThan(10_000_000m)
                    .WithMessage("Ціна має бути більшою за нуль.");
            }).When(x => x.Items is not null);
        }).When(x => x.Rows is not null);
    }
}
