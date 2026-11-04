using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Introductions.Contracts;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.Introductions.Templates.CreateTemplate;

/// <summary>Keeps the choice with the current product and fabric names for the list; no prices (R-19).</summary>
public sealed class CreateQuoteIntroTemplateCommandHandler(
    IQuoteStore store, IQuoteStaffReader staff, IQuoteCatalogReader catalog, TimeProvider clock)
    : ICommandHandler<CreateQuoteIntroTemplateCommand, Result<QuoteIntroTemplateResponse>>
{
    public async ValueTask<Result<QuoteIntroTemplateResponse>> Handle(CreateQuoteIntroTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var author = await staff.PersonAsync(command.Actor, cancellationToken);
        if (author is null) return Result.Forbidden(QuoteMessages.ActorUnknown);
        if (await store.QuoteIntroTemplates.CountAsync(cancellationToken) >= QuoteIntroTemplateLimits.MaxTemplates)
            return Result.Conflict(QuoteIntroTemplateMessages.TooMany);

        var input = command.Input;
        var ids = input.Pages.SelectMany(x => x.Models).Select(x => x.ProductId).Distinct().ToArray();
        var products = await catalog.GetAsync(ids, cancellationToken);
        var pages = new List<QuoteIntroTemplatePage>(input.Pages.Count);
        foreach (var page in input.Pages)
        {
            var models = new List<QuoteIntroTemplateModel>(page.Models.Count);
            foreach (var model in page.Models)
            {
                var variant = products.GetValueOrDefault(model.ProductId)?.Variants
                    .FirstOrDefault(x => x.ArticleId == model.ArticleId);
                if (variant is null) return Result.Conflict(QuoteIntroTemplateMessages.ModelMissing);
                models.Add(new QuoteIntroTemplateModel(model.ProductId, model.ArticleId,
                    products[model.ProductId].Name, variant.Title));
            }

            pages.Add(new QuoteIntroTemplatePage(Enum.Parse<QuoteIntroSeason>(page.Season), models));
        }

        var template = new QuoteIntroTemplate(input.Name, Enum.Parse<QuoteIntroLayout>(input.Layout), input.LetterName,
            pages, author, clock.GetUtcNow());
        store.QuoteIntroTemplates.Add(template);
        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (store.IsDuplicateTemplateName(exception))
        {
            return Result.Conflict(QuoteIntroTemplateMessages.Duplicate(template.Name));
        }

        return Result.Success(template.ToResponse(command.Actor));
    }
}
