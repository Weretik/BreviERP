using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Content.Domain.Articles;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;
using Content.Application.Features.Articles.Commands.SetContentArticleVisibility;
using Content.Application.Features.Articles.Queries.GetContentArticles;
using Content.Application.Features.Articles.Shared;
using Content.Api.Endpoints.Articles.ImportOldSiteArticles;
using Content.Api.Endpoints.Articles.SetContentArticleVisibility;
using Content.Api.Endpoints.Articles.Shared;
using Identity.Domain.Authorization.Roles;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles.Results;
using Content.Application.Features.Articles.Shared.Admin;

namespace UnitTests.Content.Articles;

/// <summary>«Контент → Статті» types and operations match the generated contract docs/sdd/contracts/openapi.json (CNT-004).</summary>
public sealed class ArticlesContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_content_articles_policy()
    {
        var controller = "ContentArticles";
        var policy = MappedEndpoints.Policy(Map, controller);
        var operations = MappedEndpoints.Of(Map, controller)
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentArticles}"));
            Assert.That(operations, Has.Length.EqualTo(7));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_full_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentArticles)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Статті"));
            Assert.That(area.Actions.HasFlag(AccessAction.Create | AccessAction.Edit | AccessAction.Delete), Is.True);
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentArticles], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentArticles], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Accountant), Does.Not.ContainKey(AccessCatalog.ContentArticles));
        });
    }
}
