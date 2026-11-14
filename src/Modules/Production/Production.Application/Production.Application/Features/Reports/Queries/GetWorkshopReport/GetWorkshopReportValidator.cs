using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Reports.Shared;

namespace Production.Application.Features.Reports.Queries.GetWorkshopReport;

public sealed class GetWorkshopReportValidator : AbstractValidator<GetWorkshopReportQuery>
{
    public GetWorkshopReportValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x).Custom((query, context) =>
        {
            Check(query.From, query.To, nameof(query.From), context);
            if (query.CompareFrom is null && query.CompareTo is null) return;
            if (query.CompareFrom is null || query.CompareTo is null)
                context.AddFailure(nameof(query.CompareFrom), ReportMessages.Comparison);
            else Check(query.CompareFrom, query.CompareTo, nameof(query.CompareFrom), context);
        });
    }

    private static void Check(string fromText, string toText, string property,
        ValidationContext<GetWorkshopReportQuery> context)
    {
        if (!PayrollMonths.TryParse(fromText, out var from) || !PayrollMonths.TryParse(toText, out var to))
            context.AddFailure(property, ReportMessages.Month);
        else if (from > to) context.AddFailure(property, ReportMessages.Range);
        else if (Months(from, to) > ReportMessages.MaxMonths) context.AddFailure(property, ReportMessages.TooLong);
    }

    private static int Months(DateOnly from, DateOnly to) => (to.Year - from.Year) * 12 + to.Month - from.Month + 1;
}
