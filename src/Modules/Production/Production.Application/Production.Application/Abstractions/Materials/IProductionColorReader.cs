namespace Production.Application.Abstractions.Materials;

/// <summary>Active colours of «Кольори» (Reference) in list order; implemented in the host.</summary>
public interface IProductionColorReader
{
    Task<IReadOnlyList<string>> ListActiveAsync(CancellationToken cancellationToken);
}
