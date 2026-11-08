using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Host.Api.Security.Access;

/// <summary>A page of «Права доступу» and, when the policy names it, the action; otherwise the HTTP method decides.</summary>
public sealed record AccessRequirement(AccessArea Area, AccessAction? Action) : IAuthorizationRequirement;
