using System.Diagnostics.Tracing;
using BuildingBlocks.Observability;
using Microsoft.Extensions.Logging;

namespace UnitTests.Platform.Observability;

/// <summary>PLT-004: a failed trace export is visible in the application log.</summary>
public sealed class OpenTelemetryErrorLogTests
{
    [Test]
    public void An_sdk_error_is_written_to_the_application_log()
    {
        var logger = new RecordingLogger();
        using var errorLog = new OpenTelemetryErrorLog(logger);
        using var source = new FakeExporterEventSource();

        source.ExportFailed("https://telemetry.googleapis.com/v1/traces", "403 Forbidden");

        Assert.That(logger.Entries, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(logger.Entries[0].Level, Is.EqualTo(LogLevel.Error));
            Assert.That(logger.Entries[0].Message, Does.Contain("403 Forbidden").And.Contain("telemetry.googleapis.com"));
        });
    }

    [Test]
    public void Sdk_information_is_not_written()
    {
        var logger = new RecordingLogger();
        using var errorLog = new OpenTelemetryErrorLog(logger);
        using var source = new FakeExporterEventSource();

        source.Exported(12);

        Assert.That(logger.Entries, Is.Empty);
    }

    [EventSource(Name = "OpenTelemetry-Exporter-Fake")]
    private sealed class FakeExporterEventSource : EventSource
    {
        [Event(1, Level = EventLevel.Error, Message = "Export to '{0}' failed: {1}")]
        public void ExportFailed(string endpoint, string error) => WriteEvent(1, endpoint, error);

        [Event(2, Level = EventLevel.Informational, Message = "Exported {0} spans")]
        public void Exported(int count) => WriteEvent(2, count);
    }

    private sealed class RecordingLogger : ILogger<OpenTelemetryErrorLog>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }
}
