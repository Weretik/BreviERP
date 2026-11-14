using Slugify;
using TranslitKit;
using Catalog.Application.Abstractions;

namespace Catalog.Infrastructure.Products.SlugGeneration;

public sealed class ProductSlugGenerator : IProductSlugGenerator
{
    private static readonly SlugHelper SlugHelper = new();
    private static readonly UkrainianKMU UkrainianTransliteration = new();

    public string GenerateFromUkrainianName(string name)
        => SlugHelper.GenerateSlug(Translit.Convert(name, UkrainianTransliteration, preserveCase: false));
}
