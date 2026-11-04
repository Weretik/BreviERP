namespace Production.Domain.Orders;

/// <summary>An «нанесення» of an item: what, where, the text or logo, and its colour.</summary>
public sealed record ItemApplication(ItemApplicationKind Kind, string? Place, string? Description, string? Color)
{
    public const int TextMaxLength = 300;

    public static ItemApplication Create(ItemApplicationKind kind, string? place, string? description, string? color) =>
        new(kind, OrderText.Optional(place, TextMaxLength), OrderText.Optional(description, TextMaxLength),
            OrderText.Optional(color, TextMaxLength));
}
