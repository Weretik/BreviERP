using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Domain.Orders.Enums;

namespace Production.Application.Features.Orders.Commands.SetProductionItemStages;

public sealed class SetProductionItemStagesValidator : AbstractValidator<SetProductionItemStagesCommand>
{
    public SetProductionItemStagesValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
        RuleFor(x => x.Input.Stages).NotNull().Must(x => x.Count <= 9).When(x => x.Input is not null);
        RuleForEach(x => x.Input.Stages)
            .Must(x => Enum.TryParse<ProductionStage>(x, true, out var stage) && Enum.IsDefined(stage))
            .WithMessage("Невідомий етап.")
            .When(x => x.Input?.Stages is not null);
    }
}
