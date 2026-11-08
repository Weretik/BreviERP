using Identity.Application.Features.Access.Shared;

namespace Identity.Application.Features.Access.CreateRole;

public sealed record CreateRoleCommand(string Title) : ICommand<Result<AccessRoleDto>>;
