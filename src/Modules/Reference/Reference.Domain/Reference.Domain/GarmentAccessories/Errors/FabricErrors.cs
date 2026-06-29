using Reference.Domain.Errors;

namespace Reference.Domain.GarmentAccessories.Errors;

public static class FabricErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.Fabric.Id.Required", "Fabric id must be provided");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.Fabric.Name.Required", "Fabric name is required");

    public static ReferenceDomainError PriceOutOfRange(decimal min, decimal max) =>
        new("Reference.Fabric.Price.OutOfRange", $"Fabric price amount must be between {min} and {max}");

    public static ReferenceDomainError ProviderIdIsRequired() =>
        new("Reference.Fabric.ProviderId.Required", "Fabric provider id must be provided");
}
