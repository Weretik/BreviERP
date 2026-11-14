namespace Identity.Application.Features.Access.Commands.DeleteRole;

public sealed record DeleteRoleCommand(string Role) : ICommand<Result>;
