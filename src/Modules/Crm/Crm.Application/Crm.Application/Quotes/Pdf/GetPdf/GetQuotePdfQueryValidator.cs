namespace Crm.Application.Quotes.Pdf.GetPdf;

public sealed class GetQuotePdfQueryValidator : AbstractValidator<GetQuotePdfQuery>
{
    public GetQuotePdfQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
