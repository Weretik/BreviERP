namespace Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Orders;

/// <summary><c>fabric_issue_components</c>: the movement a line was issued with.</summary>
public sealed record V133IssueComponentRow(string MovementId, string LineId);
