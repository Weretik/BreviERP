using Production.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using Production.Application.Features.Payroll.Commands.CreatePayrollAdjustment;
using Production.Application.Features.Payroll.Commands.CreatePayrollPayment;
using Production.Application.Features.Payroll.Commands.SetPayrollEmbroidery;
using Production.Application.Features.Payroll.Commands.SetPayrollIssued;
using Production.Application.Features.Payroll.Commands.SetPayrollRoles;
using Production.Application.Features.Payroll.Queries.GetPayrollEmployee;
using Production.Application.Features.Payroll.Queries.GetPayrollYear;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Timesheet.Commands.SaveTimesheetDay;
using Production.Application.Features.Timesheet.Shared;
using Production.Application.Features.Payroll.Shared.Employee;
using Production.Application.Features.Payroll.Shared.Month;
using Production.Application.Features.Timesheet.Shared.Month;

namespace UnitTests.Production.Payroll;

/// <summary>Timesheet and payroll types and operations match the generated contract docs/sdd/contracts/openapi.json (PRD-006).</summary>
public sealed class ProductionPayrollContractTests
{
    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_workshop_policy()
    {
        var endpoints = MappedEndpoints.Of(app => app.MapProductionEndpoints(), "Payroll", "Timesheet");
        var policies = endpoints.SelectMany(x => x.Policies).Distinct().ToArray();
        var operations = endpoints.Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "access:payroll", "access:timesheet" }));
            Assert.That(operations, Has.Length.EqualTo(14));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }
}
