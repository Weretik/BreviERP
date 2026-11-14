using Content.Application.Abstractions;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;

public sealed class TransferSiteGalleryValidator : AbstractValidator<TransferSiteGalleryCommand>
{
    public TransferSiteGalleryValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(SiteGallery.KeyMaxLength);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}
