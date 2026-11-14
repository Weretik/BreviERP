using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.UploadProductionOrderFile;

public sealed class UploadProductionOrderFileValidator : AbstractValidator<UploadProductionOrderFileCommand>
{
    public UploadProductionOrderFileValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0).When(x => x.ItemId is not null);
        RuleFor(x => x.Kind).Must(x => Enum.TryParse<ProductionFileKind>(x, true, out var kind) && Enum.IsDefined(kind))
            .WithMessage("Невідомий вид файлу.");
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Назва файлу обов'язкова.")
            .MaximumLength(ProductionOrderFile.NameMaxLength);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(ProductionOrderFile.ContentTypeMaxLength);
        RuleFor(x => x.SizeInBytes).GreaterThan(0).WithMessage(OrderMessages.FileEmpty)
            .LessThanOrEqualTo(ProductionOrderFile.MaxBytes).WithMessage(OrderMessages.FileTooLarge);
    }
}
