namespace Catalog.Application.Abstractions;

/// <summary>Reads the composition of every sewing model at once, for the workshop «Комплектація моделей».</summary>
public interface ISewingCompositionListReader
{
    Task<IReadOnlyList<SewingCompositionListRow>> ReadAllAsync(CancellationToken cancellationToken);
}
