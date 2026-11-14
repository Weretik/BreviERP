using Production.Api.Access;
using Production.Api.Endpoints.Payroll.ClosePayrollMonth;
using Production.Api.Endpoints.Payroll.CreatePayrollAdjustment;
using Production.Api.Endpoints.Payroll.CreatePayrollPayment;
using Production.Api.Endpoints.Payroll.GetPayrollEmployee;
using Production.Api.Endpoints.Payroll.GetPayrollMonth;
using Production.Api.Endpoints.Payroll.GetPayrollYear;
using Production.Api.Endpoints.Payroll.ReopenPayrollMonth;
using Production.Api.Endpoints.Payroll.SetPayrollEmbroidery;
using Production.Api.Endpoints.Payroll.SetPayrollIssued;
using Production.Api.Endpoints.Payroll.SetPayrollRoles;
using Production.Api.Endpoints.Payroll.VoidPayrollAdjustment;
using Production.Api.Endpoints.Payroll.VoidPayrollPayment;

namespace Production.Api.Endpoints.Payroll;

/// <summary>
/// «Зарплата» (PRD-D42..D46): the head of the workshop and Admin see a month and set the roles and the embroidery;
/// payments, adjustments, «Видано» and closing are for Admin.
/// </summary>
internal static class PayrollEndpoints
{
    public static IEndpointRouteBuilder MapPayrollEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/payroll").RequireAuthorization(ProductionAccess.Payroll)
            .WithTags("Payroll");
        GetPayrollMonthEndpoint.Map(group);
        GetPayrollEmployeeEndpoint.Map(group);
        GetPayrollYearEndpoint.Map(group);
        SetPayrollRolesEndpoint.Map(group);
        SetPayrollEmbroideryEndpoint.Map(group);
        CreatePayrollPaymentEndpoint.Map(group);
        VoidPayrollPaymentEndpoint.Map(group);
        CreatePayrollAdjustmentEndpoint.Map(group);
        VoidPayrollAdjustmentEndpoint.Map(group);
        SetPayrollIssuedEndpoint.Map(group);
        ClosePayrollMonthEndpoint.Map(group);
        ReopenPayrollMonthEndpoint.Map(group);
        return app;
    }
}
