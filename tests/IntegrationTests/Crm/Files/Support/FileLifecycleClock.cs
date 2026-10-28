namespace IntegrationTests.Crm.Files.Support;

internal sealed class FileLifecycleClock : TimeProvider
{
    internal DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
    public override DateTimeOffset GetUtcNow() => Now;
}
