using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Mockup;
using Crm.Application.Embroidery.Persistence;

namespace Crm.Application.Embroidery.GetCalculation;

/// <summary>One saved calculation as it was saved; a Manager gets no cost and no rates.</summary>
public sealed record GetEmbroideryCalculationQuery(EmbroideryActor Actor, long Id)
    : IQuery<Result<EmbroiderySavedCalculationResponse>>;

public sealed class GetEmbroideryCalculationQueryHandler(IEmbroideryStore store, IEmbroideryMediaReader media)
    : IQueryHandler<GetEmbroideryCalculationQuery, Result<EmbroiderySavedCalculationResponse>>
{
    public async ValueTask<Result<EmbroiderySavedCalculationResponse>> Handle(GetEmbroideryCalculationQuery query,
        CancellationToken cancellationToken)
    {
        var saved = await store.EmbroideryCalculations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (saved is null) return Result.NotFound(EmbroideryMessages.CalculationNotFound);
        var mockups = await media.ReadForAsync([saved.MockupMediaFileId], cancellationToken);
        return Result.Success(saved.ToResponse(query.Actor.SeesCost, mockups.Of(saved.MockupMediaFileId)));
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
