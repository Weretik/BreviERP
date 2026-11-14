using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;
using Crm.Application.Features.Embroidery.Commands.SaveEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Commands.SaveEmbroiderySettings;
using Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Commands.ImportEmbroideryArchive;

/// <summary>
/// «Імпорт архіву» (Director and Admin): moves the old calculator's saved calculations into «Збережені» with their
/// own dates, authors and frozen rates. Each record is priced with its old rates and must give the old saved prices to
/// the kopeck; nothing is priced with today's rates. A record already moved stays as it is; a list with any error
/// saves nothing. With <c>DryRun</c> only shows what would happen.
/// </summary>
public sealed record ImportEmbroideryArchiveCommand(EmbroideryActor Actor, IReadOnlyList<EmbroideryArchiveRow> Rows,
    bool DryRun) : ICommand<Result<EmbroideryArchiveImportResult>>;
