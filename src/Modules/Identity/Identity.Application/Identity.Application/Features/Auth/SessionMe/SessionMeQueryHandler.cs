using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Services;

namespace Identity.Application.Features.Auth.SessionMe;

public sealed class SessionMeQueryHandler(ICookieSessionService cookieSessionService)
    : IQueryHandler<SessionMeQuery, Result<SessionMeResult>>
{
    public async ValueTask<Result<SessionMeResult>> Handle(SessionMeQuery query, CancellationToken cancellationToken)
    {
        var result = await cookieSessionService.GetCurrentUserAsync(cancellationToken);
        return result is null ? Result.Unauthorized() : Result.Success(result);
    }
}
