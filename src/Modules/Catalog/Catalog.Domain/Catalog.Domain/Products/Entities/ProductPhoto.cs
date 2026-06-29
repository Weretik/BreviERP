using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public class ProductPhoto : BaseEntity<ProductPhotoId>
{
    private const int AltMaxLength = 300;

    #region Properties
    public MediaFileId MediaFileId { get; private set; }
    public string? Alt { get; private set; }
    public bool IsVisible { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsMain { get; private set; }
    #endregion

    #region Constructors
    private ProductPhoto() { }

    private ProductPhoto(
        ProductPhotoId id,
        MediaFileId mediaFileId,
        string? alt,
        bool isVisible,
        int sortOrder,
        bool isMain)
    {
        SetId(id);
        SetMediaFileId(mediaFileId);
        SetAlt(alt);
        IsVisible = isVisible;
        SetSortOrder(sortOrder);
        IsMain = isMain;
    }
    #endregion

    #region Factories
    public static ProductPhoto Create(
        ProductPhotoId id,
        MediaFileId mediaFileId,
        string? alt = null,
        bool isVisible = true,
        int sortOrder = 0,
        bool isMain = false)
        => new(id, mediaFileId, alt, isVisible, sortOrder, isMain);

    public void ReplaceMediaFile(MediaFileId mediaFileId)
    {
        SetMediaFileId(mediaFileId);
    }

    public void SetAltText(string? alt)
    {
        SetAlt(alt);
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
    }

    public void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new DomainException(ProductErrors.PhotoSortOrderInvalid());

        SortOrder = sortOrder;
    }

    public void SetMain(bool isMain)
    {
        IsMain = isMain;
    }
    #endregion

    #region Setters/Validation
    private void SetId(ProductPhotoId id)
    {
        if (id.Value == default)
            throw new DomainException(ProductErrors.PhotoIdIsRequired());

        Id = id;
    }

    private void SetMediaFileId(MediaFileId mediaFileId)
    {
        if (mediaFileId.Value == default)
            throw new DomainException(ProductErrors.PhotoMediaFileIdIsRequired());

        MediaFileId = mediaFileId;
    }

    private void SetAlt(string? alt)
    {
        if (string.IsNullOrWhiteSpace(alt))
        {
            Alt = null;
            return;
        }

        var normalizedAlt = alt.Trim();
        if (normalizedAlt.Length > AltMaxLength)
            throw new DomainException(ProductErrors.PhotoAltLengthInvalid(AltMaxLength));

        Alt = normalizedAlt;
    }
    #endregion
}
