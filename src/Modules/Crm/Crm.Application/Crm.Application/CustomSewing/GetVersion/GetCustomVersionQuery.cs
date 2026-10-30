using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;

namespace Crm.Application.CustomSewing.GetVersion;

public sealed record GetCustomVersionQuery(Guid DraftId, Guid VersionId, CrmActor Actor) : IQuery<Result<CustomVersionView>>;
