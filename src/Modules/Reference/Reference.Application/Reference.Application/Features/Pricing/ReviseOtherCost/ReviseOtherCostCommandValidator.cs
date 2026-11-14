namespace Reference.Application.Features.Pricing.ReviseOtherCost;

public sealed class ReviseOtherCostCommandValidator : AbstractValidator<ReviseOtherCostCommand>
{
    public const decimal MaxValue = 999_999_999_999.99999999m;

    public ReviseOtherCostCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ExpectedVersionId).GreaterThan(0).WithMessage("Неправильна версія. Оновіть сторінку.");
        RuleFor(x => x.RequestId).NotEmpty().WithMessage("Немає ідентифікатора запиту. Оновіть сторінку й спробуйте ще раз.");
        RuleFor(x => x.Value).Must(value => value is >= 0 and <= MaxValue && decimal.Round(value, 8) == value)
            .WithMessage("Вкажіть невід'ємне число, не більше восьми знаків після коми.");
    }
}
