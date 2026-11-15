using System.Text.Json;
using UnitTests.TestSupport;
using Warehouse.Api.DependencyInjection;
using Warehouse.Application.Features.Stock.Commands.AdjustFabricStock;
using Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;
using Warehouse.Application.Features.Stock.Commands.EditFabricMovement;
using Warehouse.Application.Features.Stock.Commands.IssueFabric;
using Warehouse.Application.Features.Stock.Commands.OpenFabricStock;
using Warehouse.Application.Features.Stock.Commands.ReceiveFabric;
using Warehouse.Application.Features.Stock.Commands.RefreshItemFabricPlan;
using Warehouse.Application.Features.Stock.Commands.RestoreFabricMovement;
using Warehouse.Application.Features.Stock.Commands.ReverseFabricIssue;
using Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;
using Warehouse.Application.Features.Stock.Commands.TransferStock;
using Warehouse.Application.Features.Stock.Commands.VoidFabricMovement;
using Warehouse.Application.Features.Stock.Queries.GetFabricConsumption;
using Warehouse.Application.Features.Stock.Queries.GetFabricIssueItems;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovementHistory;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements;
using Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;
using Warehouse.Application.Features.Stock.Queries.GetFabricStock;
using Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices;
using Warehouse.Application.Features.Stock.Queries.GetFabricValuation;
using Warehouse.Application.Features.Stock.Queries.GetHardwareOrder;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Commands.TransferStock.Transfer;
using Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Orders;
using Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Stock;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;
using Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact.Responses;
using Warehouse.Application.Features.Stock.Queries.GetFabricStock.Results;
using Warehouse.Application.Features.Stock.Queries.GetFabricValuation.Results;
using Warehouse.Application.Features.Stock.Queries.GetHardwareOrder.Results;
using Warehouse.Application.Features.Stock.Shared.Booking;
using Warehouse.Application.Features.Stock.Shared.Issues;
using Warehouse.Application.Features.Stock.Shared.Operations;
using Warehouse.Application.Features.Stock.Shared.Plans;

namespace UnitTests.Warehouse;

/// <summary>Stock types and operations match the generated contract docs/sdd/contracts/openapi.json (WH-001…WH-006).</summary>
public sealed class FabricStockContractTests
{
    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_orders_policy()
    {
        var endpoints = MappedEndpoints.Of(app => app.MapWarehouseEndpoints()).Where(x => !Reports.Contains(x.Name))
            .ToArray();
        var policies = endpoints.SelectMany(x => x.Policies).Distinct().Order().ToArray();
        var operations = endpoints.Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "access:fabric-stock", "access:production-orders" }));
            Assert.That(operations, Has.Length.EqualTo(22));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }

    private static readonly string[] Reports = ["getFabricPlanFact", "getFabricBooking", "getFabricValuation", "getHardwareOrder"];

    [Test]
    public void Every_report_is_in_the_contract_and_is_its_own_page_of_access_rights()
    {
        var actions = MappedEndpoints.Of(app => app.MapWarehouseEndpoints()).Where(x => Reports.Contains(x.Name))
            .Select(x => (x.Name, Policy: x.Policies.SingleOrDefault())).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(actions.Select(x => x.Name), Is.EquivalentTo(Reports));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(Reports));
            Assert.That(actions.Select(x => x.Policy), Is.EquivalentTo(new[]
            {
                "access:fabric-plan-fact", "access:fabric-booking", "access:fabric-valuation", "access:hardware-order"
            }));
            Assert.That(actions.Select(x => global::Identity.Application.Security.Access.AccessPolicies.Parse(x.Policy!)),
                Has.None.Null, "every policy names a page of «Права доступу»");
            Assert.That(global::Identity.Domain.Authorization.AccessCatalog.DefaultsOf("Manager"),
                Does.Not.ContainKey(global::Identity.Domain.Authorization.AccessCatalog.FabricValuation), "the owner's page");
        });
    }
}
