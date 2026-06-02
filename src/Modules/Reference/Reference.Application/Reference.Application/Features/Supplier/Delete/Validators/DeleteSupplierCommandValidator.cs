namespace Reference.Application.Features.Supplier.Delete.Validators;

public sealed class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
