using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.ImportProductionProgress;

public sealed class ImportProductionProgressValidator : AbstractValidator<ImportProductionProgressCommand>
{
    public ImportProductionProgressValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Rows).NotNull().Must(x => x.Count is > 0 and <= 1000)
            .WithMessage("У файлі має бути від 1 до 1000 завдань.");
        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(ProductionOrder.LegacyIdMaxLength);
            row.RuleFor(x => x.Items).NotNull();
            row.RuleFor(x => x.Files).NotNull();
            row.RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.Position).GreaterThan(0);
                item.RuleFor(x => x.Stages).NotNull();
                item.RuleForEach(x => x.Stages)
                    .Must(x => Enum.TryParse<ProductionStage>(x, true, out var stage) && Enum.IsDefined(stage))
                    .WithMessage("Невідомий етап.");
            }).When(x => x.Items is not null);
            row.RuleForEach(x => x.Files).ChildRules(file =>
            {
                file.RuleFor(x => x.Position).GreaterThan(0).When(x => x.Position is not null);
                file.RuleFor(x => x.Kind)
                    .Must(x => Enum.TryParse<ProductionFileKind>(x, true, out var kind) && Enum.IsDefined(kind))
                    .WithMessage("Невідомий вид файлу.");
                file.RuleFor(x => x.Name).NotEmpty().MaximumLength(ProductionOrderFile.NameMaxLength);
                file.RuleFor(x => x.ContentType).NotEmpty().MaximumLength(ProductionOrderFile.ContentTypeMaxLength);
                file.RuleFor(x => x.Bytes).GreaterThanOrEqualTo(0);
                file.RuleFor(x => x.Url).NotEmpty().MaximumLength(ProductionOrderFile.UrlMaxLength)
                    .Must(x => Uri.TryCreate(x, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
                    .WithMessage("Посилання на файл має бути https.");
                file.RuleFor(x => x.CreatedByName).MaximumLength(ProductionOrder.PersonMaxLength);
            }).When(x => x.Files is not null);
        }).When(x => x.Rows is not null);
    }
}
