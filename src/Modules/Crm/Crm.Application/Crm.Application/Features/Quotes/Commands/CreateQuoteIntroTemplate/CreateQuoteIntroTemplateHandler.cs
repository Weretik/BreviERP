using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Introductions;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Introductions;
using Crm.Application.Features.Quotes.Shared.Introductions.Templates;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Application.Abstractions.Quotes.Products;

namespace Crm.Application.Features.Quotes.Commands.CreateQuoteIntroTemplate;

/// <summary>Keeps the choice with the current product and fabric names for the list; no prices (R-19).</summary>
public sealed class CreateQuoteIntroTemplateHandler(
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

        var created = QuoteIntroTemplate.Create(input.Name, Enum.Parse<QuoteIntroLayout>(input.Layout), input.LetterName,
            pages, author, clock.GetUtcNow());
        if (created.IsFailure) return created.Error!.ToResult<QuoteIntroTemplateResponse>();
        var template = created.Value;
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
