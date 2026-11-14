using Production.Application.Abstractions;
using Production.Domain.Forecast;
using Production.Application.Features.Forecast.Shared;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Forecast.Commands.SaveForecastQueue;

public sealed class SaveForecastQueueValidator : AbstractValidator<SaveForecastQueueCommand>
{
    public SaveForecastQueueValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Version).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Input.OrderIds).NotNull().Must(x => x.Count <= ForecastQueue.MaxOrders);
            RuleFor(x => x.Input.OrderIds).Must(x => x.Distinct().Count() == x.Count)
                .WithMessage(ForecastMessages.Duplicate).When(x => x.Input.OrderIds is not null);
        });
    }
}
