using Ardalis.Result;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Application.CustomSewing.Templates.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Templates;
using Mediator;

namespace Crm.Application.CustomSewing.CreateTemplate;

public sealed class CreateTemplateCommandHandler(ITemplateMutationStore store)
    : ICommandHandler<CreateTemplateCommand, Result<TemplateDetail>>
{
    public async ValueTask<Result<TemplateDetail>> Handle(CreateTemplateCommand command, CancellationToken cancellationToken)
    {
        if (command.Actor.UserId <= 0 || command.RequestId == Guid.Empty)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        return await store.ExecuteAsync("CreateTemplate", command.RequestId, command.Actor, command, async (session, ct) =>
        {
            var template = CustomTemplate.Create(new CrmUserId(command.Actor.UserId));
            session.Add(template);
            await session.FlushAsync(ct);
            if (template.Archived) return Result.Conflict("template_archived");
            var version = CustomTemplateVersion.Create(template.Id, template.VersionNumber + 1, command.Introduction, command.Selections);
            template.RecordVersion(template.Revision, version);
            session.Add(version);
            await session.FlushAsync(ct);
            return Result.Success(await session.DetailAsync(template, command.Actor, ct));
        }, cancellationToken);
    }
}
