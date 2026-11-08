namespace Crm.Application.Tasks.Contracts;

public sealed record TaskPersonResponse(int UserId, string Name);

/// <summary>A table row or a kanban card. <paramref name="Lane"/> is `Overdue`, `Urgent`, `Active` or `Done`.</summary>
public sealed record TaskSummaryResponse(
    long Id,
    string Number,
    string Title,
    string Status,
    string Lane,
    bool IsUrgent,
    bool IsOverdue,
    DateTimeOffset DueAt,
    TaskPersonResponse Assignee,
    string? CustomerName,
    int StepsTotal,
    int StepsDone,
    DateTimeOffset? CompletedAt,
    DateTimeOffset UpdatedAt,
    int Version,
    DateTimeOffset CreatedAt);

public sealed record TaskPageResponse(IReadOnlyList<TaskSummaryResponse> Items, int Page, int PageSize, int Total);

/// <summary>Open tasks and those done in the chosen period, newest added first, for the «Мої завдання» board.</summary>
public sealed record TaskBoardResponse(IReadOnlyList<TaskSummaryResponse> Items, DateTimeOffset At);

public sealed record TaskStepResponse(long Id, int Position, string Text, bool IsDone);

/// <summary><paramref name="Kind"/> is a <c>ManagerTaskEventKind</c>; the Admin writes the text.</summary>
public sealed record TaskEventResponse(string Kind, TaskPersonResponse By, DateTimeOffset At, string? Details);

public sealed record TaskDetailResponse(
    long Id,
    string Number,
    string Title,
    string? Description,
    string Status,
    string Lane,
    bool IsUrgent,
    bool IsOverdue,
    DateTimeOffset DueAt,
    TaskPersonResponse Assignee,
    TaskPersonResponse Author,
    Guid? CustomerPublicId,
    string? CustomerName,
    IReadOnlyList<TaskStepResponse> Steps,
    IReadOnlyList<TaskEventResponse> History,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version);

/// <summary>Counts for the metric strip and the filter chips (R-09).</summary>
/// <param name="OnTimePercent">Share of tasks done in the last 30 days that were done by their due time; null when none.</param>
public sealed record TaskOverviewResponse(
    int Open,
    int Overdue,
    int Urgent,
    int New,
    int InProgress,
    int Done,
    int DoneLastWeek,
    int? OnTimePercent,
    IReadOnlyList<TaskLoadResponse> Load);

/// <summary>
/// Workload of one manager («Навантаження» tab); Director and Admin get every manager, a Manager only self.
/// OnTimePercent is the share of tasks done by their due time over the last 30 days, null without done tasks.
/// </summary>
public sealed record TaskLoadResponse(int UserId, string Name, int Open, int Overdue, int Urgent, int InProgress,
    int DoneLastWeek, int? OnTimePercent);

public sealed record TaskAssigneeResponse(int UserId, string Name, string? Email);
