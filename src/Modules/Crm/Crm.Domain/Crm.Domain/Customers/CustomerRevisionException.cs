namespace Crm.Domain.Customers;

public sealed class CustomerRevisionException() : Exception("Клієнта вже змінили. Оновіть сторінку й повторіть зміни.");
