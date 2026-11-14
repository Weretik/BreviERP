using Production.Application.Features.Reports.Shared;
namespace Production.Application.Features.Reports.Queries.GetWorkshopReport;

/// <summary>«Звіти цеху» (PRD-D48): the chosen months and, when asked, the months to compare with.</summary>
public sealed record WorkshopReportPairResponse(WorkshopReportResponse Current, WorkshopReportResponse? Comparison);
