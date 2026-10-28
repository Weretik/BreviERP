using Ardalis.Result;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Application.CustomSewing.Templates.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Templates;
using Mediator;

namespace Crm.Application.CustomSewing.ArchiveTemplate;

public sealed class ArchiveTemplateCommandHandler(ITemplateMutationStore store)
    : ICommandHandler<ArchiveTemplateCommand, Result<TemplateDetail>>
{
    public async ValueTask<Result<TemplateDetail>> Handle(ArchiveTemplateCommand command, CancellationToken cancellationToken)
    {
        if (command.Actor.UserId <= 0 || command.RequestId == Guid.Empty || command.TemplateId == Guid.Empty || command.ExpectedRevision <= 0)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        return await store.ExecuteAsync("ArchiveTemplate", command.RequestId, command.Actor, command, async (session, ct) =>
        {
            var template = await session.FindAsync(command.TemplateId, command.Actor, ct);
            if (template is null) return Result.NotFound();
            if (template.Revision != command.ExpectedRevision) return Result.Conflict("revision_conflict");
            template.SetArchived(command.ExpectedRevision, command.Archived);
            await session.FlushAsync(ct);
            return Result.Success(await session.DetailAsync(template, command.Actor, ct));
        }, cancellationToken);
    }
}
