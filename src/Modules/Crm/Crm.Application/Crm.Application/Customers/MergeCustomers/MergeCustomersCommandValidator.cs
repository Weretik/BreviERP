namespace Crm.Application.Customers.MergeCustomers;

public sealed class MergeCustomersCommandValidator : AbstractValidator<MergeCustomersCommand>
{
    public MergeCustomersCommandValidator()
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
