using BuildingBlocks.Domain.Errors;

namespace Content.Domain.Redirects;

/// <summary>Business rule errors of site redirects.</summary>
public static class SiteRedirectErrors
{
    public static Error InvalidFromPath() =>
        Error.Validation("content.redirect_invalid_from", "Стара адреса: шлях сторінки, наприклад /specodyag-litnij, без пробілів, до 500 символів.", "FromPath");

    public static Error InvalidToPath() =>
        Error.Validation("content.redirect_invalid_to", "Нова адреса: сторінка нового сайту, що починається з /uk або /ru.", "ToPath");

    public static Error ToItself() =>
        Error.Validation("content.redirect_to_itself", "Адреса не може вести сама на себе.", "ToPath");
}
