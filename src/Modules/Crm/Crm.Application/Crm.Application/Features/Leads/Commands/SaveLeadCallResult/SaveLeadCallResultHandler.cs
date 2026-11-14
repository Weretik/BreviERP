using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Mapping;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;
using Crm.Domain.Leads.Enums;

namespace Crm.Application.Features.Leads.Commands.SaveLeadCallResult;

public sealed class SaveLeadCallResultHandler(ILeadStore store)
    : ICommandHandler<SaveLeadCallResultCommand, Result<LeadCallResultResponse>>
{
    public async ValueTask<Result<LeadCallResultResponse>> Handle(SaveLeadCallResultCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        var outcome = Enum.Parse<LeadOutcome>(input.Outcome);
        LeadCallResult result;
        if (command.Id is { } id)
        {
            var found = await store.LeadCallResults.FindAsync([id], cancellationToken);
            if (found is null) return Result.NotFound(LeadMessages.ResultNotFound);
            result = found;
            result.Edit(input.Name, outcome, input.RetryDays, input.IsActive, input.Position);
        }
        else
        {
            if (await store.LeadCallResults.CountAsync(cancellationToken) >= LeadLimits.MaxResults)
                return Result.Invalid(new ValidationError(LeadMessages.TooManyResults));
            result = new LeadCallResult(input.Name, outcome, input.RetryDays, input.Position);
            if (!input.IsActive) result.Edit(input.Name, outcome, input.RetryDays, false, input.Position);
            store.LeadCallResults.Add(result);
        }

        await store.SaveChangesAsync(cancellationToken);
        return command.Id is null ? Result.Created(result.ToResponse()) : Result.Success(result.ToResponse());
    }
}
