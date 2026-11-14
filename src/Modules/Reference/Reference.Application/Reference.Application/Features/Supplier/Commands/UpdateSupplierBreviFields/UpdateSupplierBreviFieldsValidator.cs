namespace Reference.Application.Features.Supplier.Commands.UpdateSupplierBreviFields;

public sealed class UpdateSupplierBreviFieldsValidator : AbstractValidator<UpdateSupplierBreviFieldsCommand>
{
    public UpdateSupplierBreviFieldsValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор постачальника має бути більшим за 0.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Примітки не можуть перевищувати 500 символів.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
