using SewingPricing.Jobs.Activation;
using Catalog.Application.Features.Product.Shared.Sewing;
using Catalog.Application.Features.Product.Shared.Sewing.Inputs;

namespace UnitTests.Jobs.SewingPricing;

public sealed class PricingActivationTests
{
    [Test]
    public void Confirmation_rejects_changed_quantities_or_manual_minutes()
    {
        var material = new SewingMaterialInput("Fabric", 1, null, 1.25m, "Metre", 0);
        Assert.That(ActivationCompositionInputs.MaterialsEqual([material], [material]), Is.True);
        Assert.That(ActivationCompositionInputs.MaterialsEqual([material], [material with { Quantity = 2m }]), Is.False);
        var operation = new SewingOperationInput(400, 1, 4, 0);
        Assert.That(ActivationCompositionInputs.OperationsEqual([operation], [operation]), Is.True);
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
