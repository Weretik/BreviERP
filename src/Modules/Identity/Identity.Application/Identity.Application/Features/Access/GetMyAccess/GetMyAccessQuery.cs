using Identity.Application.Features.Access.Shared;

namespace Identity.Application.Features.Access.GetMyAccess;

public sealed record GetMyAccessQuery : IQuery<Result<MyAccessDto>>;
