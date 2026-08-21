using Ardalis.Result;
using Catalog.Api.Contracts.Products;
using Catalog.Domain.Products.Enums;
using System.Text.Json;

namespace UnitTests;

public sealed class ProductWriteRequestMapperTests
{
    [Test]
    public void Maps_valid_sewing_request()
    {
        var result = ProductWriteRequestMapper.Map(CreateRequest("Sewing", metersPerProduct: 1m));

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));
            Assert.That(result.Value.Type, Is.EqualTo(ProductType.Sewing));
            Assert.That(result.Value.Sewing, Is.Not.Null);
        });
    }

    [TestCase("Reference")]
    [TestCase("Custom")]
    public void Maps_valid_ppe_percent_source(string source)
    {
        var result = ProductWriteRequestMapper.Map(CreateRequest("Ppe", retailPercent: new(source, 7, 12m), wholesalePercent: new(source, 8, 15m)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));
            Assert.That(result.Value.Type, Is.EqualTo(ProductType.Ppe));
            Assert.That(result.Value.Ppe, Is.Not.Null);
        });
    }

    [TestCase("sewing")]
    [TestCase("1")]
    [TestCase("Unknown")]
    public void Rejects_noncanonical_product_type(string type)
    {
        var result = ProductWriteRequestMapper.Map(CreateRequest(type));

        Assert.That(result.ValidationErrors, Has.Some.Matches<ValidationError>(x => x.Identifier == "type"));
    }

    [TestCase("reference")]
    [TestCase("CUSTOM")]
    [TestCase("1")]
    [TestCase("Unknown")]
    public void Rejects_noncanonical_percent_source_on_create_and_replace(string source)
    {
        var createResult = ProductWriteRequestMapper.Map(CreateRequest("Ppe", retailPercent: new(source, 7, null), wholesalePercent: new("Reference", 8, null)));
        var replaceResult = ProductWriteRequestMapper.Map(ReplaceRequest("Ppe", retailPercent: new("Custom", null, 12m), wholesalePercent: new(source, 8, null)));

        Assert.Multiple(() =>
        {
            Assert.That(createResult.ValidationErrors, Has.Some.Matches<ValidationError>(x => x.Identifier == "retailPercent.source"));
            Assert.That(replaceResult.ValidationErrors, Has.Some.Matches<ValidationError>(x => x.Identifier == "wholesalePercent.source"));
        });
    }

    [Test]
    public void Preserves_foreign_type_specific_blocks_for_application_validation()
    {
        var result = ProductWriteRequestMapper.Map(CreateRequest(
            "Sewing",
            metersPerProduct: 1m,
            retailPercent: new("Reference", 7, null),
            wholesalePercent: new("Custom", null, 12m)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));
            Assert.That(result.Value.Sewing, Is.Not.Null);
            Assert.That(result.Value.Ppe, Is.Not.Null);
        });
    }

    [Test]
    public void Rejects_numeric_enum_values_deserialized_from_json()
    {
        const string json = """
            { "Id": 101, "Type": 1, "Name": "Name", "RuName": "Ru name", "DescriptionUk": "Description", "DescriptionRu": "Описание", "CategoryIds": [], "Photos": [], "InformationBlocks": [], "CharacteristicTables": [], "SupplierId": 5, "BasePrice": 100, "RetailPercent": { "Source": 1 }, "WholesalePercent": { "Source": 2 } }
            """;
        var request = JsonSerializer.Deserialize<CreateProductRequest>(json)!;

        var result = ProductWriteRequestMapper.Map(request);

        Assert.That(result.ValidationErrors.Select(x => x.Identifier), Is.EquivalentTo(["type", "retailPercent.source", "wholesalePercent.source"]));
    }

    private static CreateProductRequest CreateRequest(string type, decimal? metersPerProduct = null, PercentWriteRequest? retailPercent = null, PercentWriteRequest? wholesalePercent = null)
        => new(101, type, "Name", "Ru name", "Description", "Описание", [], [], [], [], metersPerProduct, [], [], [], retailPercent is null && wholesalePercent is null ? null : 5, retailPercent is null && wholesalePercent is null ? null : 100m, retailPercent, wholesalePercent);

    private static ProductWriteRequest ReplaceRequest(string type, PercentWriteRequest? retailPercent, PercentWriteRequest? wholesalePercent)
        => new(type, "Name", "Ru name", "Description", "Описание", [], [], [], [], null, [], [], [], 5, 100m, retailPercent, wholesalePercent);
}
