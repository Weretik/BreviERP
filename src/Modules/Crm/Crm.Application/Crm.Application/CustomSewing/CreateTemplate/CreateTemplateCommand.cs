using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.CustomSewing.Templates;
using Mediator;

namespace Crm.Application.CustomSewing.CreateTemplate;

public sealed record CreateTemplateCommand(Guid RequestId, string Introduction, TemplateSelections Selections, CrmActor Actor) : ICommand<Result<TemplateDetail>>;
