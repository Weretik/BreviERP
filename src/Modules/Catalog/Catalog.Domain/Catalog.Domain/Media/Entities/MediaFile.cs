using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Media.Enums;
using Catalog.Domain.Media.Errors;
using Catalog.Domain.Media.ValueObjects;

namespace Catalog.Domain.Media.Entities;

public sealed class MediaFile : BaseEntity<MediaFileId>, IAggregateRoot
{
    private const int FileNameMaxLength = 255;
    private const int ContentTypeMaxLength = 100;
    private const int StorageProviderMaxLength = 100;
    private const int BucketNameMaxLength = 200;
    private const int StorageKeyMaxLength = 500;
    private const int PublicUrlMaxLength = 1000;

    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeInBytes { get; private set; }
    public string StorageProvider { get; private set; } = null!;
    public string BucketName { get; private set; } = null!;
    public string StorageKey { get; private set; } = null!;
    public string? PublicUrl { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public MediaFileStatus Status { get; private set; }

    private MediaFile() { }

    private MediaFile(
        MediaFileId id,
        string fileName,
        string contentType,
        long sizeInBytes,
        string storageProvider,
        string bucketName,
        string storageKey)
    {
        SetId(id);
        SetFileName(fileName);
        SetContentType(contentType);
        SetSizeInBytes(sizeInBytes);
        SetStorageProvider(storageProvider);
        SetBucketName(bucketName);
        SetStorageKey(storageKey);
        Status = MediaFileStatus.PendingUpload;
    }

    public static MediaFile CreatePending(
        MediaFileId id,
        string fileName,
        string contentType,
        long sizeInBytes,
        string storageProvider,
        string bucketName,
        string storageKey)
        => new(id, fileName, contentType, sizeInBytes, storageProvider, bucketName, storageKey);

    public void MarkUploaded(string publicUrl, int? width, int? height)
    {
        if (Status != MediaFileStatus.PendingUpload)
            throw new DomainException(MediaFileErrors.UploadCompletionInvalidState());

        SetPublicUrl(publicUrl);
        SetDimensions(width, height);
        Status = MediaFileStatus.Ready;
    }

    public bool IsReadyForProductUsage() => Status == MediaFileStatus.Ready;

    private void SetId(MediaFileId id)
    {
        if (id.Value == default)
            throw new DomainException(MediaFileErrors.IdIsRequired());

        Id = id;
    }

    private void SetFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException(MediaFileErrors.FileNameIsRequired());

        var normalizedFileName = fileName.Trim();
        if (normalizedFileName.Length > FileNameMaxLength)
            throw new DomainException(MediaFileErrors.FileNameLengthInvalid(FileNameMaxLength));

        FileName = normalizedFileName;
    }

    private void SetContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException(MediaFileErrors.ContentTypeIsRequired());

        var normalizedContentType = contentType.Trim();
        if (normalizedContentType.Length > ContentTypeMaxLength)
            throw new DomainException(MediaFileErrors.ContentTypeLengthInvalid(ContentTypeMaxLength));

        ContentType = normalizedContentType;
    }

    private void SetSizeInBytes(long sizeInBytes)
    {
        if (sizeInBytes <= 0)
            throw new DomainException(MediaFileErrors.SizeInvalid());

        SizeInBytes = sizeInBytes;
    }

    private void SetStorageProvider(string storageProvider)
    {
        if (string.IsNullOrWhiteSpace(storageProvider))
            throw new DomainException(MediaFileErrors.StorageProviderIsRequired());

        var normalizedStorageProvider = storageProvider.Trim();
        if (normalizedStorageProvider.Length > StorageProviderMaxLength)
            throw new DomainException(MediaFileErrors.StorageProviderLengthInvalid(StorageProviderMaxLength));

        StorageProvider = normalizedStorageProvider;
    }

    private void SetBucketName(string bucketName)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
            throw new DomainException(MediaFileErrors.BucketNameIsRequired());

        var normalizedBucketName = bucketName.Trim();
        if (normalizedBucketName.Length > BucketNameMaxLength)
            throw new DomainException(MediaFileErrors.BucketNameLengthInvalid(BucketNameMaxLength));

        BucketName = normalizedBucketName;
    }

    private void SetStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new DomainException(MediaFileErrors.StorageKeyIsRequired());

        var normalizedStorageKey = storageKey.Trim();
        if (normalizedStorageKey.Length > StorageKeyMaxLength)
            throw new DomainException(MediaFileErrors.StorageKeyLengthInvalid(StorageKeyMaxLength));

        StorageKey = normalizedStorageKey;
    }

    private void SetPublicUrl(string publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
            throw new DomainException(MediaFileErrors.PublicUrlIsRequired());

        var normalizedPublicUrl = publicUrl.Trim();
        if (normalizedPublicUrl.Length > PublicUrlMaxLength)
            throw new DomainException(MediaFileErrors.PublicUrlLengthInvalid(PublicUrlMaxLength));

        PublicUrl = normalizedPublicUrl;
    }

    private void SetDimensions(int? width, int? height)
    {
        if (width.HasValue && width.Value <= 0)
            throw new DomainException(MediaFileErrors.WidthInvalid());

        if (height.HasValue && height.Value <= 0)
            throw new DomainException(MediaFileErrors.HeightInvalid());

        Width = width;
        Height = height;
    }
}
