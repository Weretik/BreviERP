using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Work.Commands.VoidProductionWork;

public sealed class VoidProductionWorkValidator : AbstractValidator<VoidProductionWorkCommand>
{
    public VoidProductionWorkValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.WorkId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Reason).NotEmpty().WithMessage("Вкажіть причину сторно.")
            .MaximumLength(ProductionWork.ReasonMaxLength).When(x => x.Input is not null);
    }
}
