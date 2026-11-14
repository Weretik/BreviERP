using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Queries.GetTaskAssignees;

public sealed class GetTaskAssigneesHandler(ITaskStaffReader staff)
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
