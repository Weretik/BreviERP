using Crm.Domain.Embroidery.Saved;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculations;

internal static class EmbroideryNumberSearch
{
    /// <summary>«ВШ-0042», «вш42» or «42» → 42; anything else → -1 (matches no record).</summary>
    public static long Parse(string search)
    {
        var digits = search.StartsWith("вш", StringComparison.Ordinal) ? search[2..].TrimStart('-', ' ') : search;
        return digits.Length is > 0 and <= 12 && digits.All(char.IsAsciiDigit) ? long.Parse(digits) : -1;
    }
}
