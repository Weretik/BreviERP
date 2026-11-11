using BuildingBlocks.Api.Result;
using Content.Application.Contracts;
using Content.Application.PublicSiteCompany;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.PublicSiteCompany;

/// <summary>
/// The company facts of the public site (CNT-009): read-only, without sign-in. Only what every visitor sees on the
/// site anyway: phones, emails, address, hours, managers, payment details and profile links.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/company")]
[ResponseCache(Duration = CacheSeconds, Location = ResponseCacheLocation.Any)]
public sealed class PublicSiteCompanyController(ISender sender) : ControllerBase
{
    public const int CacheSeconds = 300;

    [HttpGet(Name = "getPublicSiteCompany")]
    public async Task<ActionResult<PublicSiteCompanyResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPublicSiteCompanyQuery(), cancellationToken));
}
