using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.GetCalculations;
using Crm.Application.Embroidery.SaveCalculation;
using Crm.Domain.Embroidery.Saved;

namespace UnitTests.Crm.Embroidery;

[TestFixture]
public sealed class EmbroiderySavedTests
{
    private static CalculateEmbroideryInput Form() =>
        new("Garment", 50, 10_000, "18", 1, 1, 1, "New", 60, null, "UsedBlanks", null);

    private static bool Valid(SaveEmbroideryCalculationInput input) => new SaveEmbroideryCalculationCommandValidator()
        .Validate(new SaveEmbroideryCalculationCommand(new EmbroideryActor(17, false), input)).IsValid;

    [Test]
    public void NameIsTrimmedAndCutTo120()
    {
        Assert.That(EmbroiderySavedCalculation.NormalizeName("  Поло  "), Is.EqualTo("Поло"));
        Assert.That(EmbroiderySavedCalculation.NormalizeName(null), Is.Empty);
        Assert.That(EmbroiderySavedCalculation.NormalizeName(new string('а', 200)), Has.Length.EqualTo(120));
    }

    [Test]
    public void NumberHasThePrefixAndFourDigits()
    {
        Assert.That(EmbroiderySavedCalculation.NumberOf(42), Is.EqualTo("ВШ-0042"));
        Assert.That(EmbroiderySavedCalculation.NumberOf(12345), Is.EqualTo("ВШ-12345"));
    }

    [Test]
    public void SaveChecksTheFormAndTheCustomerId()
    {
        Assert.That(Valid(new SaveEmbroideryCalculationInput(null, null, Form())), Is.True);
        Assert.That(Valid(new SaveEmbroideryCalculationInput(null, Guid.Empty, Form())), Is.False);
        Assert.That(Valid(new SaveEmbroideryCalculationInput(null, null, Form() with { Quantity = 0 })), Is.False);
        Assert.That(Valid(new SaveEmbroideryCalculationInput(new string('а', 1001), null, Form())), Is.False);
    }

    [Test]
    public void ListPageIsLimited()
    {
        var validator = new GetEmbroideryCalculationsQueryValidator();
        var actor = new EmbroideryActor(17, false);
        Assert.That(validator.Validate(new GetEmbroideryCalculationsQuery(actor, null)).IsValid, Is.True);
        Assert.That(validator.Validate(new GetEmbroideryCalculationsQuery(actor, null, 1, 101)).IsValid, Is.False);
        Assert.That(validator.Validate(new GetEmbroideryCalculationsQuery(actor, null, 0)).IsValid, Is.False);
    }
}
