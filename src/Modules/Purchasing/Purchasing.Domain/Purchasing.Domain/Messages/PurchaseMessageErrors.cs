
using BuildingBlocks.Domain.Results;

namespace Purchasing.Domain.Messages;

public static class PurchaseMessageErrors
{
    public static Error PaymentNotLinked() => Error.Conflict("Purchasing.Message.Payment.NotLinked",
        "Цю оплату вже відв'язано від рахунку. Оновіть сторінку.");

    public static Error NoteTooLong() => Error.Validation("Purchasing.Message.Note.TooLong",
        $"Примітка — не більше {PurchaseMessage.ReviewNoteMaxLength} символів.", "note");
}
