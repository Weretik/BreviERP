using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.Persistence;
using Crm.Application.Tasks.Staff;
using Crm.Domain.Tasks;

namespace Crm.Application.Tasks.Building;

/// <summary>Turns the side panel into a domain draft: the assignee must be an active Manager, the client must exist.</summary>
internal static class TaskDraftResolver
{
    public static async Task<Result<ManagerTaskDraft>> ResolveAsync(this ITaskStaffReader staff, ITaskStore store,
        SaveTaskInput input, CancellationToken cancellationToken)
    {
        var assignee = await staff.FindAssigneeAsync(input.AssigneeUserId, cancellationToken);
        if (assignee is null) return Result.Invalid(new ValidationError(TaskMessages.AssigneeNotFound));

        string? customerName = null;
        if (input.CustomerPublicId is { } customerId)
        {
            customerName = await store.FindCustomerNameAsync(customerId, cancellationToken);
            if (customerName is null) return Result.Invalid(new ValidationError(TaskMessages.CustomerNotFound));
        }

        return new ManagerTaskDraft(input.Title, input.Description, new ManagerTaskPerson(assignee.UserId, assignee.Name),
            input.DueAt, input.IsUrgent, input.CustomerPublicId, customerName,
            (input.Steps ?? []).Select(x => new ManagerTaskStepDraft(x.Text, x.IsDone)).ToArray());
    }

    /// <summary>The actor as history shows them.</summary>
    public static async Task<ManagerTaskPerson?> PersonAsync(this ITaskStaffReader staff, TaskActor actor,
        CancellationToken cancellationToken) =>
        await staff.FindAsync(actor.UserId, cancellationToken) is { } member
            ? new ManagerTaskPerson(member.UserId, member.Name)
            : null;
}
