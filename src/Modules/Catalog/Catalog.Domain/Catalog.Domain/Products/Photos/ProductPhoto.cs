using BuildingBlocks.Domain.Entity;
using Catalog.Domain.Media;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Photos;

public class ProductPhoto : BaseEntity<ProductPhotoId>
{
    private const int AltMaxLength = 300;

    #region Properties
    public MediaFileId MediaFileId { get; private set; }
    public ProductId ProductId { get; private set; }
    public string? Alt { get; private set; }
    public bool IsVisible { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsMain { get; private set; }
    #endregion

    #region Constructors
    private ProductPhoto() { }

    #endregion

    #region Factories

    public static DomainResult<ProductPhoto> Create(
        ProductId productId,
        ProductPhotoId id,
        MediaFileId mediaFileId,
        string? alt = null,
        bool isVisible = true,
        int sortOrder = 0,
        bool isMain = false)
    {
        if (productId.Value == default) throw new ArgumentException("A photo needs its product id.", nameof(productId));
        if (id.Value == default) throw new ArgumentException("A photo needs an id.", nameof(id));
        var cleanAlt = CleanAlt(alt);
        if (cleanAlt.IsFailure) return cleanAlt.Error!;
        if (sortOrder < 0) return ProductErrors.PhotoSortOrderInvalid();
        var photo = new ProductPhoto
        {
            ProductId = productId,
            Id = id,
            Alt = cleanAlt.Value,
            IsVisible = isVisible,
            SortOrder = sortOrder,
            IsMain = isMain,
        };
        photo.ReplaceMediaFile(mediaFileId);
        return photo;
    }

    public void ReplaceMediaFile(MediaFileId mediaFileId)
    {
        if (mediaFileId.Value == default) throw new ArgumentException("A photo needs a media file.", nameof(mediaFileId));
        MediaFileId = mediaFileId;
    }

    public DomainResult SetAltText(string? alt)
    {
        var cleanAlt = CleanAlt(alt);
        if (cleanAlt.IsFailure) return cleanAlt.Error!;
        Alt = cleanAlt.Value;
        return DomainResult.Success();
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
    }

    public DomainResult SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0) return ProductErrors.PhotoSortOrderInvalid();
        SortOrder = sortOrder;
        return DomainResult.Success();
    }

    public void SetMain(bool isMain)
    {
        IsMain = isMain;
    }

    #endregion

    #region Validation

    private static DomainResult<string?> CleanAlt(string? alt)
    {
        if (string.IsNullOrWhiteSpace(alt)) return DomainResult<string?>.Success(null);
        var normalizedAlt = alt.Trim();
        if (normalizedAlt.Length > AltMaxLength) return ProductErrors.PhotoAltLengthInvalid(AltMaxLength);
        return normalizedAlt;
    }

    #endregion
}
