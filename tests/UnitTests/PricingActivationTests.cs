using Catalog.Application.Features.Product.Sewing;
using SewingPricing.Jobs.Activation;

namespace UnitTests;

public sealed class PricingActivationTests
{
    [Test]
    public void Confirmation_may_change_review_flag_but_never_quantities_or_manual_minutes()
    {
        var material = new SewingMaterialInput("Fabric", 1, null, 1.25m, "Metre", true, 0, false);
        Assert.That(ActivationCompositionInputs.MaterialsEqual([material], [material with { Reviewed = true }]), Is.True);
        Assert.That(ActivationCompositionInputs.MaterialsEqual([material], [material with { Quantity = 2m }]), Is.False);
        var operation = new SewingOperationInput(400, 1, 4, 0, false);
        Assert.That(ActivationCompositionInputs.OperationsEqual([operation], [operation with { Reviewed = true }]), Is.True);
        Assert.That(ActivationCompositionInputs.OperationsEqual([operation], [operation with { ManualMinutes = 0 }]), Is.False);
    }

    [Test]
    public void Activation_requires_local_and_exact_planned_database()
    {
        Assert.Throws<InvalidOperationException>(() => ActivationTargetGuard.Validate("Host=example.invalid;Database=brevi"));
        Assert.Throws<InvalidOperationException>(() => ActivationTargetGuard.Validate("Host=localhost;Database=other", "brevi"));
        Assert.That(ActivationTargetGuard.Validate("Host=127.0.0.1;Database=brevi", "brevi"), Is.EqualTo("brevi"));
    }
}
