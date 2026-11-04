namespace Crm.Application.Quotes.Introductions.Templates.GetTemplates;

public sealed class GetQuoteIntroTemplatesQueryValidator : AbstractValidator<GetQuoteIntroTemplatesQuery>
{
    public GetQuoteIntroTemplatesQueryValidator() => RuleFor(x => x.Actor).NotNull();
}
