using Crm.Domain.Quotes.Introductions;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Introductions;

namespace Crm.Application.Features.Quotes.Shared.Introductions.Templates;

internal static class QuoteIntroTemplateMapper
{
    public static QuoteIntroTemplateResponse ToResponse(this QuoteIntroTemplate template, QuoteActor actor) => new(
        template.Id, template.Name, template.Layout.ToString(), template.LetterName,
        template.Pages.Select(page => new QuoteIntroTemplatePageResponse(page.Season.ToString(),
            page.Models.Select(x => new QuoteIntroTemplateModelResponse(x.ProductId, x.ArticleId, x.ProductName,
                x.FabricTitle)).ToArray())).ToArray(),
        new QuotePersonResponse(template.AuthorUserId, template.AuthorName), template.CreatedAt,
        actor.CanDeleteTemplate(template));

    /// <summary>R-19: the author, Director or Admin.</summary>
    public static bool CanDeleteTemplate(this QuoteActor actor, QuoteIntroTemplate template) =>
        actor.CanSeeAll || template.AuthorUserId == actor.UserId;
}
