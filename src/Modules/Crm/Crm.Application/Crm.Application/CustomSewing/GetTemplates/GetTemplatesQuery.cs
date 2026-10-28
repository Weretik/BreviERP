using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.CustomSewing.Templates;
using Mediator;

namespace Crm.Application.CustomSewing.GetTemplates;

public sealed record GetTemplatesQuery(CrmActor Actor, bool IncludeArchived, int Page, int PageSize) : IQuery<PagedResult<IReadOnlyList<TemplateSummary>>>;
