namespace Crm.Application.Quotes.Introductions;

/// <summary>Verifies exact imported source keys and source-reviewed Catalog products.</summary>
public interface IIntroductionSourceMappingReader
{
    Task<IntroductionSourceMapping?> ResolveAsync(string snapshotHash, string summerModelCode,
        string winterModelCode, CancellationToken cancellationToken);
}
