using Microsoft.AspNetCore.Authorization;

namespace Identity.Api.Access;

/// <summary>
/// A page of «Права доступу» by its key and, when the policy names it, the action (<c>view</c>, <c>edit</c>...);
/// otherwise the HTTP method decides.
/// </summary>
public sealed record AccessRequirement(string AreaKey, string? Action) : IAuthorizationRequirement;
