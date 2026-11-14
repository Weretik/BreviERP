namespace Catalog.Application.Abstractions;

public interface IProductSlugGenerator
{
    string GenerateFromUkrainianName(string name);
}
