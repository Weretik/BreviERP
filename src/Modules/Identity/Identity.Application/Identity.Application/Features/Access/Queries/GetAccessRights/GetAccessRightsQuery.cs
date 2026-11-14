using Identity.Application.Features.Access.Shared;

namespace Identity.Application.Features.Access.Queries.GetAccessRights;

public sealed record GetAccessRightsQuery : IQuery<Result<AccessRightsDto>>;
