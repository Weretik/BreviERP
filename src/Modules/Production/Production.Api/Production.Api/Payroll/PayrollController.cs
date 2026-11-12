using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Payroll.Close;
using Production.Application.Payroll.GetMonth;
using Production.Application.Payroll.Payments;
using Production.Application.Payroll.Setup;

namespace Production.Api.Payroll;

/// <summary>
/// «Зарплата» (PRD-D42..D46): the head of the workshop and Admin see a month and set the roles and the embroidery;
/// payments, adjustments, «Видано» and closing are for Admin.
/// </summary>
[ApiController]
[Authorize(Policy = ProductionAccess.Payroll)]
[Route("api/production/payroll")]
public sealed class PayrollController(ISender sender) : ControllerBase
{
    /// <summary>A month <c>YYYY-MM</c>; a closed month as it was closed.</summary>
    [HttpGet("{month}", Name = "getPayrollMonth")]
    public async Task<ActionResult<PayrollMonthResponse>> Month(string month, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetPayrollMonthQuery(actor, month), cancellationToken))
            : Forbid();

    [HttpGet("{month}/employees/{employeeId:int}", Name = "getPayrollEmployee")]
    public async Task<ActionResult<PayrollEmployeeResponse>> Employee(string month, int employeeId,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetPayrollEmployeeQuery(actor, month, employeeId), cancellationToken))
            : Forbid();

    /// <summary>«Історія за рік».</summary>
    [HttpGet("history/{year:int}", Name = "getPayrollYear")]
    public async Task<ActionResult<PayrollYearResponse>> Year(int year, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetPayrollYearQuery(actor, year), cancellationToken))
            : Forbid();

    /// <summary>«Керівні частки»: the master and the chief.</summary>
    [HttpPut("{month}/roles", Name = "setPayrollRoles")]
    public async Task<ActionResult<PayrollMonthResponse>> Roles(string month, SetPayrollRolesInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new SetPayrollRolesCommand(actor, month, input), cancellationToken))
            : Forbid();

    [HttpPut("{month}/embroidery", Name = "setPayrollEmbroidery")]
    public async Task<ActionResult<PayrollMonthResponse>> Embroidery(string month, SetPayrollEmbroideryInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new SetPayrollEmbroideryCommand(actor, month, input), cancellationToken))
            : Forbid();

    [HttpPost("{month}/payments", Name = "createPayrollPayment")]
    public async Task<ActionResult<PayrollMonthResponse>> CreatePayment(string month, CreatePayrollPaymentInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CreatePayrollPaymentCommand(actor, month, input), cancellationToken))
            : Forbid();

    [HttpPost("payments/{paymentId:int}/void", Name = "voidPayrollPayment")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<PayrollMonthResponse>> VoidPayment(int paymentId, VoidPayrollEntryInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new VoidPayrollPaymentCommand(actor, paymentId, input), cancellationToken))
            : Forbid();

    [HttpPost("{month}/adjustments", Name = "createPayrollAdjustment")]
    public async Task<ActionResult<PayrollMonthResponse>> CreateAdjustment(string month,
        CreatePayrollAdjustmentInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CreatePayrollAdjustmentCommand(actor, month, input),
                cancellationToken))
            : Forbid();

    [HttpPost("adjustments/{adjustmentId:int}/void", Name = "voidPayrollAdjustment")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<PayrollMonthResponse>> VoidAdjustment(int adjustmentId, VoidPayrollEntryInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new VoidPayrollAdjustmentCommand(actor, adjustmentId, input),
                cancellationToken))
            : Forbid();

    /// <summary>«Видано» of a worker; a null amount clears it.</summary>
    [HttpPut("{month}/issued", Name = "setPayrollIssued")]
    public async Task<ActionResult<PayrollMonthResponse>> Issued(string month, SetPayrollIssuedInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new SetPayrollIssuedCommand(actor, month, input), cancellationToken))
            : Forbid();

    /// <summary>Closes the month: keeps the calculation and locks work, hours and pay in it.</summary>
    [HttpPost("{month}/close", Name = "closePayrollMonth")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<PayrollMonthResponse>> Close(string month, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ClosePayrollMonthCommand(actor, month), cancellationToken))
            : Forbid();

    [HttpPost("{month}/reopen", Name = "reopenPayrollMonth")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<PayrollMonthResponse>> Reopen(string month, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ReopenPayrollMonthCommand(actor, month), cancellationToken))
            : Forbid();
}
