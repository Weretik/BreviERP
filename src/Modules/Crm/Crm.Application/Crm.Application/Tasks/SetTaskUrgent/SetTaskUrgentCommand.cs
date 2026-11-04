using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;

namespace Crm.Application.Tasks.SetTaskUrgent;

/// <summary>Director or Admin drags a card into or out of «Терміново» (R-08).</summary>
public sealed record SetTaskUrgentCommand(TaskActor Actor, long Id, int Version, bool IsUrgent)
    : ICommand<Result<TaskDetailResponse>>;
