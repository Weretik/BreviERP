using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Contracts;
using Mediator;

namespace Crm.Application.Documents.CreateIssuerSettingsVersion;

public sealed record CreateIssuerSettingsVersionCommand(Guid RequestId, IssuerSettingsInput Issuer,
    DocumentTemplateInput Template, CrmActor Actor) : ICommand<Result<IssuerSettingsDetail>>;
