namespace Warehouse.Api.Endpoints.FabricStock.DeleteFabricStock;

/// <summary>Stock lines are never deleted: their history stays; «Уточнити» brings a balance to zero.</summary>
internal static class DeleteFabricStockEndpoint
{
    public const string Deleted =
        "Складську позицію не видаляють, щоб зберегти історію. Уточніть залишок до нуля з причиною.";

    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", Handle).WithName("deleteFabricStock")
            .ProducesProblem(StatusCodes.Status410Gone);

    private static IResult Handle(int id) => Results.Problem(statusCode: StatusCodes.Status410Gone, title: Deleted);
}
