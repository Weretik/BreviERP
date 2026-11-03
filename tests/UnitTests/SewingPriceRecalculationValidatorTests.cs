using Catalog.Application.Features.Pricing.Sewing.Recalculation;
using Catalog.Application.Features.Pricing.Sewing.Recalculation.Validators;

namespace UnitTests;

/// <summary>CAT-011: «Перерахувати всі» takes every model, or 1–500 chosen ones, and needs an actor.</summary>
public sealed class SewingPriceRecalculationValidatorTests
{
    private readonly RecalculateSewingPricesCommandValidator _validator = new();

    [Test]
    public void Every_model_or_a_bounded_list_of_positive_ids_is_valid()
    {
        Assert.That(_validator.Validate(new RecalculateSewingPricesCommand(null, true, "user-1")).IsValid, Is.True);
        Assert.That(_validator.Validate(new RecalculateSewingPricesCommand([1, 2], false, "user-1")).IsValid, Is.True);
    }

    [Test]
    public void An_empty_list_a_bad_id_too_many_ids_or_no_actor_is_rejected()
    {
        Assert.That(_validator.Validate(new RecalculateSewingPricesCommand([], true, "user-1")).IsValid, Is.False);
        Assert.That(_validator.Validate(new RecalculateSewingPricesCommand([0], true, "user-1")).IsValid, Is.False);
        Assert.That(_validator.Validate(new RecalculateSewingPricesCommand(
            Enumerable.Range(1, 501).ToList(), true, "user-1")).IsValid, Is.False);
        Assert.That(_validator.Validate(new RecalculateSewingPricesCommand(null, true, "")).IsValid, Is.False);
    }
}
