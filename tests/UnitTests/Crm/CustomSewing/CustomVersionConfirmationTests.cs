using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.Persistence;
using Crm.Application.CustomSewing.Pricing;
using Crm.Application.CustomSewing.SaveVersion;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Pricing;
using Crm.Domain.CustomSewing.Versions;
using Moq;

namespace UnitTests.Crm.CustomSewing;

public sealed class CustomVersionConfirmationTests
{
    private sealed class ConfirmationClock(DateTimeOffset initial) : TimeProvider
    {
        public DateTimeOffset Current { get; set; } = initial;
        public override DateTimeOffset GetUtcNow() => Current;
    }

    [TestCase(59, true)]
    [TestCase(60, false)]
    [TestCase(61, false)]
    public async Task Confirmation_rechecks_expiry_after_source_validation_before_any_version_write(
        int elapsedSeconds, bool accepted)
    {
        var at = new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
        var clock = new ConfirmationClock(at);
        var config = new CustomConfiguration("Synthetic", "Jacket", "winter", "", "", "",
            [new(1, 1, "Meters")], [], [new(2, 1, null)], [], 0, 0);
        var draft = CustomSewingDraft.Create(new CrmUserId(17), null, null, config);
        // Simulate the IDs assigned by a persisted write session, without a database or wall-clock delay.
        typeof(CustomSewingDraft).GetProperty(nameof(CustomSewingDraft.Id))!.SetValue(draft, new CustomDraftId(1));
        var basis = new CustomPriceBasis(Guid.NewGuid(), at, at.AddMinutes(1), "custom-sewing-sheet-v1", 10,
            [new("Fabric:1", 1, 100, "Meters", "synthetic"), new("Operation:2", 2, 10, "Minutes", "synthetic")],
            new(100, 0, 10, 1, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0), 110, 200, 180, 160, []);
        var actor = new CrmActor(17, false);
        var session = new Mock<ICustomWriteSession>();
        session.Setup(x => x.FindDraftAsync(draft.PublicId, actor, It.IsAny<CancellationToken>())).ReturnsAsync(draft);
        session.Setup(x => x.FindBasisAsync(basis.Token, draft.Id, 1, 17, It.IsAny<CancellationToken>())).ReturnsAsync(basis);
        session.Setup(x => x.Add(It.IsAny<CustomSewingVersion>())).Callback<CustomSewingVersion>(version =>
            typeof(CustomSewingVersion).GetProperty(nameof(CustomSewingVersion.Id))!.SetValue(version, new CustomVersionId(1)));
        session.Setup(x => x.FlushAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        session.Setup(x => x.DetailAsync(draft, It.IsAny<CancellationToken>())).ReturnsAsync(
            new CustomDetail(draft.PublicId, 2, 17, null, null, config, null, null, at, true));
        var store = new Mock<ICustomMutationStore>();
        store.Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<Guid>(), actor, It.IsAny<object>(),
                It.IsAny<Func<ICustomWriteSession, CancellationToken, Task<Result<CustomDetail>>>>(), It.IsAny<CancellationToken>()))
            .Returns((string operation, Guid requestId, CrmActor who, object intent,
                Func<ICustomWriteSession, CancellationToken, Task<Result<CustomDetail>>> work, CancellationToken ct) => work(session.Object, ct));
        var pricing = new Mock<ICustomSewingPricing>();
        pricing.Setup(x => x.CalculateAsync(It.IsAny<CustomConfiguration>(), at, It.IsAny<CancellationToken>()))
            .Returns(() => { clock.Current = at.AddSeconds(elapsedSeconds); return Task.FromResult(basis); });

        var result = await new SaveCustomVersionCommandHandler(store.Object, pricing.Object, clock)
            .Handle(new(draft.PublicId, 1, Guid.NewGuid(), basis.Token, actor), default);

        Assert.That(result.IsSuccess, Is.EqualTo(accepted));
        if (!accepted)
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
            session.Verify(x => x.Add(It.IsAny<CustomSewingVersion>()), Times.Never);
            session.Verify(x => x.FlushAsync(It.IsAny<CancellationToken>()), Times.Never);
            Assert.That(draft.Revision, Is.EqualTo(1));
        }
    }
}
