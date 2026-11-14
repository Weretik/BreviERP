namespace Crm.Application.Features.Quotes.Queries.GetQuoteIntroTemplates;

public sealed class GetQuoteIntroTemplatesValidator : AbstractValidator<GetQuoteIntroTemplatesQuery>
{
    public GetQuoteIntroTemplatesValidator() => RuleFor(x => x.Actor).NotNull();
}
