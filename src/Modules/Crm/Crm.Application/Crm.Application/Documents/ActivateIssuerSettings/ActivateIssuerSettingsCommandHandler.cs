using Ardalis.Result;
using Crm.Application.Documents.Contracts;
using Crm.Application.Documents.Persistence;
using Mediator;

namespace Crm.Application.Documents.ActivateIssuerSettings;

public sealed class ActivateIssuerSettingsCommandHandler(IDocumentSettingsStore store)
    : ICommandHandler<ActivateIssuerSettingsCommand, Result<ActiveIssuerSettings>>
{
    public async ValueTask<Result<ActiveIssuerSettings>> Handle(ActivateIssuerSettingsCommand command, CancellationToken ct)
    {
        if (!command.Actor.IsAdmin) return Result.Forbidden();
        if (command.RequestId == Guid.Empty || command.ExpectedRevision <= 0 ||
            command.IssuerVersionId == Guid.Empty || command.TemplateVersionId == Guid.Empty)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        return await store.ActivateAsync(command.RequestId, command.ExpectedRevision,
            command.IssuerVersionId, command.TemplateVersionId, command.Actor, ct);
    }
}
