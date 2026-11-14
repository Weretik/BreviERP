using Identity.Application.Features.Access.Shared;

namespace Identity.Application.Features.Access.Queries.GetMyAccess;

public sealed record GetMyAccessQuery : IQuery<Result<MyAccessDto>>;
