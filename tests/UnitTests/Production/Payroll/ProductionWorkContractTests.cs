using Production.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using Production.Application.Features.Forecast.Commands.SaveForecastQueue;
using Production.Application.Features.Forecast.Queries.GetForecast;
using Production.Application.Features.Work.Commands.CreateProductionWork;
using Production.Application.Features.Work.Commands.ImportProductionWork;
using Production.Application.Features.Work.Commands.VoidProductionWork;
using Production.Application.Features.Work.Queries.GetWorkChoices;
using Production.Application.Features.Work.Queries.GetWorkLog;
using Production.Application.Features.Work.Shared;

namespace UnitTests.Production.Payroll;

/// <summary>Work log and forecast types and operations match the generated contract docs/sdd/contracts/openapi.json (PRD-005).</summary>
public sealed class ProductionWorkContractTests
{
    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_orders_policy()
    {
        var endpoints = MappedEndpoints.Of(app => app.MapProductionEndpoints(), "ProductionWork", "ProductionForecast");
        var policies = endpoints.SelectMany(x => x.Policies).Distinct().ToArray();
        var operations = endpoints.Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "access:work-log", "access:forecast" }));
            Assert.That(operations, Has.Length.EqualTo(8));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }
}
