using System.Net;
using Npgsql;

namespace SewingPricing.Jobs.Activation;

public static class ActivationTargetGuard
{
    public static string Validate(string connectionString, string? expectedDatabase = null)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        if (!(target.Host == "localhost" || IPAddress.TryParse(target.Host, out var address) && IPAddress.IsLoopback(address))
            || string.IsNullOrWhiteSpace(target.Database) || expectedDatabase is not null && target.Database != expectedDatabase)
            throw new InvalidOperationException("Activation requires the exact verified local database.");
        return target.Database;
    }
}
