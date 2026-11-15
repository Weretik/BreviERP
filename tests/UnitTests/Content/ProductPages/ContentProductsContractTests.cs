using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Content.Domain.ProductPages;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;
using Content.Application.Features.ProductPages.Commands.SetContentProductVisibility;
using Content.Application.Features.ProductPages.Queries.GetContentProducts;
using Content.Application.Features.ProductPages.Shared;
using Content.Api.Endpoints.ProductPages.ImportOldSiteProductPages;
using Content.Api.Endpoints.ProductPages.SaveContentProduct;
using Content.Api.Endpoints.ProductPages.SetContentProductVisibility;
using Identity.Domain.Authorization.Roles;
using Content.Application.Features.ProductPages.Shared.Responses;

namespace UnitTests.Content.ProductPages;

/// <summary>«Контент → Товари» types and operations match the generated contract docs/sdd/contracts/openapi.json (CNT-002).</summary>
public sealed class ContentProductsContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [Test]
    public void Block_kind_is_written_by_name_as_in_the_contract()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ApiContract.EnumValues("ProductTextBlockKind"), Is.EqualTo(Enum.GetNames<ProductTextBlockKind>()));
            Assert.That(JsonSerializer.Serialize(ProductTextBlockKind.List), Is.EqualTo("\"List\""));
        });
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_content_products_policy()
    {
        var controller = "ContentProducts";
        var policy = MappedEndpoints.Policy(Map, controller);
        var operations = MappedEndpoints.Of(Map, controller)
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentProducts}"));
            Assert.That(operations, Has.Length.EqualTo(6));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_full_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentProducts)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Actions.HasFlag(AccessAction.Edit | AccessAction.Delete), Is.True);
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentProducts], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentProducts], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Accountant), Does.Not.ContainKey(AccessCatalog.ContentProducts));
        });
    }
}
