using Production.Application.Abstractions;
using Production.Application.FabricNorms.GetCompositions;
using Production.Domain.FabricNorms;

namespace UnitTests.Production;

public sealed class ModelCompositionHandlerTests
{
    [Test]
    public async Task Every_catalog_model_is_listed_with_its_layers_and_full_composition()
    {
        using var workshop = new ProductionTestHarness();
        workshop.Models.Models.AddRange([new("MOD-052", "Куртка «Віол»", true, 52, "Зима"), new("MOD-025", "Костюм", true, 25, "Літо")]);
        workshop.Models.Compositions.Add(new(52, "MOD-052",
        [
            new("Fabric", "386", "FAB-023", "Тканина «Грета»", 2.5m, "Metre", null),
            new("Accessory", null, "FUR-001", "Нитки поліестерові", 0.1m, "Piece", null),
            new("Accessory", null, "FUR-021", "Підкладка поліестерова", 1.9m, "Metre", null),
            new("Accessory", null, "FUR-023", "Синтепон 150", 3.5m, "Metre", null),
        ]));

        var rows = (await new GetModelCompositionsQueryHandler(workshop.Models)
            .Handle(new GetModelCompositionsQuery(), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(rows.Select(x => x.ModelCode), Is.EqualTo(new[] { "MOD-025", "MOD-052" }));
            Assert.That(rows[0].Lines, Is.Empty, "a model without composition is still listed");
            Assert.That(rows[1].Lines, Has.Count.EqualTo(4));
            Assert.That(rows[1].Layers.Select(x => (x.Purpose, x.Code, x.Quantity)),
                Is.EqualTo(new[] { ("Підкладка", (string?)"FUR-021", 1.9m), ("Утеплювач", (string?)"FUR-023", 3.5m) }));
            Assert.That((rows[1].ProductId, rows[1].Season), Is.EqualTo((52, "Зима")));
        });
    }

    [TestCase("Підкладка поліестерова", ModelLayer.Lining)]
    [TestCase("Синтепон 200", ModelLayer.Insulation)]
    [TestCase("Фліс", ModelLayer.Fleece)]
    [TestCase("Нитки ЛЛ", null)]
    [TestCase(null, null)]
    public void Layer_purpose_is_recognised_by_material_name(string? name, string? purpose) =>
        Assert.That(ModelLayer.PurposeOf(name), Is.EqualTo(purpose));
}
