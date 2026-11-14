using BuildingBlocks.Domain.Errors;

namespace Production.Domain.Orders;

/// <summary>Business rule errors of production orders, their items and shipments.</summary>
public static class ProductionOrderErrors
{
    public static Error ItemsCount() =>
        Error.Validation("production.order_items_count",
            $"У завданні має бути від 1 до {ProductionOrder.MaxItems} позицій.", "items");

    public static Error QuantityOutOfRange() =>
        Error.Validation("production.item_quantity",
            $"Кількість — від 1 до {ProductionItem.MaxQuantity}.", "quantity");

    public static Error TooManyColors() =>
        Error.Validation("production.item_colors",
            $"Додаткових кольорів — не більше {ProductionItem.MaxAdditionalColors}.", "additionalColors");

    public static Error TooManyApplications() =>
        Error.Validation("production.item_applications",
            $"Нанесень — не більше {ProductionItem.MaxApplications}.", "applications");

    public static Error ItemNotInOrder() =>
        Error.NotFound("production.item_not_in_order", "Позиції немає в цьому завданні.");

    public static Error ShipmentNotFound() =>
        Error.NotFound("production.shipment_not_found", "Відвантаження не знайдено.");

    public static Error TooMuchShipped() =>
        Error.Conflict("production.too_much_shipped", "Кількість більша за залишок до відвантаження. Оновіть сторінку.");

    public static Error NotFullyShipped() =>
        Error.Conflict("production.not_fully_shipped", "Закрити можна, коли все відвантажено.");

    public static Error NotNew() =>
        Error.Conflict("production.order_not_new", "Це можна зробити лише з новим завданням.");

    public static Error AlreadyOpen() =>
        Error.Conflict("production.order_already_open", "Завдання вже в роботі.");
}
