using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public partial class Product
{
    /// <summary>
    /// Makes the product photos exactly <paramref name="mediaFileIds"/> in this order (CAT-016 tab «Фото»). Photos that
    /// stay keep their id, alt text and visibility; new ones get the next ids. <paramref name="mainMediaFileId"/> becomes
    /// the main photo; when it is null the current main photo stays, or the first one becomes main.
    /// </summary>
    public void SetPhotoList(IReadOnlyList<MediaFileId> mediaFileIds, MediaFileId? mainMediaFileId, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(mediaFileIds);
        var duplicate = mediaFileIds.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null)
            throw new DomainException(ProductErrors.PhotoMediaFileAlreadyAttached(duplicate.Key.Value));
        if (mainMediaFileId is { } main && !mediaFileIds.Contains(main))
            throw new DomainException(ProductErrors.PhotoMediaFileIdIsRequired());

        var currentMain = _photos.FirstOrDefault(x => x.IsMain)?.MediaFileId;
        _photos.RemoveAll(x => !mediaFileIds.Contains(x.MediaFileId));
        var nextId = _photos.Count == 0 ? 1 : _photos.Max(x => x.Id.Value) + 1;
        for (var index = 0; index < mediaFileIds.Count; index++)
        {
            var fileId = mediaFileIds[index];
            var photo = _photos.FirstOrDefault(x => x.MediaFileId == fileId);
            if (photo is null)
            {
                photo = ProductPhoto.Create(Id, ProductPhotoId.Create(nextId++), fileId, null, true, index, false);
                _photos.Add(photo);
            }
            else photo.SetSortOrder(index);
        }

        var wanted = mainMediaFileId ?? (currentMain is { } kept && mediaFileIds.Contains(kept) ? kept : null);
        ClearMainPhoto();
        if (wanted is { } target) _photos.First(x => x.MediaFileId == target).SetMain(true);
        else EnsureMainPhotoSelected();
        MarkAsUpdated(updatedAt);
    }
}
