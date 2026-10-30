using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.CustomSewing.Templates;
using Mediator;

namespace Crm.Application.CustomSewing.GetTemplate;

public sealed record GetTemplateQuery(Guid TemplateId, Guid? VersionId, CrmActor Actor) : IQuery<Result<TemplateDetail>>;
