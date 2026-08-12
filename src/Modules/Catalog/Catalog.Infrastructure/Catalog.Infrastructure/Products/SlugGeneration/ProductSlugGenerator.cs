using Catalog.Application.Contracts.SlugGeneration;
using Slugify;
using TranslitKit;

namespace Catalog.Infrastructure.Products.SlugGeneration;

public sealed class ProductSlugGenerator : IProductSlugGenerator
{
    private static readonly SlugHelper SlugHelper = new();
    private static readonly UkrainianKMU UkrainianTransliteration = new();

    public string GenerateFromUkrainianName(string name)
        => SlugHelper.GenerateSlug(Translit.Convert(name, UkrainianTransliteration, preserveCase: false));
}
