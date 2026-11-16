namespace ActivityLog.Contracts.Activity;

/// <summary>The outbox message that carries an <see cref="ActivityRecorded"/> (ADR-0016).</summary>
public static class ActivityMessage
{
    public const string Type = "activity.recorded";
}
