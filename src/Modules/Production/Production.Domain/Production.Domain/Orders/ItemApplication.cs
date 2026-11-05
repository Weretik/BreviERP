namespace Production.Domain.Orders;

/// <summary>
/// An «нанесення» of an item: what, where, the text or logo, and its colour. An embroidery may point to a saved
/// «Ціна вишивки» calculation; its number is kept, so the order still says «ВШ-0042» if the calculation is removed.
/// </summary>
public sealed record ItemApplication(ItemApplicationKind Kind, string? Place, string? Description, string? Color,
    long? EmbroideryCalculationId = null, string? EmbroideryNumber = null)
{
    public const int TextMaxLength = 300;
    public const int NumberMaxLength = 20;

    /// <remarks>A print never keeps a calculation.</remarks>
    public static ItemApplication Create(ItemApplicationKind kind, string? place, string? description, string? color,
        long? embroideryCalculationId = null, string? embroideryNumber = null)
    {
        var linked = kind == ItemApplicationKind.Embroidery && embroideryCalculationId is > 0;
        return new(kind, OrderText.Optional(place, TextMaxLength), OrderText.Optional(description, TextMaxLength),
            OrderText.Optional(color, TextMaxLength), linked ? embroideryCalculationId : null,
            linked ? OrderText.Optional(embroideryNumber, NumberMaxLength) : null);
    }
}
