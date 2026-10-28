using Ardalis.Result;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Application.CustomSewing.Templates.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Templates;
using Mediator;

namespace Crm.Application.CustomSewing.ReviseTemplate;

public sealed class ReviseTemplateCommandHandler(ITemplateMutationStore store)
    : ICommandHandler<ReviseTemplateCommand, Result<TemplateDetail>>
{
    public async ValueTask<Result<TemplateDetail>> Handle(ReviseTemplateCommand command, CancellationToken cancellationToken)
    {
        if (command.Actor.UserId <= 0 || command.RequestId == Guid.Empty || command.TemplateId == Guid.Empty || command.ExpectedRevision <= 0)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        return await store.ExecuteAsync("ReviseTemplate", command.RequestId, command.Actor, command, async (session, ct) =>
        {
            var template = await session.FindAsync(command.TemplateId, command.Actor, ct);
            if (template is null) return Result.NotFound();
            if (template.Revision != command.ExpectedRevision) return Result.Conflict("revision_conflict");
            if (template.Archived) return Result.Conflict("template_archived");
            var version = CustomTemplateVersion.Create(template.Id, template.VersionNumber + 1, command.Introduction, command.Selections);
            template.RecordVersion(command.ExpectedRevision, version);
            session.Add(version);
            await session.FlushAsync(ct);
            return Result.Success(await session.DetailAsync(template, command.Actor, ct));
        }, cancellationToken);
    }
}
