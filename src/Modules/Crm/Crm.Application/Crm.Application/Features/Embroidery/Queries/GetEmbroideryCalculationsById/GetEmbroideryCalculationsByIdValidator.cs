namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculationsById;

public sealed class GetEmbroideryCalculationsByIdValidator : AbstractValidator<GetEmbroideryCalculationsByIdQuery>
{
    public GetEmbroideryCalculationsByIdValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Ids).NotNull().Must(x => x.Count <= GetEmbroideryCalculationsByIdQuery.MaxIds)
            .WithMessage($"Не більше {GetEmbroideryCalculationsByIdQuery.MaxIds} розрахунків за раз.");
        RuleForEach(x => x.Ids).GreaterThan(0);
    }
}
