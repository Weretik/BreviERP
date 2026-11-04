using Crm.Application.Quotes.Persistence;

namespace Crm.Application.Quotes.Introductions.Templates.DeleteTemplate;

public sealed class DeleteQuoteIntroTemplateCommandHandler(IQuoteStore store)
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
