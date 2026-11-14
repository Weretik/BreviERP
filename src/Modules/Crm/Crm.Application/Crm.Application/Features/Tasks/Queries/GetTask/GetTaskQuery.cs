using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Application.Features.Tasks.Queries.GetTask;

/// <summary>The side panel of one task: description, steps, history (R-11).</summary>
public sealed record GetTaskQuery(TaskActor Actor, long Id) : IQuery<Result<TaskDetailResponse>>;
