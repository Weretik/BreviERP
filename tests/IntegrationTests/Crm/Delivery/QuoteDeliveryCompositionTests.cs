using Crm.Application.Delivery.EnqueueQuote;
using Crm.Application.Delivery.GetDeliveries;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.Processing;
using Crm.Application.Delivery.Provider;
using Crm.Application.Delivery.Readiness;
using Crm.Infrastructure.DependencyInjection;
using Crm.Infrastructure.Persistence;
using Host.Api.Crm.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntegrationTests.Crm.Delivery;

public sealed class QuoteDeliveryCompositionTests
{
    [Test]
    public async Task Empty_configuration_resolves_delivery_graph_but_blocks_token_and_dispatch_readiness()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(
            "Host=localhost;Database=unused_composition_fixture;Username=unused"));
        services.AddCrmQuoteDelivery(new ConfigurationBuilder().Build());
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
            { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = provider.CreateAsyncScope();

        Assert.Multiple(() =>
        {
            Assert.That(scope.ServiceProvider.GetRequiredService<EnqueueQuoteDeliveryHandler>(), Is.Not.Null);
            Assert.That(scope.ServiceProvider.GetRequiredService<QuoteDeliveryReader>(), Is.Not.Null);
            Assert.That(scope.ServiceProvider.GetRequiredService<QuoteDeliveryProcessor>(), Is.Not.Null);
            Assert.That(provider.GetRequiredService<IQuoteDeliveryReadiness>().Assess("sandbox@example.test").CanSubmit,
                Is.False);
        });
        Assert.That(await provider.GetRequiredService<IQuoteDeliveryAccessTokenSource>().GetAccessTokenAsync(default),
            Is.Null);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Hosted_worker_processes_one_claim_and_uses_only_the_authorized_provider_operation(bool reconcile)
    {
        var identity = new Mock<IQuoteDeliveryIdentity>();
        identity.SetupGet(x => x.ProviderKey).Returns("gmail");
        var readiness = new Mock<IQuoteDeliveryReadiness>();
        readiness.Setup(x => x.Assess(It.IsAny<string>())).Returns(new QuoteDeliveryReadiness(true, []));
        var deliveryId = Guid.NewGuid();
        var leaseToken = Guid.NewGuid();
        var submission = new QuoteDeliverySubmission(deliveryId, DateTimeOffset.UtcNow,
            "sandbox@example.test", "Synthetic quote", "Public fixture", "%PDF-fixture"u8.ToArray());
        var claim = new QuoteDeliveryClaim(deliveryId, leaseToken, reconcile, submission);
        var outcome = new QuoteDeliveryProviderResult(QuoteDeliveryProviderOutcome.ConfirmedSuccess, "fixture123", null);
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var store = new Mock<IQuoteDeliveryStore>(MockBehavior.Strict);
        store.SetupSequence(x => x.ClaimAsync(identity.Object, It.IsAny<Func<string, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(claim).ReturnsAsync((QuoteDeliveryClaim?)null);
        store.Setup(x => x.CompleteAsync(deliveryId, leaseToken, outcome, It.IsAny<CancellationToken>()))
            .Callback(() => completed.TrySetResult()).ReturnsAsync(true);
        var transport = new Mock<IQuoteDeliveryProvider>(MockBehavior.Strict);
        transport.SetupGet(x => x.Capabilities).Returns(new QuoteDeliveryProviderCapabilities("gmail", true, true));
        if (reconcile)
            transport.Setup(x => x.ReconcileAsync(deliveryId, It.IsAny<CancellationToken>())).ReturnsAsync(outcome);
        else
            transport.Setup(x => x.SubmitAsync(submission, It.IsAny<CancellationToken>())).ReturnsAsync(outcome);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(identity.Object);
        services.AddSingleton(readiness.Object);
        services.AddSingleton(store.Object);
        services.AddSingleton(transport.Object);
        services.AddScoped<QuoteDeliveryProcessor>();
        services.AddSingleton<QuoteDeliveryWorker>();
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
            { ValidateOnBuild = true, ValidateScopes = true });
        var worker = provider.GetRequiredService<QuoteDeliveryWorker>();
        await worker.StartAsync(default);
        try { await completed.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
        finally
        {
            using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await worker.StopAsync(stop.Token);
        }

        store.Verify(x => x.CompleteAsync(deliveryId, leaseToken, outcome, It.IsAny<CancellationToken>()), Times.Once);
        transport.Verify(x => x.SubmitAsync(It.IsAny<QuoteDeliverySubmission>(), It.IsAny<CancellationToken>()),
            reconcile ? Times.Never() : Times.Once());
        transport.Verify(x => x.ReconcileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            reconcile ? Times.Once() : Times.Never());
    }
}
