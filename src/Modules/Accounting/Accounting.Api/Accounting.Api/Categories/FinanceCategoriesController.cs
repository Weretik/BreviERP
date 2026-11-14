using Accounting.Api.Access;
using Accounting.Api.Results;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Accounting.Application.Features.Categories.Commands.CreateFinanceCategory;
using Accounting.Application.Features.Categories.Commands.DeleteFinanceCategory;
using Accounting.Application.Features.Categories.Commands.UpdateFinanceCategory;
using Accounting.Application.Features.Categories.Queries.GetFinanceCategories;
using Accounting.Application.Features.Categories.Shared;
using Accounting.Application.Features.History.Queries.GetFinanceHistory;

namespace Accounting.Api.Categories;

/// <summary>«Бухгалтерія → Категорії»: two levels per area of the books and direction.</summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/categories")]
public sealed class FinanceCategoriesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFinanceCategories")]
    public async Task<ActionResult<IReadOnlyList<FinanceCategoryResponse>>> List([FromQuery] LedgerScope? scope,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetFinanceCategoriesQuery(scope), cancellationToken));

    /// <summary>409 when the same level already has this name.</summary>
    [HttpPost(Name = "createFinanceCategory")]
    public async Task<ActionResult<FinanceCategoryResponse>> Create([FromBody] FinanceCategoryInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new CreateFinanceCategoryCommand(input.Name, input.Direction, input.Scope,
            input.ParentId, input.Note, input.SortOrder, AccountingAccess.By(User)), cancellationToken));

    [HttpPut("{id:guid}", Name = "updateFinanceCategory")]
    public async Task<ActionResult<FinanceCategoryResponse>> Update(Guid id, [FromBody] FinanceCategoryUpdateInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateFinanceCategoryCommand(id, input.Name, input.Note, input.SortOrder,
            input.Version, AccountingAccess.By(User)), cancellationToken));

    /// <summary>A root is deleted with its subcategories.</summary>
    [HttpDelete("{id:guid}", Name = "deleteFinanceCategory")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, [FromQuery] int version, CancellationToken cancellationToken) =>
        this.ToNoContentResult(await sender.Send(new DeleteFinanceCategoryCommand(id, version, AccountingAccess.By(User)),
            cancellationToken));

    [HttpGet("{id:guid}/history", Name = "getFinanceCategoryHistory")]
    public async Task<ActionResult<IReadOnlyList<FinanceHistoryEntryResponse>>> History(Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetFinanceHistoryQuery(AuditEntity.Category, id), cancellationToken));
}
