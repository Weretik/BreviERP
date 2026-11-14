using Reference.Application.Features.PpeMarkupRule.Commands.CreatePpeMarkupRule;
using Reference.Application.Features.PpeMarkupRule.Commands.DeletePpeMarkupRule;
using Reference.Application.Features.PpeMarkupRule.Commands.UpdatePpeMarkupRule;
using Reference.Application.Features.PpeMarkupRule.Queries.GetPpeMarkupRules;

namespace Reference.Api.PpeMarkupRule;

/// <summary>PPE markup rules of the Calculator sheet «Налаштування цін» (right table).</summary>
[ApiController]
[Route("api/reference/ppe-markup-rules")]
public sealed class PpeMarkupRulesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(List<PpeMarkupRuleRowDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PpeMarkupRuleRowDTO>>> Get(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPpeMarkupRulesQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = "access:pricing-parameters")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreatePpeMarkupRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePpeMarkupRuleCommand(new CreatePpeMarkupRuleCommandRequest(
            request.Name, request.RetailMarkup, request.WholesaleMarkup, request.LargeWholesaleMarkup,
            request.PriceNote, request.AppliesTo, request.SortOrder));

        return this.ToActionResult(await sender.Send(command, cancellationToken));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "access:pricing-parameters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdatePpeMarkupRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePpeMarkupRuleCommand(id, new UpdatePpeMarkupRuleCommandRequest(
            request.RetailMarkup, request.WholesaleMarkup, request.LargeWholesaleMarkup, request.PriceNote,
            request.AppliesTo, request.SortOrder));

        return this.ToActionResult(await sender.Send(command, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "access:pricing-parameters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeletePpeMarkupRuleCommand(id), cancellationToken));
}
