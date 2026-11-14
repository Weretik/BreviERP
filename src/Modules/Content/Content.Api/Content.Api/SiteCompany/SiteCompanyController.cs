using System.Security.Claims;
using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Content.Application.Features.SiteCompany.Commands.SaveSiteCompany;
using Content.Application.Features.SiteCompany.Queries.GetSiteCompany;
using Content.Application.Features.SiteCompany.Shared;

namespace Content.Api.SiteCompany;

public sealed record SaveSiteCompanyRequest(
    int Revision,
    string Phone,
    string Email,
    CompanyText City,
    CompanyText Street,
    string MapQuery,
    CompanySchedule Schedule,
    IReadOnlyList<CompanyManager> Managers,
    CompanyBankDetails BankDetails,
    CompanySocials Socials);

/// <summary>«Контент → Контакти та компанія» (CNT-009).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.CompanyPolicy)]
[Route("api/content/company")]
public sealed class SiteCompanyController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSiteCompany")]
    public async Task<ActionResult<SiteCompanyResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetSiteCompanyQuery(), cancellationToken));

    /// <summary>Replaces the facts as a whole: 409 when the revision is stale.</summary>
    [HttpPut(Name = "saveSiteCompany")]
    public async Task<ActionResult<SiteCompanyResponse>> Save([FromBody] SaveSiteCompanyRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SaveSiteCompanyCommand(request.Revision, request.Phone, request.Email,
            request.City, request.Street, request.MapQuery, request.Schedule, request.Managers, request.BankDetails,
            request.Socials, By()), cancellationToken));

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
