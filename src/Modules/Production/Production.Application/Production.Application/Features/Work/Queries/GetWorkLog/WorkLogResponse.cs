using Production.Application.Features.Work.Shared;
namespace Production.Application.Features.Work.Queries.GetWorkLog;

/// <summary>«Журнал робіт» of a month, newest first.</summary>
public sealed record WorkLogResponse(string Month, IReadOnlyList<ProductionWorkResponse> Rows, WorkLogTotals Totals);
