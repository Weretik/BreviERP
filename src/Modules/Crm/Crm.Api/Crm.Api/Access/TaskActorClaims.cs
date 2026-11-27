using System.Security.Claims;
using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Api.Access;

internal static class TaskActorClaims
{
    /// <summary>
    /// «Manages tasks» (sees every manager's tasks, TSK-001 R-01) is «Завдання менеджерам: Перегляд» on «Права доступу»
    /// (ACC-002 R-01); the director always has it.
    /// </summary>
    public static async Task<TaskActor?> ReadAsync(ClaimsPrincipal user, IAuthorizationService authorization) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
            ? new TaskActor(id, (await authorization.AuthorizeAsync(user, CrmAccess.ManagerTasksView)).Succeeded)
            : null;
}
