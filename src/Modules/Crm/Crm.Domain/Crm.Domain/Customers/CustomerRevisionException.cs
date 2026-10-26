namespace Crm.Domain.Customers;

public sealed class CustomerRevisionException() : Exception("revision_conflict");
