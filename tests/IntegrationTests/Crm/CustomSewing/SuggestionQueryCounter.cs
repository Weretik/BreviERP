using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace IntegrationTests.Crm.CustomSewing;

internal sealed class SuggestionQueryCounter : DbCommandInterceptor
{
    public int Reads { get; set; }
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Reads++;
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}
