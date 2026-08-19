using System.Globalization;
using Catalog.Application.Features.Product.Create;
using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Create.Validators;
using Catalog.Application.Features.Product.Delete;
using Catalog.Application.Features.Product.Delete.Validators;
using Catalog.Application.Features.Product.GetAdminDetail;
using Catalog.Application.Features.Product.GetAdminDetail.Validators;
using Catalog.Application.Features.Product.GetAdminList;
using Catalog.Application.Features.Product.GetAdminList.Validators;
using Catalog.Application.Features.Product.Update;
using Catalog.Application.Features.Product.Update.Validators;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace UnitTests;

public sealed class ProductValidationTests
{
    [Test]
    public void Create_returns_ukrainian_indexed_validation_error_for_invalid_photo()
    {
        using var _ = UseCulture("uk-UA");
        var request = ValidSewingRequest() with { Photos = [new ProductPhotoRequest(0, null, true, true, 0)] };

        var result = new CreateProductCommandValidator().Validate(new CreateProductCommand(request));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(x =>
                x.PropertyName == "Request.Photos[0].MediaFileId" && x.ErrorMessage.Contains("ID медіафайлу")));
        });
    }

    [Test]
    public void Create_uses_request_culture_only_for_message()
    {
        var request = ValidSewingRequest() with { Name = string.Empty };
        using var _ = UseCulture("ru-RU");

        var result = new CreateProductCommandValidator().Validate(new CreateProductCommand(request));

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(x =>
            x.PropertyName == "Request.Name" && x.ErrorMessage.Contains("Украинское название")));
    }

    [Test]
    public void Create_accepts_valid_sewing_request()
    {
        var result = new CreateProductCommandValidator().Validate(new CreateProductCommand(ValidSewingRequest()));

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Update_rejects_route_and_request_id_mismatch()
    {
        var result = new ReplaceProductCommandValidator().Validate(new ReplaceProductCommand(2, ValidSewingRequest() with { Id = 1 }));

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(x => x.PropertyName == "Request.Id"));
    }

    [Test]
    public void Update_rejects_ppe_with_invalid_custom_percent()
    {
        var request = ValidPpeRequest() with
        {
            Ppe = ValidPpeRequest().Ppe! with
            {
                RetailPercent = new PercentRequest(PricePercentSource.Custom, null, -1)
            }
        };

        var result = new ReplaceProductCommandValidator().Validate(new ReplaceProductCommand(1, request));

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(x =>
            x.PropertyName == "Request.Ppe.RetailPercent.CustomPercent"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Delete_rejects_non_positive_id(int id)
        => Assert.That(new DeleteProductCommandValidator().Validate(new DeleteProductCommand(id)).IsValid, Is.False);

    [TestCase(0)]
    [TestCase(-1)]
    public void Detail_rejects_non_positive_id(int id)
        => Assert.That(new GetAdminProductDetailQueryValidator().Validate(new GetAdminProductDetailQuery(id)).IsValid, Is.False);

    [Test]
    public void List_rejects_invalid_paging_search_category_and_sorting()
    {
        var result = new GetAdminProductsQueryValidator().Validate(
            new GetAdminProductsQuery(0, 15, " ", null, 0, "price"));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Select(x => x.PropertyName), Is.EquivalentTo(["Page", "PageSize", "Search", "CategoryId", "SortBy"]));
        });
    }

    [Test]
    public void List_accepts_documented_paging_boundary()
        => Assert.That(new GetAdminProductsQueryValidator().Validate(new GetAdminProductsQuery(1, 50, "тест", ProductType.Sewing, 1, "updatedAt")).IsValid, Is.True);

    private static CreateProductCommandRequest ValidSewingRequest()
        => new(
            1,
            "Куртка",
            "Куртка",
            ProductType.Sewing,
            "Опис",
            "Описание",
            [1],
            [],
            [],
            [],
            new SewingProductRequest(1, [], [], []),
            null);

    private static CreateProductCommandRequest ValidPpeRequest()
        => ValidSewingRequest() with
        {
            Type = ProductType.Ppe,
            Sewing = null,
            Ppe = new PpeProductRequest(
                1,
                1,
                new PercentRequest(PricePercentSource.Custom, null, 0),
                new PercentRequest(PricePercentSource.Reference, 1, null))
        };

    private static IDisposable UseCulture(string name)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var culture = new CultureInfo(name);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        return new RestoreCulture(originalCulture, originalUiCulture);
    }

    private sealed class RestoreCulture(CultureInfo culture, CultureInfo uiCulture) : IDisposable
    {
        public void Dispose()
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiCulture;
        }
    }
}
