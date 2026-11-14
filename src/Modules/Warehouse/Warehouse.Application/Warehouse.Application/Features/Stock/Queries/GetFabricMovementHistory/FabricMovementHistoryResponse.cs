namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovementHistory;

/// <summary>«Історія правок» of a movement, newest first.</summary>
/// <param name="CanRestore">The last edit or void can be taken back now.</param>
public sealed record FabricMovementHistoryResponse(
    int MovementId,
    int Revision,
    bool CanRestore,
    IReadOnlyList<FabricMovementRevisionResponse> Revisions);
