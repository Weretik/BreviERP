using Crm.Domain.Tasks;

namespace Crm.Application.Tasks.Access;

/// <summary>
/// The signed-in staff member. A Manager sees and moves only tasks given to them; Director and Admin
/// (the director also holds Admin) see, create and change all of them (TSK-001 R-01, R-02).
/// </summary>
public sealed record TaskActor(int UserId, bool IsAdmin)
{
    public bool CanAccess(ManagerTask task) => IsAdmin || task.AssigneeUserId == UserId;
}
