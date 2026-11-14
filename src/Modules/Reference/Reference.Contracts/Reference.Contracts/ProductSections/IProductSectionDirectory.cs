namespace Reference.Contracts.ProductSections;

/// <summary>«Типи і розділи» (<c>Type</c> is <c>Sewing</c> or <c>Ppe</c>) for other modules.</summary>
public interface IProductSectionDirectory
{
    /// <summary>All rows, switched-off ones included.</summary>
    Task<IReadOnlyList<ProductSectionInfo>> ListAsync(CancellationToken cancellationToken);
}
