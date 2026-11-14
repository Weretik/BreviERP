namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

/// <summary>What the transfer did, or would do on a dry run.</summary>
/// <param name="Applied">The Brevi stock was replaced.</param>
/// <param name="Unchanged">This export was already transferred and nothing was entered in Brevi since.</param>
/// <param name="Hash">SHA-256 of the export rows; the journal knows each transferred export by it.</param>
/// <param name="Target">What Brevi gets; null when the export has errors or is unchanged.</param>
/// <param name="Current">What the Brevi stock holds now, and will lose.</param>
/// <param name="NeedsConfirmation">Records entered in Brevi itself would be removed; they are listed in Current.</param>
public sealed record StockTransferResult(
    bool Applied,
    bool Unchanged,
    string Hash,
    StockTransferSource Source,
    StockTransferTarget? Target,
    StockTransferCurrent Current,
    bool NeedsConfirmation,
    IReadOnlyList<StockTransferProblem> Errors,
    IReadOnlyList<StockTransferProblem> Notes,
    DateTimeOffset? LastTransferAt);
