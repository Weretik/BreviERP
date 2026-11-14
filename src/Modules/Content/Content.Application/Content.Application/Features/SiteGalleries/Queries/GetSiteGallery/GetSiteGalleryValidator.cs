using Content.Application.Abstractions;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Application.Features.SiteGalleries.Queries.GetSiteGallery;

public sealed class GetSiteGalleryValidator : AbstractValidator<GetSiteGalleryQuery>
{
    public GetSiteGalleryValidator() => RuleFor(x => x.Key).NotEmpty().MaximumLength(SiteGallery.KeyMaxLength);
}
