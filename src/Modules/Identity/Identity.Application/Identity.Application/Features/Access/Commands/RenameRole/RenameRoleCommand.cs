using Identity.Application.Features.Access.Shared;

namespace Identity.Application.Features.Access.Commands.RenameRole;

/// <param name="Role">The role's key, a generated <c>custom-…</c> name.</param>
public sealed record RenameRoleCommand(string Role, string Title) : ICommand<Result<AccessRoleDto>>;
