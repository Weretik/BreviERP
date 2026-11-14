using Production.Application.Abstractions;
using Production.Domain;
using Production.Domain.Orders;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Commands.ImportProductionWork;

public sealed class ImportProductionWorkValidator : AbstractValidator<ImportProductionWorkCommand>
{
    public ImportProductionWorkValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Rows).NotNull().Must(x => x.Count is > 0 and <= 5000)
            .WithMessage("У файлі має бути від 1 до 5000 записів.");
        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(ProductionWork.LegacyIdMaxLength);
            row.RuleFor(x => x.OrderLegacyId).NotEmpty().MaximumLength(ProductionOrder.LegacyIdMaxLength);
            row.RuleFor(x => x.Position).GreaterThan(0);
            row.RuleFor(x => x.EmployeeName).NotEmpty().MaximumLength(ProductionOrder.PersonMaxLength);
            row.RuleFor(x => x.Kind).Must(x => Enum.TryParse<ProductionWorkKind>(x, out var k) && Enum.IsDefined(k))
                .WithMessage("Невідомий вид роботи.");
            row.RuleFor(x => x.Quantity).InclusiveBetween(1, ProductionWork.MaxQuantity);
            row.RuleFor(x => x.Rate).GreaterThan(0);
            row.RuleFor(x => x.Reason).Must(OverPlanReasons.IsKnown).WithMessage(WorkMessages.Reason)
                .When(x => x.OverPlan);
            row.RuleFor(x => x.Comment).MaximumLength(ProductionWork.CommentMaxLength);
            row.RuleFor(x => x.VoidReason).MaximumLength(ProductionWork.ReasonMaxLength);
        }).When(x => x.Rows is not null);
    }
}
