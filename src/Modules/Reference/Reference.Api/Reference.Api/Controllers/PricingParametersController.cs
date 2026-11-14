using Reference.Api.Contracts.PricingParameters;
using Reference.Application.Features.PricingParameter.Commands.CreatePricingParameter;
using Reference.Application.Features.PricingParameter.Commands.DeletePricingParameter;
using Reference.Application.Features.PricingParameter.Commands.UpdatePricingParameter;
using Reference.Application.Features.PricingParameter.Queries.GetPricingParameters;
using Reference.Application.Features.PricingSettingsImport.Commands.ImportPricingSettings;
using Reference.Application.Features.PricingSettingsImport.Shared;

namespace Reference.Api.Controllers;

/// <summary>Pricing parameters of the Calculator sheet «Налаштування цін» (left table).</summary>
[ApiController]
[Route("api/reference/pricing-parameters")]
public sealed class PricingParametersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(List<PricingParameterRowDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PricingParameterRowDTO>>> Get(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPricingParametersQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = "access:pricing-parameters")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreatePricingParameterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePricingParameterCommand(new CreatePricingParameterCommandRequest(
            request.Code, request.Group, request.Name, request.Value, request.Unit, request.IsCalculated,
            request.Comment, request.SortOrder));

        return this.ToActionResult(await sender.Send(command, cancellationToken));
    }

    /// <summary>
    /// Imports both tables of «Налаштування цін» read from the sheet: dryRun returns the preview; otherwise the
    /// rows are saved when none has an error.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "access:pricing-parameters")]
    [ProducesResponseType(typeof(PricingSettingsImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PricingSettingsImportResult>> Import(
        [FromBody] ImportPricingSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ImportPricingSettingsCommand(request.Parameters, request.PpeMarkupRules, request.DryRun,
            request.DeleteMissing);

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
        [FromBody] UpdatePricingParameterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePricingParameterCommand(id, new UpdatePricingParameterCommandRequest(
            request.Group, request.Name, request.Value, request.Unit, request.IsCalculated, request.Comment,
            request.SortOrder));

        return this.ToActionResult(await sender.Send(command, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "access:pricing-parameters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeletePricingParameterCommand(id), cancellationToken));
}
