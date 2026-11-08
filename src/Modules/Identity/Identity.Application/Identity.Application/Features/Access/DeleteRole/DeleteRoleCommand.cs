namespace Identity.Application.Features.Access.DeleteRole;

public sealed record DeleteRoleCommand(string Role) : ICommand<Result>;
