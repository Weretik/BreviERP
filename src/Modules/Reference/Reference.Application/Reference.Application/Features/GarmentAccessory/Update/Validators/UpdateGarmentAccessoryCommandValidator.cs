namespace Reference.Application.Features.GarmentAccessory.Update.Validators;

public sealed class UpdateGarmentAccessoryCommandValidator : AbstractValidator<UpdateGarmentAccessoryCommand>
{
    public UpdateGarmentAccessoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор фурнітури має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на оновлення фурнітури не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва фурнітури є обов'язковою.")
                .MaximumLength(500)
                .WithMessage("Назва фурнітури не може перевищувати 500 символів.");

            RuleFor(x => x.Request.Price)
                .InclusiveBetween(0, 10_000)
                .WithMessage("Ціна фурнітури має бути від 0 до 10000.");

            RuleFor(x => x.Request.SupplierName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва постачальника фурнітури є обов'язковою.")
                .MaximumLength(200)
                .WithMessage("Назва постачальника фурнітури не може перевищувати 200 символів.");
        });
    }
}
