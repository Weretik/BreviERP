using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.Quotes;

internal static class QuoteStoreErrors
{
    private const string NumberIndex = "IX_Quotes_Number";
    private const string SequenceIndex = "IX_Quotes_NumberYear_NumberMonth_NumberSequence";

    public static bool IsDuplicateNumber(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres &&
        postgres.ConstraintName is NumberIndex or SequenceIndex;
}
