namespace Presentation.Shared.States.ExampleAggregate;

public interface ICatalogStore
{
    void SetSearchTerm(string? value);
    void SetCategoryId(int? id);
    void SetSort(string? sort);
    void SetPrice(decimal? min, decimal? max);
    void SetManufacturer(string? name);
    void SetPageSize(int size);

    void GoToPage(int page);
    void Reset();
    void Reload();
}
