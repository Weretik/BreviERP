namespace Crm.Domain.Customers;

public sealed class CustomerValidationException(string message) : Exception(message);
