using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;

namespace Crm.Application.Tasks.GetTask;

/// <summary>The side panel of one task: description, steps, history (R-11).</summary>
public sealed record GetTaskQuery(TaskActor Actor, long Id) : IQuery<Result<TaskDetailResponse>>;
