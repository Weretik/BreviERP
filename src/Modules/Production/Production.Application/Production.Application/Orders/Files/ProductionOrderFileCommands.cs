using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Orders;

namespace Production.Application.Orders.Files;

/// <summary>
/// Adds a file to the order or one of its items: sizes, application layout, an extra file or a photo. It goes to the
/// CDN folder of the order.
/// </summary>
public sealed record UploadProductionOrderFileCommand(
    ProductionActor Actor, int OrderId, int? ItemId, string Kind, Stream Content, string FileName, string ContentType,
    long SizeInBytes) : ICommand<Result<ProductionOrderResponse>>
{
    /// <summary>The file name without any folder path a browser may send.</summary>
    public string DisplayName => FileName is null ? string.Empty : FileName[(FileName.LastIndexOfAny(['/', '\\']) + 1)..];
}

public sealed class UploadProductionOrderFileCommandValidator : AbstractValidator<UploadProductionOrderFileCommand>
{
    public UploadProductionOrderFileCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0).When(x => x.ItemId is not null);
        RuleFor(x => x.Kind).Must(x => Enum.TryParse<ProductionFileKind>(x, true, out var kind) && Enum.IsDefined(kind))
            .WithMessage("Невідомий вид файлу.");
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Назва файлу обов'язкова.")
            .MaximumLength(ProductionOrderFile.NameMaxLength);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(ProductionOrderFile.ContentTypeMaxLength);
        RuleFor(x => x.SizeInBytes).GreaterThan(0).WithMessage(OrderMessages.FileEmpty)
            .LessThanOrEqualTo(ProductionOrderFile.MaxBytes).WithMessage(OrderMessages.FileTooLarge);
    }
}

public sealed class UploadProductionOrderFileCommandHandler(
    IProductionDbContext db, IProductionStaffReader staff, IProductionFileStorage storage, OrderCards cards,
    TimeProvider clock) : ICommandHandler<UploadProductionOrderFileCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(UploadProductionOrderFileCommand command,
        CancellationToken cancellationToken)
    {
        var order = await cards.Orders.SingleOrDefaultAsync(x => x.Id == command.OrderId, cancellationToken);
        if (order is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound);
        if (!command.Actor.CanEdit(order)) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.CannotEdit);
        if (command.ItemId is { } itemId && order.Items.All(x => x.Id != itemId))
            return Result<ProductionOrderResponse>.NotFound(OrderMessages.ItemNotInOrder);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var name = command.DisplayName.Trim();
        var key = $"/production/orders/{order.Number}/{Guid.NewGuid():N}{SafeExtension(name)}";
        var url = await storage.UploadAsync(command.Content, key, command.ContentType, cancellationToken);
        var now = clock.GetUtcNow();
        var file = new ProductionOrderFile(order.Id, command.ItemId,
            Enum.Parse<ProductionFileKind>(command.Kind, ignoreCase: true), name, command.ContentType,
            command.SizeInBytes, url, key, actorName, now);
        db.OrderFiles.Add(file);
        order.RecordAbout(command.ItemId, "Файл додано", null, name, actorName, now);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // The CDN already has the file; remove it so it does not stay without a row.
            try { await storage.DeleteAsync(key, CancellationToken.None); } catch { /* best effort */ }
            throw;
        }

        return Result.Success(await cards.BuildAsync(order, command.Actor, cancellationToken));
    }

    /// <summary>A short Latin extension, so the CDN key stays safe whatever the file is called.</summary>
    private static string SafeExtension(string name)
    {
        var extension = Path.GetExtension(name).ToLowerInvariant();
        return extension.Length is > 1 and <= 8 && extension.Skip(1).All(char.IsAsciiLetterOrDigit) ? extension : "";
    }
}

/// <summary>Removes a file from the order; a CDN file is deleted too.</summary>
public sealed record DeleteProductionOrderFileCommand(ProductionActor Actor, int OrderId, int FileId)
    : ICommand<Result<ProductionOrderResponse>>;

public sealed class DeleteProductionOrderFileCommandValidator : AbstractValidator<DeleteProductionOrderFileCommand>
{
    public DeleteProductionOrderFileCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.FileId).GreaterThan(0);
    }
}

public sealed class DeleteProductionOrderFileCommandHandler(
    IProductionDbContext db, IProductionStaffReader staff, IProductionFileStorage storage, OrderCards cards,
    TimeProvider clock, ILogger<DeleteProductionOrderFileCommandHandler> logger)
    : ICommandHandler<DeleteProductionOrderFileCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(DeleteProductionOrderFileCommand command,
        CancellationToken cancellationToken)
    {
        var order = await cards.Orders.SingleOrDefaultAsync(x => x.Id == command.OrderId, cancellationToken);
        if (order is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound);
        if (!command.Actor.CanEdit(order)) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.CannotEdit);
        var file = await db.OrderFiles.SingleOrDefaultAsync(
            x => x.Id == command.FileId && x.OrderId == order.Id && x.DeletedAt == null, cancellationToken);
        if (file is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.FileNotFound);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        file.Delete(now);
        order.RecordAbout(file.ItemId, "Файл видалено", file.Name, null, actorName, now);
        await db.SaveChangesAsync(cancellationToken);
        if (file.StorageKey is { } key)
        {
            try
            {
                await storage.DeleteAsync(key, CancellationToken.None);
            }
            catch (Exception error)
            {
                // The file is already gone from the order; a CDN leftover is harmless.
                logger.LogWarning(error, "CDN file {Key} of production order {Number} was not deleted", key, order.Number);
            }
        }
        return Result.Success(await cards.BuildAsync(order, command.Actor, cancellationToken));
    }
}
