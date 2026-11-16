using System.Diagnostics;
using BuildingBlocks.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace UnitTests.Platform.Observability;

/// <summary>PLT-004 R-001, R-003, R-004: tracing only with a project id, sampled by trace id, no health checks.</summary>
public sealed class CloudTracingTests
{
    [Test]
    public void Without_a_project_id_nothing_is_exported()
    {
        var services = new ServiceCollection();

        CloudTracing.Add(services, new ObservabilityOptions { ServiceName = "brevierp-api" });

        using var provider = services.BuildServiceProvider();
        Assert.That(provider.GetService<TracerProvider>(), Is.Null);
    }

    [Test]
    public void With_a_project_id_the_tracer_is_registered_and_passes_startup_validation()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        CloudTracing.Add(services, new ObservabilityOptions { ServiceName = "brevierp-api", GoogleCloudProjectId = "brevistoreapi" });

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        Assert.That(provider.GetService<TracerProvider>(), Is.Not.Null);
    }

    [Test]
    public void The_sampler_ignores_the_not_sampled_flag_of_cloud_run()
    {
        var parent = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.None, isRemote: true);
        var parameters = new SamplingParameters(parent, parent.TraceId, "GET /api/crm/quotes", ActivityKind.Server);

        Assert.That(CloudTracing.CreateSampler(1).ShouldSample(parameters).Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void A_zero_ratio_drops_everything()
    {
        var parameters = new SamplingParameters(default, ActivityTraceId.CreateRandom(), "GET /api/crm/quotes", ActivityKind.Server);

        Assert.That(CloudTracing.CreateSampler(0).ShouldSample(parameters).Decision, Is.EqualTo(SamplingDecision.Drop));
    }

    [TestCase("/health", false)]
    [TestCase("/api/crm/quotes", true)]
    public void The_health_check_is_not_traced(string path, bool traced)
    {
        var context = new DefaultHttpContext { Request = { Path = path } };

        Assert.That(CloudTracing.ShouldTrace(context), Is.EqualTo(traced));
    }

    [TestCase(-0.1)]
    [TestCase(1.1)]
    public void A_sample_ratio_outside_zero_to_one_is_rejected(double ratio)
        => Assert.Throws<ArgumentOutOfRangeException>(() => CloudTracing.CreateSampler(ratio));
}
