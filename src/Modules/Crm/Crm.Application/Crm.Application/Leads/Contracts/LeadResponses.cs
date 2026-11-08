namespace Crm.Application.Leads.Contracts;

public sealed record LeadPersonResponse(int UserId, string Name);

/// <summary>A row of «Усі ліди» and of the «Мій день» queue.</summary>
public sealed record LeadSummaryResponse(
    long Id,
    int SourceId,
    string Company,
    string? Category,
    string? Region,
    string? Industry,
    string? CallReason,
    string Stage,
    LeadPersonResponse? Owner,
    string? LastResultName,
    DateTimeOffset? LastCallAt,
    DateTimeOffset? NextContactAt,
    bool IsOverdue,
    int CallCount,
    DateOnly? AddedOn,
    bool InSheet,
    int Version);

/// <param name="Free">Counts for the filter chips; they ignore the chosen chip but follow search and category.</param>
public sealed record LeadPageResponse(
    IReadOnlyList<LeadSummaryResponse> Items,
    int Page,
    int PageSize,
    int Total,
    int Mine,
    int Free,
    int All);

public sealed record LeadCallResponse(
    long Id,
    LeadPersonResponse By,
    DateTimeOffset At,
    long ResultId,
    string ResultName,
    string Outcome,
    string? Comment,
    DateTimeOffset? NextContactAt);

/// <param name="OpenTaskId">The open «Подзвонити» task in «Мої завдання», if any.</param>
public sealed record LeadDetailResponse(
    long Id,
    int SourceId,
    string Company,
    string? Category,
    string? CategoryDescription,
    DateOnly? AddedOn,
    string? Region,
    string? Industry,
    string? Scale,
    string? Signal,
    DateOnly? SignalDate,
    string? FactOrPlan,
    string? SignalUrl,
    string? VolumeReason,
    string? Offer,
    string? Website,
    string? Contacts,
    string? WhomToCall,
    string? TenderCheck,
    string? CallReason,
    string? Reliability,
    string? Email,
    string? SheetNotes,
    bool InSheet,
    string Stage,
    LeadPersonResponse? Owner,
    DateTimeOffset? TakenAt,
    string? LastResultName,
    DateTimeOffset? LastCallAt,
    DateTimeOffset? NextContactAt,
    bool IsOverdue,
    int CallCount,
    long? OpenTaskId,
    IReadOnlyList<LeadCallResponse> Calls,
    int Version);

/// <summary>«Мій день» (R-09).</summary>
/// <param name="Queue">Own open leads due by the end of today or never called: overdue and today first, then new.</param>
/// <param name="Later">Own open leads planned after today, the nearest first (up to 50).</param>
public sealed record LeadMyDayResponse(
    IReadOnlyList<LeadSummaryResponse> Queue,
    IReadOnlyList<LeadSummaryResponse> Later,
    int CallsToday,
    int PlannedToday,
    int Free);

public sealed record LeadCallResultResponse(long Id, string Name, string Outcome, int? RetryDays, bool IsActive, int Position);

public sealed record LeadCategoryResponse(string Code, string? Description, int Leads);

public sealed record LeadSyncResponse(
    int Rows,
    int Added,
    int Updated,
    int Gone,
    int Skipped,
    DateTimeOffset At,
    int Duplicates,
    string? Warning);

public sealed record LeadSyncStateResponse(
    DateTimeOffset? LastRunAt,
    DateTimeOffset? LastSuccessAt,
    int Rows,
    int Added,
    int Updated,
    string? Error,
    string? Warning);

/// <summary>A manager row of «Зведення» for the period.</summary>
/// <param name="ReachedPercent">Calls whose result is not «не дозвонились»-like, of all calls; null without calls.</param>
/// <param name="Overdue">Own open leads whose next contact has passed (now, not the period).</param>
public sealed record LeadManagerStatsResponse(
    LeadPersonResponse Manager,
    int InWork,
    int Calls,
    int? ReachedPercent,
    int Offers,
    int Clients,
    int Overdue);

public sealed record LeadCountResponse(string Name, int Count);

public sealed record LeadIndustryStatsResponse(string Industry, IReadOnlyList<LeadCountResponse> Categories, int Total);

/// <summary>«Адміністрування › Ліди»: the old «Сводка» (R-10).</summary>
public sealed record LeadStatsResponse(
    DateOnly From,
    DateOnly To,
    int Total,
    int Free,
    int InWork,
    int Offer,
    int Clients,
    int Closed,
    int Overdue,
    int CallsInPeriod,
    int AddedInPeriod,
    IReadOnlyList<LeadManagerStatsResponse> Managers,
    IReadOnlyList<LeadCountResponse> Results,
    IReadOnlyList<LeadIndustryStatsResponse> Industries,
    IReadOnlyList<string> Categories,
    LeadSyncStateResponse? Sync);
