using Content.Domain.ProductPages;

namespace Content.Application.Features.ProductPages.Commands.SetContentProductVisibility;

public sealed record ContentProductVisibilityResponse(int ProductId, bool ShowOnSite, bool OnSite, int Revision, string Slug);
