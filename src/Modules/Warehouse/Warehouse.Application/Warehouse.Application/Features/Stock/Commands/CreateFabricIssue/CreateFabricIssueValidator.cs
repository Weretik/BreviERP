using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;

public sealed class CreateFabricIssueValidator : AbstractValidator<CreateFabricIssueCommand>
{
    public CreateFabricIssueValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.OrderVersion).GreaterThan(0);
            RuleFor(x => x.Input.PlanVersion).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Input.IssuedOn).OperationDate();
            RuleFor(x => x.Input.IssuedQuantity).GreaterThan(0).WithMessage(StockMessages.IssuedQuantity);
            RuleFor(x => x.Input.Note).MaximumLength(FabricIssue.NoteMaxLength);
            RuleFor(x => x.Input.Lines).NotNull().Must(x => x.Count is > 0 and <= FabricIssue.MaxLines)
                .WithMessage("Додайте від 1 до 20 рядків видачі.");
            RuleForEach(x => x.Input.Lines).ChildRules(line =>
            {
                line.RuleFor(x => x.FabricCode).NotEmpty().WithMessage("Оберіть тканину зі складу.")
                    .MaximumLength(FabricStock.CodeMaxLength);
                line.RuleFor(x => x.Color).NotEmpty().WithMessage("Оберіть колір зі складу.")
                    .MaximumLength(StockColor.MaxLength);
                line.RuleFor(x => x.ExpectedVersion).MaximumLength(64);
                line.RuleFor(x => x.Meters).PositiveMeters();
                line.RuleFor(x => x.PlanColorKey).NotEmpty().When(x => !string.IsNullOrWhiteSpace(x.PlanFabricCode))
                    .WithMessage("Оберіть плановий колір.");
                line.RuleFor(x => x.Layer).Must(x => x is null || Enum.TryParse<FabricLayer>(x, out var l) && Enum.IsDefined(l))
                    .WithMessage(StockMessages.Layer);
                line.RuleFor(x => x.Note).MaximumLength(FabricIssueLine.NoteMaxLength);
            }).When(x => x.Input.Lines is not null);
        });
    }
}
