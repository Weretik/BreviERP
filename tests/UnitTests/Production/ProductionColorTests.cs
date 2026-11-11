using Ardalis.Result;
using Production.Application.Colors;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.UpdateOrder;

namespace UnitTests.Production;

/// <summary>The workshop picks colours only from «Кольори» (REF-006).</summary>
public sealed class ProductionColorTests
{
    private static readonly ProductionActor Manager = new(2, false, true, false);
    private static readonly ProductionActor Chief = new(4, false, false, true);
    private static readonly DateOnly Day = new(2026, 11, 4);

    [Test]
    public async Task Order_with_a_colour_outside_the_list_is_rejected()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var input = ProductionOrderHandlerTests.Input();
        input = input with
        {
            Items = [input.Items[0] with { Details = input.Items[0].Details with { AdditionalColors = ["Бузковий"] } }]
        };

        var result = await ProductionOrderHandlerTests.Create(workshop, Manager, input);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(result.ValidationErrors.Single().ErrorMessage, Does.Contain("«Бузковий»"));
            Assert.That(workshop.Db.Orders.Count(), Is.Zero);
        });
    }

    [Test]
    public async Task Colour_matches_the_list_ignoring_case_and_outer_spaces()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var input = ProductionOrderHandlerTests.Input();
        input = input with
        {
            Items = [input.Items[0] with { Details = input.Items[0].Details with { MainColor = "  темно-сірий " } }]
        };

        var result = await ProductionOrderHandlerTests.Create(workshop, Manager, input);

        Assert.That(result.IsSuccess, Is.True, string.Join("; ", result.ValidationErrors.Select(x => x.ErrorMessage)));
    }

    [Test]
    public async Task Edit_keeps_a_colour_the_item_already_had_after_it_left_the_list()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        var created = (await ProductionOrderHandlerTests.Create(workshop, Manager, ProductionOrderHandlerTests.Input()))
            .Value;
        workshop.ColorList.Colors.Remove("Червоний");
        var item = created.Items.Single();
        var handler = new UpdateProductionOrderCommandHandler(workshop.Db, workshop.Staff, workshop.Cards(),
            workshop.EmbroideryLinks(), workshop.Colors(), workshop.Time);

        var kept = await handler.Handle(new(Chief, created.Id, new UpdateProductionOrderInput(created.Version,
            created.ClientName, created.OrderDate, null,
            [new(item.Id, ProductionOrderHandlerTests.Details(4), null)])), CancellationToken.None);
        var replaced = await handler.Handle(new(Chief, created.Id, new UpdateProductionOrderInput(kept.Value.Version,
            created.ClientName, created.OrderDate, null,
            [new(item.Id, ProductionOrderHandlerTests.Details(4) with { MainColor = "Бузковий" }, null)])),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(kept.IsSuccess, Is.True, string.Join("; ", kept.ValidationErrors.Select(x => x.ErrorMessage)));
            Assert.That(replaced.Status, Is.EqualTo(ResultStatus.Invalid));
        });
    }

    [Test]
    public async Task Colours_query_lists_the_active_colours_in_order()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();

        var colors = await new GetProductionColorsQueryHandler(workshop.Colors())
            .Handle(new GetProductionColorsQuery(), CancellationToken.None);

        Assert.That(colors.Value, Is.EqualTo(workshop.ColorList.Colors));
    }
}
