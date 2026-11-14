using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Commands.CreateProductionWork;

public sealed class CreateProductionWorkValidator : AbstractValidator<CreateProductionWorkCommand>
{
    public CreateProductionWorkValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.EmployeeId).GreaterThan(0).WithMessage("Оберіть працівника.");
            RuleFor(x => x.Input.Kind).Must(x => Enum.TryParse<ProductionWorkKind>(x, out var k) && Enum.IsDefined(k))
                .WithMessage("Оберіть пошив або крій.");
            RuleFor(x => x.Input.Quantity).InclusiveBetween(1, ProductionWork.MaxQuantity)
                .WithMessage("Кількість — ціле число від 1.");
            RuleFor(x => x.Input.WorkDate).NotEmpty().WithMessage("Вкажіть дату роботи.");
            RuleFor(x => x.Input.Reason).Must(OverPlanReasons.IsKnown).WithMessage(WorkMessages.Reason)
                .When(x => x.Input.OverPlan);
            RuleFor(x => x.Input.Comment).MaximumLength(ProductionWork.CommentMaxLength);
        });
    }
}
