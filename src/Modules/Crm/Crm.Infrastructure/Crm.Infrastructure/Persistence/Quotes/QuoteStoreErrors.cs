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

    public static bool IsDuplicateSettings(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres &&
        postgres.ConstraintName == "PK_QuoteSettings";

    public static bool IsDuplicateMailbox(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres &&
        postgres.ConstraintName == "PK_StaffMailboxes";

    public static bool IsDuplicateTemplateName(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres &&
        postgres.ConstraintName == "IX_QuoteIntroTemplates_NameKey";

    public static bool IsPrimaryKeyViolation(DbUpdateException exception, string constraint) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres &&
        postgres.ConstraintName == constraint;
}
