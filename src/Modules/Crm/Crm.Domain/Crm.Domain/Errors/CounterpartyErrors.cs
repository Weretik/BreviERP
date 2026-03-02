namespace Crm.Domain.Errors;

public static class CounterpartyErrors
{
    public static CrmDomainError IdIsRequired() =>
        new("Crm.Counterparty.Id.Required",
            "Counterparty id must be provided");

    public static CrmDomainError NameIsRequired() =>
        new("Crm.Counterparty.Name.Required",
            "Counterparty name is required");

    public static CrmDomainError TypeIsRequired() =>
        new("Crm.Counterparty.Type.Required",
            "Counterparty type must be provided");
}
