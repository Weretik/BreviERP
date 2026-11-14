using Crm.Application.Features.Quotes.Shared.Introductions.Templates;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Commands.DeleteQuoteIntroTemplate;

public sealed class DeleteQuoteIntroTemplateHandler(IQuoteStore store)
    : ICommandHandler<DeleteQuoteIntroTemplateCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteQuoteIntroTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await store.QuoteIntroTemplates.SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (template is null) return Result.NotFound(QuoteIntroTemplateMessages.NotFound);
        if (!command.Actor.CanDeleteTemplate(template)) return Result.Forbidden(QuoteIntroTemplateMessages.DeleteForbidden);

        store.QuoteIntroTemplates.Remove(template);
        await store.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
