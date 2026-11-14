namespace Crm.Application.Features.Customers.Commands.MergeCustomers;

public sealed class MergeCustomersValidator : AbstractValidator<MergeCustomersCommand>
{
    public MergeCustomersValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
        RuleFor(x => x.Actor.IsAdmin).Equal(true);
        RuleFor(x => x.VictimId).NotEmpty().NotEqual(x => x.CustomerId);
        RuleFor(x => x.VictimRevision).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
