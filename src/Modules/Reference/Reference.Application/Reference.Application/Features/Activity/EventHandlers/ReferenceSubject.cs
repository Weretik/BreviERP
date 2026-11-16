using FabricColorItem = Reference.Domain.FabricColors.FabricColor;
using FabricItem = Reference.Domain.Fabrics.Fabric;
using GarmentAccessoryItem = Reference.Domain.GarmentAccessories.GarmentAccessory;
using GarmentPartOperationItem = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using GarmentPartItem = Reference.Domain.GarmentParts.GarmentPart;
using PpeMarkupRuleItem = Reference.Domain.PpeMarkupRules.PpeMarkupRule;
using PricingParameterItem = Reference.Domain.PricingParameters.PricingParameter;
using ProductSectionItem = Reference.Domain.ProductSections.ProductSection;
using SupplierItem = Reference.Domain.Suppliers.Supplier;

namespace Reference.Application.Features.Activity.EventHandlers;

/// <summary>How a «Довідники» entry is named in «Журнал дій»: its type key and title, id, name and price.</summary>
public sealed record ReferenceSubject(string Key, string Title, string Id, string Name, decimal? Price)
{
    public static ReferenceSubject? Of(object item) => item switch
    {
        FabricItem x => new("fabric", "Тканина", Text(x.Id.Value), Coded(x.Code, x.Name), x.Price.Value),
        GarmentAccessoryItem x => new("accessory", "Фурнітура", Text(x.Id.Value), x.Name, x.Price.Value),
        FabricColorItem x => new("color", "Колір", Text(x.Id.Value), x.Name, null),
        GarmentPartItem x => new("garment-part", "Деталь виробу", Text(x.Id.Value), x.Name, null),
        GarmentPartOperationItem x => new("operation", "Операція", Text(x.Id.Value), x.Name, null),
        PpeMarkupRuleItem x => new("ppe-markup-rule", "Правило націнки ЗІЗ", Text(x.Id.Value), x.Name, null),
        PricingParameterItem x => new("pricing-parameter", "Параметр ціни", Text(x.Id.Value), Coded(x.Code, x.Name), null),
        ProductSectionItem x => new("product-section", "Розділ товарів", Text(x.Id.Value), x.Name, null),
        SupplierItem x => new("supplier", "Постачальник", Text(x.Id.Value), x.Name, null),
        _ => null,
    };

    private static string Text(int id) => id.ToString(CultureInfo.InvariantCulture);

    private static string Coded(string? code, string name) => string.IsNullOrWhiteSpace(code) ? name : $"{code} · {name}";
}
