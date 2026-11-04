using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.Staff;

namespace Crm.Application.Tasks.GetTaskAssignees;

public sealed class GetTaskAssigneesQueryHandler(ITaskStaffReader staff)
    : IQueryHandler<GetTaskAssigneesQuery, Result<IReadOnlyList<TaskAssigneeResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<TaskAssigneeResponse>>> Handle(GetTaskAssigneesQuery query,
        CancellationToken cancellationToken)
    {
        var members = await staff.ListAssigneesAsync(cancellationToken);
        return Result.Success<IReadOnlyList<TaskAssigneeResponse>>(
            members.Select(x => new TaskAssigneeResponse(x.UserId, x.Name, x.Email)).ToArray());
    }
}
