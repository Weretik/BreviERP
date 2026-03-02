namespace Reference.Domain.Errors;

public static class FabricErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.Fabric.Id.Required",
            "Fabric id must be provided");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.Fabric.Name.Required",
            "Fabric name is required");

    public static ReferenceDomainError CounterpartyIdIsRequired() =>
        new("Reference.Fabric.CounterpartyId.Required",
            "Counterparty id must be non-negative");

    public static ReferenceDomainError PriceOutOfRange(decimal min, decimal max) =>
        new("Reference.Fabric.Price.OutOfRange",
            $"Fabric price amount must be between {min} and {max}");
}
