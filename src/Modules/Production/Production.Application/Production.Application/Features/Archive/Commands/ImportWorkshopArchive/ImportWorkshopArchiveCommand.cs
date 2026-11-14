using Production.Application.Abstractions;
using Production.Domain.Archive;
using Production.Application.Features.Archive.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Archive.Commands.ImportWorkshopArchive;

/// <summary>
/// Moves the old «ГРАФИК работы» and the old requests into «Архів цеху» (Admin). <paramref name="DryRun"/> only
/// checks; a line already moved is skipped, so a repeat or a fresh export adds only what is new.
/// </summary>
public sealed record ImportWorkshopArchiveCommand(
    ProductionActor Actor, IReadOnlyList<WorkshopArchiveImportRow> Rows, bool DryRun)
    : ICommand<Result<WorkshopArchiveImportResult>>;
