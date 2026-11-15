using Production.Api.DependencyInjection;
using UnitTests.TestSupport;
using System.Text.Json;
using Production.Application.Features.Orders.Commands.CancelProductionOrder;
using Production.Application.Features.Orders.Commands.CopyProductionOrder;
using Production.Application.Features.Orders.Commands.CreateProductionOrder;
using Production.Application.Features.Orders.Commands.CreateProductionShipment;
using Production.Application.Features.Orders.Commands.ImportProductionOrders;
using Production.Application.Features.Orders.Commands.ImportProductionProgress;
using Production.Application.Features.Orders.Commands.ImportProductionReportPrices;
using Production.Application.Features.Orders.Commands.SetProductionItemStages;
using Production.Application.Features.Orders.Commands.UpdateProductionOrder;
using Production.Application.Features.Orders.Queries.GetProductionEmbroideries;
using Production.Application.Features.Orders.Queries.GetProductionModelSummary;
using Production.Application.Features.Orders.Queries.GetProductionOrderFilterOptions;
using Production.Application.Features.Orders.Queries.GetProductionOrders;
using Production.Application.Features.Orders.Queries.GetProductionQuoteDraft;
using Production.Application.Features.Orders.Queries.GetProductionQuoteSources;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Settings.Commands.UpdateWorkshopSettings;
using Production.Application.Features.Settings.Shared;
using Production.Application.Features.Orders.Commands.ImportProductionOrders.Rows;
using Production.Application.Features.Orders.Commands.ImportProductionProgress.Rows;
using Production.Application.Features.Orders.Commands.ImportProductionReportPrices.Rows;
using Production.Application.Features.Orders.Queries.GetProductionOrders.Results;
using Production.Application.Features.Orders.Queries.GetProductionQuoteDraft.Results;
using Production.Application.Features.Orders.Shared.Items;
using Production.Application.Features.Orders.Shared.Responses;

namespace UnitTests.Production.Orders;

/// <summary>Order types and operations match the generated contract docs/sdd/contracts/openapi.json (PRD-002, PRD-003).</summary>
public sealed class ProductionOrdersContractTests
{
    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_orders_policy()
    {
        var endpoints = MappedEndpoints.Of(app => app.MapProductionEndpoints(), "ProductionOrders", "ProductionOrderTracking", "WorkshopSettings");
        var policies = endpoints.SelectMany(x => x.Policies).Distinct().ToArray();
        var operations = endpoints.Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policies, Is.EqualTo(new[] { "access:production-orders", "access:workshop-settings" }));
            Assert.That(operations, Has.Length.EqualTo(28));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }
}
