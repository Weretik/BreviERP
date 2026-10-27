using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Domain.Quotes.Items;

namespace Crm.Application.Quotes.Mapping;

public static class QuoteBuildResult
{
    public static Result<QuoteDetail> Failure(Result<IReadOnlyList<QuoteItem>> result)
        => result.Status switch
        {
            ResultStatus.Conflict => Result.Conflict(result.Errors.ToArray()),
            ResultStatus.Invalid => Result.Invalid(result.ValidationErrors.ToArray()),
            _ => Result.Error("quote_content_invalid")
        };
}
