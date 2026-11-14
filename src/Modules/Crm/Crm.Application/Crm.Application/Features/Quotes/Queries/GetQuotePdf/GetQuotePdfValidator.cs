namespace Crm.Application.Features.Quotes.Queries.GetQuotePdf;

public sealed class GetQuotePdfValidator : AbstractValidator<GetQuotePdfQuery>
{
    public GetQuotePdfValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
