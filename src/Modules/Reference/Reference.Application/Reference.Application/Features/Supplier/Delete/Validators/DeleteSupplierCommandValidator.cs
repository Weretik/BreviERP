namespace Reference.Application.Features.Supplier.Delete.Validators;

public sealed class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор постачальника має бути більшим за 0.");
    }
}
