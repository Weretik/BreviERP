using System.Diagnostics;
using System.Text.Json;
using BuildingBlocks.Observability;
using Serilog.Events;
using Serilog.Parsing;

namespace UnitTests.Platform.Observability;

/// <summary>PLT-004 R-005: one Cloud Logging JSON object per line, linked to the request's trace.</summary>
public sealed class CloudLoggingJsonFormatterTests
{
    private static readonly ActivityTraceId TraceId = ActivityTraceId.CreateFromString("4bf92f3577b34da6a3ce929d0e0e4736");
    private static readonly ActivitySpanId SpanId = ActivitySpanId.CreateFromString("00f067aa0ba902b7");

    [TestCase(LogEventLevel.Verbose, "DEBUG")]
    [TestCase(LogEventLevel.Debug, "DEBUG")]
    [TestCase(LogEventLevel.Information, "INFO")]
    [TestCase(LogEventLevel.Warning, "WARNING")]
    [TestCase(LogEventLevel.Error, "ERROR")]
    [TestCase(LogEventLevel.Fatal, "CRITICAL")]
    public void Severity_follows_the_level(LogEventLevel level, string severity)
        => Assert.That(Format(Event(level)).GetProperty("severity").GetString(), Is.EqualTo(severity));

    [Test]
    public void A_line_is_one_json_object_with_the_rendered_message()
    {
        var writer = new StringWriter();
        new CloudLoggingJsonFormatter("brevistoreapi").Format(Event(LogEventLevel.Information), writer);

        var text = writer.ToString();
        using var json = JsonDocument.Parse(text);
        Assert.Multiple(() =>
        {
            Assert.That(text, Does.EndWith(Environment.NewLine));
            Assert.That(text.TrimEnd(), Does.Not.Contain("\n"));
            Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("Quote 42 sent"));
            Assert.That(json.RootElement.GetProperty("QuoteId").GetInt32(), Is.EqualTo(42));
            Assert.That(json.RootElement.GetProperty("logger").GetString(), Is.EqualTo("Crm.Quotes"));
            Assert.That(json.RootElement.TryGetProperty("time", out _), Is.True);
        });
    }

    [Test]
    public void Text_values_are_rendered_without_quotes()
    {
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Now listening on: {Address}, {Count} times"),
            [new LogEventProperty("Address", new ScalarValue("http://[::]:8080")), new LogEventProperty("Count", new ScalarValue(2))]);

        Assert.That(Format(logEvent).GetProperty("message").GetString(), Is.EqualTo("Now listening on: http://[::]:8080, 2 times"));
    }

    [Test]
    public void Trace_and_span_use_the_cloud_logging_fields()
    {
        var json = Format(Event(LogEventLevel.Information, traceId: TraceId, spanId: SpanId));

        Assert.Multiple(() =>
        {
            Assert.That(json.GetProperty("logging.googleapis.com/trace").GetString(),
                Is.EqualTo("projects/brevistoreapi/traces/4bf92f3577b34da6a3ce929d0e0e4736"));
            Assert.That(json.GetProperty("logging.googleapis.com/spanId").GetString(), Is.EqualTo("00f067aa0ba902b7"));
        });
    }

    [Test]
    public void Without_an_activity_there_are_no_trace_fields()
    {
        var json = Format(Event(LogEventLevel.Information));

        Assert.Multiple(() =>
        {
            Assert.That(json.TryGetProperty("logging.googleapis.com/trace", out _), Is.False);
            Assert.That(json.TryGetProperty("logging.googleapis.com/spanId", out _), Is.False);
        });
    }

    [Test]
    public void An_exception_goes_to_stack_trace()
    {
        var json = Format(Event(LogEventLevel.Error, exception: new InvalidOperationException("boom")));

        Assert.That(json.GetProperty("stack_trace").GetString(), Does.Contain("System.InvalidOperationException: boom"));
    }

    [Test]
    public void A_property_never_overwrites_a_reserved_field()
    {
        var json = Format(Event(LogEventLevel.Warning, extra: new LogEventProperty("severity", new ScalarValue("INFO"))));

        Assert.That(json.GetProperty("severity").GetString(), Is.EqualTo("WARNING"));
    }

    private static JsonElement Format(LogEvent logEvent)
    {
        var writer = new StringWriter();
        new CloudLoggingJsonFormatter("brevistoreapi").Format(logEvent, writer);
        return JsonDocument.Parse(writer.ToString()).RootElement.Clone();
    }

    private static LogEvent Event(
        LogEventLevel level,
        Exception? exception = null,
        ActivityTraceId? traceId = null,
        ActivitySpanId? spanId = null,
        LogEventProperty? extra = null)
    {
        var properties = new List<LogEventProperty>
        {
            new("QuoteId", new ScalarValue(42)),
            new("SourceContext", new ScalarValue("Crm.Quotes")),
        };
        if (extra is not null)
        {
            properties.Add(extra);
        }

        return new LogEvent(
            DateTimeOffset.Parse("2026-11-16T21:00:00Z"),
            level,
            exception,
            new MessageTemplateParser().Parse("Quote {QuoteId} sent"),
            properties,
            traceId ?? default,
            spanId ?? default);
    }
}
