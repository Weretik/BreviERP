using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Application.Features.Tasks.Commands.SetTaskUrgent;

/// <summary>Director or Admin drags a card into or out of «Терміново» (R-08).</summary>
public sealed record SetTaskUrgentCommand(TaskActor Actor, long Id, int Version, bool IsUrgent)
    : ICommand<Result<TaskDetailResponse>>;
