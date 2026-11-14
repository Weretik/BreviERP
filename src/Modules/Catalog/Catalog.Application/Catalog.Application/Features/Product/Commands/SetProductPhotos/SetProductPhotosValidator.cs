namespace Catalog.Application.Features.Product.Commands.SetProductPhotos;

public sealed class SetProductPhotosValidator : AbstractValidator<SetProductPhotosCommand>
{
    public SetProductPhotosValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.MediaFileIds).NotNull()
            .Must(x => x.Count <= SetProductPhotosCommand.MaxPhotos)
            .WithMessage($"Не більше {SetProductPhotosCommand.MaxPhotos} фото в товарі.")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("Фото повторюється.");
        RuleForEach(x => x.MediaFileIds).GreaterThan(0);
        RuleFor(x => x.MainMediaFileId)
            .Must((command, main) => command.MediaFileIds.Contains(main!.Value))
            .WithMessage("Головне фото має бути серед фото товару.")
            .When(x => x.MainMediaFileId is not null && x.MediaFileIds is not null);
    }
}
