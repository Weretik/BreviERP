using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.PayrollParameters.GetPayrollParameters;

namespace Production.Api.PayrollParameters;

/// <summary>«Правила зарплати»: read-only view of the payroll values kept in «Параметри ціноутворення».</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.PayrollRules)]
[Route("api/production/payroll-parameters")]
public sealed class PayrollParametersController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getPayrollParameters")]
    public async Task<ActionResult<IReadOnlyList<PayrollParameterResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPayrollParametersQuery(), cancellationToken));
}
