namespace BuildingBlocks.Api.Outbox;

/// <summary>
/// Who may call <c>POST /internal/outbox/dispatch</c>: Cloud Scheduler with an OIDC token of
/// <see cref="SchedulerServiceAccount"/> issued for <see cref="Audience"/> (PLT-003 R-06). Without both the endpoint
/// answers 404.
/// </summary>
public sealed class OutboxDispatchOptions
{
    public const string Section = "Outbox";

    /// <summary>The URL the scheduler calls, as set in the job's OIDC audience.</summary>
    public string? Audience { get; set; }

    /// <summary>The e-mail of the scheduler's service account.</summary>
    public string? SchedulerServiceAccount { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Audience) && !string.IsNullOrWhiteSpace(SchedulerServiceAccount);
}
