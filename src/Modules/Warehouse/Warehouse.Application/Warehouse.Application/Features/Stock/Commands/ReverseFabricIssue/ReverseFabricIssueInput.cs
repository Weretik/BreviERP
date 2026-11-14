namespace Warehouse.Application.Features.Stock.Commands.ReverseFabricIssue;

public sealed record ReverseFabricIssueInput(int OrderVersion, string Reason);
