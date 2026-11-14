using Identity.Application.Abstractions;

namespace Identity.Application.Features.Auth.Queries.SessionMe;

public sealed class SessionMeHandler(ICookieSessionService cookieSessionService)
    : IQueryHandler<SessionMeQuery, Result<SessionMeResult>>
{
    public async ValueTask<Result<SessionMeResult>> Handle(SessionMeQuery query, CancellationToken cancellationToken)
    {
        var result = await cookieSessionService.GetCurrentUserAsync(cancellationToken);
        return result is null ? Result.Unauthorized() : Result.Success(result);
    }
}
