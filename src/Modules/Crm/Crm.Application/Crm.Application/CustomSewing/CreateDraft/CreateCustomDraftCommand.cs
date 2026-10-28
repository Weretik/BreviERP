using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;
using Crm.Domain.CustomSewing.Composition;

namespace Crm.Application.CustomSewing.CreateDraft;

public sealed record CreateCustomDraftCommand(Guid RequestId, Guid? CustomerPublicId, Guid? QuotePublicId,
    CustomConfiguration Configuration, CrmActor Actor) : ICommand<Result<CustomDetail>>;
