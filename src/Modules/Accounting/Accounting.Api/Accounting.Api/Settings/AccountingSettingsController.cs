using Accounting.Api.Access;
using Accounting.Application.Contracts;
using Accounting.Application.Settings;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Settings;

/// <summary>Fixed rules of the books for the forms: cutover date and today in Kyiv.</summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/settings")]
public sealed class AccountingSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAccountingSettings")]
    public async Task<ActionResult<AccountingSettingsResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetAccountingSettingsQuery(), cancellationToken));
}
