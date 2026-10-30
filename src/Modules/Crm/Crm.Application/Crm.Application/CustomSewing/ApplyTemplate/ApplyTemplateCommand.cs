using Ardalis.Result;
using Crm.Application.Customers.Access;
using Mediator;
namespace Crm.Application.CustomSewing.ApplyTemplate;

public sealed record ApplyTemplateCommand(Guid TemplateId, Guid VersionId, long ExpectedRevision, Guid RequestId,
    Guid? CustomerPublicId, Guid? QuotePublicId, CrmActor Actor) : ICommand<Result<AppliedTemplate>>;
