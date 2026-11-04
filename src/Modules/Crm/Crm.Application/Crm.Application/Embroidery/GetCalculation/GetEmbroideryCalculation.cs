using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Persistence;

namespace Crm.Application.Embroidery.GetCalculation;

/// <summary>One saved calculation as it was saved; a Manager gets no cost and no rates.</summary>
public sealed record GetEmbroideryCalculationQuery(EmbroideryActor Actor, long Id)
    : IQuery<Result<EmbroiderySavedCalculationResponse>>;

public sealed class GetEmbroideryCalculationQueryHandler(IEmbroideryStore store)
    : IQueryHandler<GetEmbroideryCalculationQuery, Result<EmbroiderySavedCalculationResponse>>
{
    public async ValueTask<Result<EmbroiderySavedCalculationResponse>> Handle(GetEmbroideryCalculationQuery query,
        CancellationToken cancellationToken)
    {
        var saved = await store.EmbroideryCalculations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        return saved is null
            ? Result.NotFound(EmbroideryMessages.CalculationNotFound)
            : Result.Success(saved.ToResponse(query.Actor.SeesCost));
    }
}

public sealed class GetEmbroideryCalculationQueryValidator : AbstractValidator<GetEmbroideryCalculationQuery>
{
    public GetEmbroideryCalculationQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
