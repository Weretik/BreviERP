using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Pricing;
using Crm.Application.Files.Verification;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Pricing;
using Moq;

namespace UnitTests.Crm.CustomSewing;

public sealed class CustomArtworkPricingTests
{
    private static readonly CrmActor Actor = new(17, false);
    private static readonly DateTimeOffset At = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
    private static CustomSewingDraft Draft(Guid? artwork) => CustomSewingDraft.Create(new CrmUserId(17), null, null,
        new CustomConfiguration("Synthetic", "Куртка", "winter", "", "", "", [new(1, 1, "Metre")], [],
            [new(2, 1, null)], artwork is null ? [] : [new("print", "Front", artwork, 10, 20, true)], 0, 0));
    private static CustomPriceBasis Basis => new(Guid.NewGuid(), At, At.AddMinutes(15), "synthetic", 1, [],
        new(1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0), 1, 3, 2, 1, []);

    [Test]
    public async Task No_artwork_keeps_existing_pricing_path_without_storage_verification()
    {
        var draft = Draft(null);
        var pricing = new Mock<ICustomSewingPricing>(MockBehavior.Strict);
        var verifier = new Mock<ICustomArtworkVerifier>(MockBehavior.Strict);
        var basis = Basis;
        pricing.Setup(x => x.CalculateAsync(draft.Configuration, At, It.IsAny<CancellationToken>())).ReturnsAsync(basis);
        var result = await new CustomSewingPriceCalculator(pricing.Object, verifier.Object).CalculateAsync(draft, Actor, At, default);
        Assert.That(result.Value, Is.SameAs(basis));
        verifier.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Artwork_without_a_verifier_fails_closed_before_pricing()
    {
        var pricing = new Mock<ICustomSewingPricing>(MockBehavior.Strict);
        var result = await new CustomSewingPriceCalculator(pricing.Object, null)
            .CalculateAsync(Draft(Guid.NewGuid()), Actor, At, default);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        pricing.VerifyNoOtherCalls();
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Proof_for_another_root_or_file_set_never_unlocks_verified_pricing(bool wrongRoot)
    {
        var file = Guid.NewGuid();
        var draft = Draft(file);
        var pricing = new Mock<ICustomSewingPricing>(MockBehavior.Strict);
        var verifier = new Mock<ICustomArtworkVerifier>();
        verifier.Setup(x => x.VerifyAsync(draft.PublicId, It.IsAny<IReadOnlyCollection<Guid>>(), Actor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new VerifiedCustomArtwork(wrongRoot ? Guid.NewGuid() : draft.PublicId,
                wrongRoot ? [file] : [Guid.NewGuid()])));
        var result = await new CustomSewingPriceCalculator(pricing.Object, verifier.Object).CalculateAsync(draft, Actor, At, default);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        pricing.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Inaccessible_artwork_preserves_the_not_found_result_without_pricing()
    {
        var draft = Draft(Guid.NewGuid());
        var pricing = new Mock<ICustomSewingPricing>(MockBehavior.Strict);
        var verifier = new Mock<ICustomArtworkVerifier>();
        verifier.Setup(x => x.VerifyAsync(draft.PublicId, It.IsAny<IReadOnlyCollection<Guid>>(), Actor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());
        var result = await new CustomSewingPriceCalculator(pricing.Object, verifier.Object).CalculateAsync(draft, Actor, At, default);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
        pricing.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Exact_scoped_proof_uses_verified_pricing_and_preserves_original_artwork_configuration()
    {
        var file = Guid.NewGuid();
        var draft = Draft(file);
        var pricing = new Mock<ICustomSewingPricing>(MockBehavior.Strict);
        var verifier = new Mock<ICustomArtworkVerifier>();
        verifier.Setup(x => x.VerifyAsync(draft.PublicId, It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(new[] { file })),
                Actor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new VerifiedCustomArtwork(draft.PublicId, [file])));
        pricing.Setup(x => x.CalculateAsync(draft.Configuration, At, It.IsAny<CancellationToken>(), true)).ReturnsAsync(Basis);
        var result = await new CustomSewingPriceCalculator(pricing.Object, verifier.Object).CalculateAsync(draft, Actor, At, default);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(draft.Configuration.Applications.Single().ArtworkFileId, Is.EqualTo(file));
        pricing.Verify(x => x.CalculateAsync(draft.Configuration, At, It.IsAny<CancellationToken>(), true), Times.Once);
    }
}
