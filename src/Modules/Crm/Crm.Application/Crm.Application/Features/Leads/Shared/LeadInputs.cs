namespace Crm.Application.Features.Leads.Shared;

/// <param name="NextContactAt">Required for callback, talking and offer results; ignored for results that close.</param>
/// <param name="RequestId">New for every form; a repeat with the same id returns the lead without a second call.</param>
public sealed record RecordLeadCallInput(long ResultId, string? Comment, DateTimeOffset? NextContactAt, int Version,
    Guid? RequestId = null);

/// <param name="Outcome">`Retry`, `Callback`, `Talking`, `Offer`, `Client` or `Closed`.</param>
/// <param name="RetryDays">Only for `Retry`: days until the next try when no date is given (1–60, default 1).</param>
public sealed record SaveLeadCallResultInput(string Name, string Outcome, int? RetryDays, bool IsActive, int Position);

public sealed record SaveLeadCategoryInput(string? Description);
