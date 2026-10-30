using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.CustomSewing.Templates;
using Mediator;

namespace Crm.Application.CustomSewing.ReviseTemplate;

public sealed record ReviseTemplateCommand(Guid TemplateId, long ExpectedRevision, Guid RequestId, string Introduction, TemplateSelections Selections, CrmActor Actor) : ICommand<Result<TemplateDetail>>;
