using Production.Application.Abstractions;
using Production.Domain.Archive;
using Production.Application.Features.Archive.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Archive.Queries.GetWorkshopArchive.Results;

namespace Production.Application.Features.Archive.Queries.GetWorkshopArchive;

/// <summary>
/// «Архів цеху»: lines of one old book, newest first, found by any word and limited by date. Everyone with production
/// orders reads it (PRD-D54); there are no prices or rates in it.
/// </summary>
public sealed record GetWorkshopArchiveQuery(
    ProductionActor Actor, string Kind, string? Search, DateOnly? From, DateOnly? To, int Page, int PageSize)
    : IQuery<Result<WorkshopArchivePageResponse>>;
