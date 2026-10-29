namespace Crm.Infrastructure.Persistence.Documents;

public sealed class DocumentNumberSequence
{
    public long Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int NextNumber { get; set; } = 10;
}
