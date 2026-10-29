using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Contracts;
using Mediator;

namespace Crm.Application.Documents.ActivateIssuerSettings;

public sealed record ActivateIssuerSettingsCommand(Guid RequestId, long ExpectedRevision,
    Guid IssuerVersionId, Guid TemplateVersionId, CrmActor Actor) : ICommand<Result<ActiveIssuerSettings>>;
