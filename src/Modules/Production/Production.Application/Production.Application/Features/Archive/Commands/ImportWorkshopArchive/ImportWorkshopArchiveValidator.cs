using Production.Application.Abstractions;
using Production.Domain.Archive;
using Production.Application.Features.Archive.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Archive.Commands.ImportWorkshopArchive;

public sealed class ImportWorkshopArchiveValidator : AbstractValidator<ImportWorkshopArchiveCommand>
{
    public ImportWorkshopArchiveValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Rows).NotNull().Must(x => x.Count is > 0 and <= ArchiveMessages.MaxImportRows)
            .WithMessage($"У файлі має бути від 1 до {ArchiveMessages.MaxImportRows} рядків.");
    }
}
