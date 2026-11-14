namespace Reference.Application.Features.Supplier.Commands.DeleteSupplier;

public sealed class DeleteSupplierValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор постачальника має бути більшим за 0.");
    }
}
