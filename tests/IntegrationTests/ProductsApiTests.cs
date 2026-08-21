using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Catalog.Api;
using Catalog.Application.Contracts.Admin.Product;
using Catalog.Application.Features.Product.Create;
using Catalog.Application.Features.Product.Delete;
using Catalog.Application.Features.Product.GetAdminDetail;
using Catalog.Application.Features.Product.GetAdminList;
using Catalog.Application.Features.Product.GetAdminList.DTOs;
using Catalog.Application.Features.Product.Update;
using Catalog.Domain.Products.Enums;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntegrationTests;

[NonParallelizable]
public sealed class ProductsApiTests
{
    [Test]
    public async Task List_is_anonymous_and_returns_main_photo_and_minimum_wholesale_price()
    {
        await using var host = await TestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<GetAdminProductsQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<PagedResult<IReadOnlyList<ProductListItem>>>(Page(
            [
                new(1, "Sewing", "sewing", ProductType.Sewing, [], new ProductMainPhoto(10, "https://cdn.test/main.jpg"), 123.456m, DateTimeOffset.UtcNow, null),
                new(2, "Ppe", "ppe", ProductType.Ppe, [], null, 99.99m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
                new(3, "Fallback", "fallback", ProductType.Sewing, [], null, 0m, DateTimeOffset.UtcNow, null)
            ])));

        var response = await host.Client.GetAsync("/api/v1/products?type=Sewing&sortDirection=desc&pageSize=50&page=2&search=work&categoryId=7&sortBy=updatedAt");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.GetProperty("value")[0].GetProperty("mainPhoto").GetProperty("url").GetString(), Is.EqualTo("https://cdn.test/main.jpg"));
            Assert.That(body.GetProperty("value")[1].GetProperty("mainPhoto").ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(body.GetProperty("value")[0].GetProperty("minimumWholesalePrice").GetDecimal(), Is.EqualTo(123.456m));
            Assert.That(body.GetProperty("value")[2].GetProperty("minimumWholesalePrice").GetDecimal(), Is.Zero);
            Assert.That(body.GetProperty("pagedInfo").GetProperty("pageNumber").GetInt32(), Is.EqualTo(2));
        });
        host.Sender.Verify(x => x.Send(It.Is<GetAdminProductsQuery>(q => q.Type == ProductType.Sewing && q.Descending && q.Page == 2 && q.PageSize == 50 && q.Search == "work" && q.CategoryId == 7 && q.SortBy == "updatedAt"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task List_returns_an_empty_successful_page()
    {
        await using var host = await TestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<GetAdminProductsQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<PagedResult<IReadOnlyList<ProductListItem>>>(Page([], totalRecords: 0)));

        var response = await host.Client.GetAsync("/api/v1/products");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.GetProperty("value").GetArrayLength(), Is.Zero);
            Assert.That(body.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt32(), Is.Zero);
        });
    }

    [Test]
    public async Task List_rejects_invalid_parameters_without_calling_mediator()
    {
        await using var host = await TestHost.CreateAsync();

        var response = await host.Client.GetAsync("/api/v1/products?type=sewing&sortDirection=descending");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(body.GetArrayLength(), Is.EqualTo(2));
            Assert.That(body.EnumerateArray().Select(x => x.GetProperty("identifier").GetString()), Is.EquivalentTo(["type", "sortDirection"]));
        });
        host.Sender.Verify(x => x.Send(It.IsAny<GetAdminProductsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Detail_returns_all_photo_urls_and_ppe_prices_or_not_found()
    {
        await using var host = await TestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.Is<GetAdminProductDetailQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<ProductAdminDetail>>(Result.Success(Detail())));
        host.Sender.Setup(x => x.Send(It.Is<GetAdminProductDetailQuery>(q => q.Id == 404), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<ProductAdminDetail>>(Result.NotFound()));

        var success = await host.Client.GetAsync("/api/v1/products/1");
        var successBody = await success.Content.ReadFromJsonAsync<JsonElement>();
        var missing = await host.Client.GetAsync("/api/v1/products/404");

        Assert.Multiple(() =>
        {
            Assert.That(success.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(successBody.GetProperty("type").GetString(), Is.EqualTo("Ppe"));
            Assert.That(successBody.GetProperty("photos").GetArrayLength(), Is.EqualTo(2));
            Assert.That(successBody.GetProperty("photos")[1].GetProperty("url").GetString(), Is.EqualTo("https://cdn.test/second.jpg"));
            Assert.That(successBody.GetProperty("ppe").GetProperty("retailPrice").GetDecimal(), Is.EqualTo(125.5m));
            Assert.That(successBody.GetProperty("ppe").GetProperty("wholesalePrice").GetDecimal(), Is.EqualTo(115.25m));
            Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task Create_and_replace_share_validation_error_body_and_do_not_call_mediator()
    {
        await using var host = await TestHost.CreateAsync();
        var create = await host.Client.PostAsJsonAsync("/api/v1/products", SewingWrite(1, "sewing"));
        var replace = await host.Client.PutAsJsonAsync("/api/v1/products/1", PpeWrite("reference"));
        var createBody = await create.Content.ReadFromJsonAsync<JsonElement>();
        var replaceBody = await replace.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Multiple(() =>
        {
            Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(replace.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(createBody.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(replaceBody.ValueKind, Is.EqualTo(JsonValueKind.Array));
        });
        host.Sender.Verify(x => x.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        host.Sender.Verify(x => x.Send(It.IsAny<ReplaceProductCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Create_rejects_numeric_type_without_calling_mediator()
    {
        await using var host = await TestHost.CreateAsync();

        var response = await host.Client.PostAsJsonAsync("/api/v1/products", new { id = 12, type = 1, name = "Name", ruName = "Ru name", descriptionUk = "Description", descriptionRu = "Описание", categoryIds = Array.Empty<int>(), photos = Array.Empty<object>(), informationBlocks = Array.Empty<object>(), characteristicTables = Array.Empty<object>(), metersPerProduct = 1m, fabrics = Array.Empty<object>(), accessories = Array.Empty<object>(), operationIds = Array.Empty<int>() });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(body.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(body[0].GetProperty("identifier").GetString(), Is.EqualTo("type"));
        });
        host.Sender.Verify(x => x.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Create_returns_created_and_conflicts_are_mapped()
    {
        await using var host = await TestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.Is<CreateProductCommand>(x => x.Request.Id == 10), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<ProductAdminDetail>>(Result.Success(Detail(10))));
        host.Sender.Setup(x => x.Send(It.Is<CreateProductCommand>(x => x.Request.Id == 11), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<ProductAdminDetail>>(Result.Conflict("Duplicate product.")));
        host.Sender.Setup(x => x.Send(It.Is<CreateProductCommand>(x => x.Request.Id == 12), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<ProductAdminDetail>>(Result.Conflict("Media file is not ready for Product usage.")));
        host.Sender.Setup(x => x.Send(It.IsAny<ReplaceProductCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<ProductAdminDetail>>(Result.Success(Detail(10))));

        var created = await host.Client.PostAsJsonAsync("/api/v1/products", SewingWrite(10));
        var conflict = await host.Client.PostAsJsonAsync("/api/v1/products", SewingWrite(11));
        var mediaConflict = await host.Client.PostAsJsonAsync("/api/v1/products", SewingWrite(12));
        var replaced = await host.Client.PutAsJsonAsync("/api/v1/products/10", PpeWrite("Reference"));

        Assert.Multiple(() =>
        {
            Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(created.Headers.Location!.AbsolutePath, Is.EqualTo("/api/v1/products/10"));
            Assert.That(conflict.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(mediaConflict.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(replaced.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    [Test]
    public async Task Delete_maps_success_not_found_and_reference_or_media_conflicts()
    {
        await using var host = await TestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.Is<DeleteProductCommand>(x => x.Id == 1), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result>(Result.Success()));
        host.Sender.Setup(x => x.Send(It.Is<DeleteProductCommand>(x => x.Id == 404), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result>(Result.NotFound()));
        host.Sender.Setup(x => x.Send(It.Is<DeleteProductCommand>(x => x.Id == 409), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result>(Result.Conflict("Referenced product cannot be deleted.")));

        var deleted = await host.Client.DeleteAsync("/api/v1/products/1");
        var missing = await host.Client.DeleteAsync("/api/v1/products/404");
        var conflict = await host.Client.DeleteAsync("/api/v1/products/409");

        Assert.Multiple(() =>
        {
            Assert.That(deleted.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(conflict.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        });
    }

    private static PagedResult<IReadOnlyList<ProductListItem>> Page(IReadOnlyList<ProductListItem> items, int totalRecords = -1)
        => new(new PagedInfo(2, 50, 1, totalRecords < 0 ? items.Count : totalRecords), items);

    private static object SewingWrite(int id, string type = "Sewing")
        => new { id, type, name = "Name", ruName = "Ru name", descriptionUk = "Description", descriptionRu = "Описание", categoryIds = Array.Empty<int>(), photos = Array.Empty<object>(), informationBlocks = Array.Empty<object>(), characteristicTables = Array.Empty<object>(), metersPerProduct = 1m, fabrics = Array.Empty<object>(), accessories = Array.Empty<object>(), operationIds = Array.Empty<int>() };

    private static object PpeWrite(string retailPercentSource)
        => new { type = "Ppe", name = "Name", ruName = "Ru name", descriptionUk = "Description", descriptionRu = "Описание", categoryIds = Array.Empty<int>(), photos = Array.Empty<object>(), informationBlocks = Array.Empty<object>(), characteristicTables = Array.Empty<object>(), supplierId = 1, basePrice = 100m, retailPercent = new { source = retailPercentSource, additionalReferenceId = 1, customPercent = (decimal?)null }, wholesalePercent = new { source = "Custom", additionalReferenceId = (int?)null, customPercent = 10m } };

    private static ProductAdminDetail Detail(int id = 1)
        => new(id, "Product", "Товар", "product", ProductType.Ppe, "Description", "Описание", [],
            [new(10, "https://cdn.test/main.jpg", null, true, true, 0), new(11, "https://cdn.test/second.jpg", "Second", true, false, 1)], [], [], null,
            new(new(7, "Supplier"), 100m, new(Catalog.Application.Contracts.Admin.Ppe.PpePercentSource.Custom, null, 25.5m), new(Catalog.Application.Contracts.Admin.Ppe.PpePercentSource.Custom, null, 15.25m), 125.5m, 115.25m), DateTimeOffset.UtcNow, null);

    private sealed class TestHost(WebApplication app, HttpClient client, Mock<ISender> sender) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;
        public Mock<ISender> Sender { get; } = sender;

        public static async Task<TestHost> CreateAsync()
        {
            var sender = new Mock<ISender>();
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddSingleton(sender.Object);
            builder.Services.AddControllers().AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly);

            var app = builder.Build();
            app.MapControllers();
            await app.StartAsync();
            return new(app, app.GetTestClient(), sender);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }
}
