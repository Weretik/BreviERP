using System.Text.Json;
using Accounting.Api.Accounts;
using Accounting.Api.Categories;
using Accounting.Api.Settings;
using Accounting.Application.Contracts;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Accounting;

/// <summary>Types and operations match docs/sdd/contracts/accounting/finance.openapi.yaml (ACC-001).</summary>
public sealed class AccountingContractTests
{
    [TestCase(typeof(AccountingSettingsResponse), "AccountingSettingsResponse")]
    [TestCase(typeof(FinanceAccountResponse), "FinanceAccountResponse")]
    [TestCase(typeof(FinanceAccountInput), "FinanceAccountInput")]
    [TestCase(typeof(FinanceAccountUpdateInput), "FinanceAccountUpdateInput")]
    [TestCase(typeof(FinanceAccountCutoverInput), "FinanceAccountCutoverInput")]
    [TestCase(typeof(FinanceVersionInput), "FinanceVersionInput")]
    [TestCase(typeof(FinanceCategoryResponse), "FinanceCategoryResponse")]
    [TestCase(typeof(FinanceCategoryInput), "FinanceCategoryInput")]
    [TestCase(typeof(FinanceCategoryUpdateInput), "FinanceCategoryUpdateInput")]
    [TestCase(typeof(FinanceHistoryEntryResponse), "FinanceHistoryEntryResponse")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [TestCase(typeof(FinanceAccountType), "FinanceAccountType")]
    [TestCase(typeof(CategoryDirection), "CategoryDirection")]
    [TestCase(typeof(LedgerScope), "LedgerScope")]
    [TestCase(typeof(AuditEntity), "AuditEntity")]
    [TestCase(typeof(AuditAction), "AuditAction")]
    public void Enum_values_match_the_machine_contract(Type type, string schema)
    {
        var values = (YamlSequenceNode)((YamlMappingNode)Schemas()[schema])["enum"];
        Assert.That(values.Children.Cast<YamlScalarNode>().Select(x => x.Value), Is.EqualTo(Enum.GetNames(type)));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_director_policy()
    {
        var controllers = new[] { typeof(FinanceAccountsController), typeof(FinanceCategoriesController), typeof(AccountingSettingsController) };
        var policies = controllers.Select(c => c.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>().Single().Policy).Distinct().ToArray();
        var operations = controllers.SelectMany(c => c.GetMethods())
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "CanUseAccounting" }));
            Assert.That(operations, Has.Length.EqualTo(14));
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/accounting/finance.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(reader);
        return (YamlMappingNode)yaml.Documents[0].RootNode;
    }

    private static YamlMappingNode Schemas() =>
        (YamlMappingNode)((YamlMappingNode)Root()["components"])["schemas"];

    private static IEnumerable<string> Operations() =>
        ((YamlMappingNode)Root()["paths"]).Children.Values.Cast<YamlMappingNode>()
        .SelectMany(path => path.Children.Values.OfType<YamlMappingNode>())
        .Where(operation => operation.Children.ContainsKey("operationId"))
        .Select(operation => ((YamlScalarNode)operation["operationId"]).Value!);
}
