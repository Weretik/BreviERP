using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Application.Features.Tasks.Commands.DeleteTask;

/// <summary>Director or Admin deletes a task with its steps and history (R-02).</summary>
public sealed record DeleteTaskCommand(TaskActor Actor, long Id, int Version) : ICommand<Result>;
