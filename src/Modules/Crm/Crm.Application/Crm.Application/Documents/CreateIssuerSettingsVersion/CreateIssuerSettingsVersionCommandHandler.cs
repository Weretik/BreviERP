using Ardalis.Result;
using Crm.Application.Documents.Contracts;
using Crm.Application.Documents.Persistence;
using Mediator;

namespace Crm.Application.Documents.CreateIssuerSettingsVersion;

public sealed class CreateIssuerSettingsVersionCommandHandler(IDocumentSettingsStore store)
    : ICommandHandler<CreateIssuerSettingsVersionCommand, Result<IssuerSettingsDetail>>
{
    public async ValueTask<Result<IssuerSettingsDetail>> Handle(CreateIssuerSettingsVersionCommand command, CancellationToken ct)
    {
        if (!command.Actor.IsAdmin) return Result.Forbidden();
        if (command.RequestId == Guid.Empty || command.Issuer is null || command.Template is null)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        return await store.CreateVersionAsync(command.RequestId, command.Issuer, command.Template, command.Actor, ct);
    }
}
