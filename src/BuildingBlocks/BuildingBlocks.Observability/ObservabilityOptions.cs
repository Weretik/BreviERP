namespace BuildingBlocks.Observability;

/// <summary>
/// Section <c>Observability</c> (PLT-004). Without <see cref="GoogleCloudProjectId"/> the host writes
/// plain-text logs and exports no traces (development, tests, the build-time OpenAPI run).
/// </summary>
public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    /// <summary>Service name in Cloud Trace, for example <c>brevierp-api</c>.</summary>
    public string ServiceName { get; init; } = "api";

    /// <summary>Google Cloud project that receives the traces and owns the log trace links.</summary>
    public string? GoogleCloudProjectId { get; init; }

    /// <summary>Share of traces kept, 0…1, decided by trace id.</summary>
    public double TraceSampleRatio { get; init; } = 1;

    public bool ExportsToGoogleCloud => !string.IsNullOrWhiteSpace(GoogleCloudProjectId);
}
