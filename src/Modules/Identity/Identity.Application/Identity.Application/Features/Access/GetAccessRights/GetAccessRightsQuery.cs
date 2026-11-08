using Identity.Application.Features.Access.Shared;

namespace Identity.Application.Features.Access.GetAccessRights;

public sealed record GetAccessRightsQuery : IQuery<Result<AccessRightsDto>>;
