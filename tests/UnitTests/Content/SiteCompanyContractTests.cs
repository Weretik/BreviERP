using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using YamlDotNet.RepresentationModel;
using Content.Application.Features.SiteCompany.Shared;
using Content.Api.Endpoints.SiteCompany.SaveSiteCompany;
using Identity.Domain.Authorization.Roles;

namespace UnitTests.Content;

/// <summary>
/// «Контакти та компанія» and the public company facts match docs/sdd/contracts/content/content-company.openapi.yaml
/// and content-public-company.openapi.yaml (CNT-009).
/// </summary>
public sealed class SiteCompanyContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    private const string Admin = "content-company.openapi.yaml";
    private const string Public = "content-public-company.openapi.yaml";

    [TestCase(typeof(SiteCompanyResponse), Admin, "SiteCompanyResponse")]
    [TestCase(typeof(SaveSiteCompanyRequest), Admin, "SaveSiteCompanyRequest")]
    [TestCase(typeof(CompanyManager), Admin, "CompanyManager")]
    [TestCase(typeof(CompanyText), Admin, "CompanyText")]
    [TestCase(typeof(CompanySchedule), Admin, "CompanySchedule")]
    [TestCase(typeof(CompanyBankDetails), Admin, "CompanyBankDetails")]
    [TestCase(typeof(CompanySocials), Admin, "CompanySocials")]
    [TestCase(typeof(PublicSiteCompanyResponse), Public, "PublicSiteCompanyResponse")]
    [TestCase(typeof(PublicCompanyManager), Public, "PublicCompanyManager")]
    [TestCase(typeof(PublicPhone), Public, "PublicPhone")]
    [TestCase(typeof(CompanyText), Public, "CompanyText")]
    [TestCase(typeof(CompanySchedule), Public, "CompanySchedule")]
    [TestCase(typeof(CompanyBankDetails), Public, "CompanyBankDetails")]
    [TestCase(typeof(CompanySocials), Public, "CompanySocials")]
    public void Serialized_properties_match_the_machine_contract(Type type, string file, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas(file)[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_company_policy()
    {
        var controller = "SiteCompany";
        var policy = MappedEndpoints.Policy(Map, controller);

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentCompany}"));
            Assert.That(Operations(Admin), Is.EquivalentTo(OperationsOf(controller)));
            Assert.That(Operations(Public), Is.EquivalentTo(OperationsOf("PublicSiteCompany")));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_edit_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentCompany)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Контакти та компанія"));
            Assert.That(area.Actions, Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentCompany], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentCompany], Is.EqualTo(AccessAction.View));
        });
    }

    private static string[] OperationsOf(string controller) => MappedEndpoints.Of(Map, controller)
        .Select(x => x.Name!).ToArray();

    private static YamlMappingNode Root(string file)
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/content", file));
        var yaml = new YamlStream();
        yaml.Load(reader);
        return (YamlMappingNode)yaml.Documents[0].RootNode;
    }

    private static YamlMappingNode Schemas(string file) =>
        (YamlMappingNode)((YamlMappingNode)Root(file)["components"])["schemas"];

    private static IEnumerable<string> Operations(string file) =>
        ((YamlMappingNode)Root(file)["paths"]).Children.Values.Cast<YamlMappingNode>()
        .SelectMany(path => path.Children.Where(x => ((YamlScalarNode)x.Key).Value != "parameters"))
        .Select(x => ((YamlScalarNode)((YamlMappingNode)x.Value)["operationId"]).Value!);
}
