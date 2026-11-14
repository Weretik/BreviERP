using Identity.Application.Features.Access.Shared;
using Identity.Domain.Authorization;

namespace Identity.Application.Features.Access.Commands.SaveRoleAccess;

/// <param name="Grants">Page key → allowed actions; a page left out is closed to the role.</param>
public sealed record SaveRoleAccessCommand(
    string Role,
    int Version,
    IReadOnlyDictionary<string, IReadOnlyList<AccessAction>> Grants) : ICommand<Result<AccessRoleDto>>;
