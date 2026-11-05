using System.Security.Claims;
using Ai.Application.Contracts;
using Ai.Application.Settings;
using Ai.Domain.Settings;
using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ai.Api.Settings;

/// <summary>«Адміністрування → Налаштування ШІ»: the OpenAI key (write-only), models, prices, on/off and the monthly limit.</summary>
[ApiController]
[Authorize(Policy = "RequireAdminRole")]
[Route("api/ai/settings")]
public sealed class AiSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAiSettings")]
    public async Task<ActionResult<AiSettingsResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetAiSettingsQuery(), cancellationToken));

    /// <summary>409 when the version is stale; 400 when switching on without a key.</summary>
    [HttpPut(Name = "updateAiSettings")]
    public async Task<ActionResult<AiSettingsResponse>> Update([FromBody] AiSettingsRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateAiSettingsCommand(request.Enabled, request.FastModel,
            request.FastInputPrice, request.FastOutputPrice, request.StrongModel, request.StrongInputPrice,
            request.StrongOutputPrice, request.MonthlyLimitUsd, request.Version, By()), cancellationToken));

    /// <summary>Replaces the key. The response never contains it, only its last four characters.</summary>
    [HttpPut("key", Name = "setAiKey")]
    public async Task<ActionResult<AiSettingsResponse>> SetKey([FromBody] AiKeyRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SetAiKeyCommand(request.ApiKey ?? string.Empty, By()), cancellationToken));

    /// <summary>Removes the key and switches the AI off.</summary>
    [HttpDelete("key", Name = "removeAiKey")]
    public async Task<ActionResult<AiSettingsResponse>> RemoveKey(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new RemoveAiKeyCommand(By()), cancellationToken));

    /// <summary>Extra rules of one request kind, e.g. <c>purchase-invoices</c>; empty text removes them.</summary>
    [HttpPut("prompts/{key}", Name = "updateAiPrompt")]
    public async Task<ActionResult<AiSettingsResponse>> UpdatePrompt(string key, [FromBody] AiPromptRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateAiPromptCommand(key, request.Text, By()), cancellationToken));

    /// <summary>«Перевірити»: 422 when OpenAI refused (key, model, balance), 503 when there is no key or OpenAI is down.</summary>
    [HttpPost("test", Name = "testAiConnection")]
    [ProducesResponseType<AiTestResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AiTestResponse>> Test([FromBody] AiTestRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new TestAiConnectionCommand(request.Tier), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Unavailable => StatusCode(StatusCodes.Status503ServiceUnavailable, new { errors = result.Errors }),
            ResultStatus.Error => UnprocessableEntity(new { errors = result.Errors }),
            _ => this.ToActionResult(result),
        };
    }

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}

public sealed record AiSettingsRequest(
    bool Enabled,
    string FastModel,
    decimal FastInputPrice,
    decimal FastOutputPrice,
    string StrongModel,
    decimal StrongInputPrice,
    decimal StrongOutputPrice,
    decimal MonthlyLimitUsd,
    int Version);

/// <summary>Write-only.</summary>
public sealed record AiKeyRequest(string? ApiKey);

public sealed record AiTestRequest(AiModelTier Tier);

public sealed record AiPromptRequest(string? Text);
