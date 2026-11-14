namespace Content.Application.Features.SiteCompany.Shared.Public;

/// <param name="E164">The <c>tel:</c> value, <c>+380504698675</c>.</param>
/// <param name="Display">As the site shows it, <c>050 469 86 75</c>.</param>
public sealed record PublicPhone(string E164, string Display);
