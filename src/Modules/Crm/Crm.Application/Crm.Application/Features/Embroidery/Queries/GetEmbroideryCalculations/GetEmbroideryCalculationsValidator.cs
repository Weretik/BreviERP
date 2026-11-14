using Crm.Domain.Embroidery.Saved;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculations;

public sealed class GetEmbroideryCalculationsValidator : AbstractValidator<GetEmbroideryCalculationsQuery>
{
    public const int MaxPageSize = 100;

    public GetEmbroideryCalculationsValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Search).MaximumLength(EmbroiderySavedCalculation.NameMaxLength);
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
