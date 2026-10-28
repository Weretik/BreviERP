using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.CustomSewing.Templates;
using Mediator;

namespace Crm.Application.CustomSewing.ArchiveTemplate;

public sealed record ArchiveTemplateCommand(Guid TemplateId, long ExpectedRevision, Guid RequestId, bool Archived, CrmActor Actor) : ICommand<Result<TemplateDetail>>;
