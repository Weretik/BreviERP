using System.Globalization;
using Identity.Contracts.Staff;

namespace SewingPricing.Jobs.Activation;

/// <summary>The activation runs on behalf of an existing, not blocked Admin or Manager account.</summary>
public static class ActivationActorReader
{
    public static async Task<string> RequireManagerAsync(IStaffDirectory staff, string id, CancellationToken cancellationToken = default)
    {
        var active = int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var userId)
            ? (await staff.ListActiveAsync(["Admin", "Manager"], cancellationToken)).FirstOrDefault(x => x.Id == userId)
            : null;
        return active?.Id.ToString(CultureInfo.InvariantCulture)
               ?? throw new InvalidOperationException("An existing unlocked Admin/Manager actor is required.");
    }
}
