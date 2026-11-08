using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Production.Api.Payroll;
using Production.Api.Timesheet;
using Production.Application.Contracts;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Production;

/// <summary>Timesheet and payroll types and operations match docs/sdd/contracts/production/payroll.openapi.yaml (PRD-006).</summary>
public sealed class ProductionPayrollContractTests
{
    [TestCase(typeof(TimesheetDayResponse))]
    [TestCase(typeof(TimesheetRowResponse))]
    [TestCase(typeof(TimesheetChangeResponse))]
    [TestCase(typeof(TimesheetMonthResponse))]
    [TestCase(typeof(TimesheetMonthDayResponse))]
    [TestCase(typeof(TimesheetMonthEmployeeResponse))]
    [TestCase(typeof(SaveTimesheetDayInput))]
    [TestCase(typeof(TimesheetEntryInput))]
    [TestCase(typeof(PayrollMonthResponse))]
    [TestCase(typeof(PayrollRatesResponse))]
    [TestCase(typeof(PayrollPersonResponse))]
    [TestCase(typeof(PayrollLineResponse))]
    [TestCase(typeof(PayrollTotalsResponse))]
    [TestCase(typeof(PayrollPaymentResponse))]
    [TestCase(typeof(PayrollAdjustmentResponse))]
    [TestCase(typeof(PayrollPermissionsResponse))]
    [TestCase(typeof(PayrollEmployeeResponse))]
    [TestCase(typeof(PayrollWorkRowResponse))]
    [TestCase(typeof(PayrollYearResponse))]
    [TestCase(typeof(PayrollYearMonthResponse))]
    [TestCase(typeof(PayrollYearRowResponse))]
    [TestCase(typeof(SetPayrollRolesInput))]
    [TestCase(typeof(SetPayrollEmbroideryInput))]
    [TestCase(typeof(CreatePayrollPaymentInput))]
    [TestCase(typeof(CreatePayrollAdjustmentInput))]
    [TestCase(typeof(VoidPayrollEntryInput))]
    [TestCase(typeof(SetPayrollIssuedInput))]
    public void Serialized_properties_match_the_machine_contract(Type type)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[type.Name])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_workshop_policy()
    {
        Type[] controllers = [typeof(PayrollController), typeof(TimesheetController)];
        var policies = controllers.Select(controller => controller.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>().Single().Policy).Distinct().ToArray();
        var operations = controllers.SelectMany(controller => controller.GetMethods())
            .SelectMany(x => x.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>())
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "access:payroll", "access:timesheet" }));
            Assert.That(operations, Has.Length.EqualTo(14));
            Assert.That(Operations(), Is.EquivalentTo(operations));
        });
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/production/payroll.openapi.yaml"));
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
