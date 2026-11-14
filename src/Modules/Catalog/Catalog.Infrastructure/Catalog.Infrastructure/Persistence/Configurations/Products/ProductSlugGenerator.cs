using Slugify;
using TranslitKit;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Products;


namespace Catalog.Infrastructure.Persistence.Configurations.Products;

public sealed class ProductSlugGenerator : IProductSlugGenerator
{
    private static readonly SlugHelper SlugHelper = new();
    private static readonly UkrainianKMU UkrainianTransliteration = new();

    public string GenerateFromUkrainianName(string name)
        => SlugHelper.GenerateSlug(Translit.Convert(name, UkrainianTransliteration, preserveCase: false));
}
