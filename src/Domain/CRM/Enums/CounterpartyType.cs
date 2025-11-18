namespace Domain.CRM.Enums;

public sealed class CounterpartyType(string name, int value) : SmartEnum<CounterpartyType>(name, value)
{
    public static readonly CounterpartyType Customer = new("Клієнт", 1);
    public static readonly CounterpartyType Partner = new("Партнер", 2);
    public static readonly CounterpartyType Lead = new("Лід", 3);
    public static readonly CounterpartyType Supplier = new("Постальник", 4);
}
