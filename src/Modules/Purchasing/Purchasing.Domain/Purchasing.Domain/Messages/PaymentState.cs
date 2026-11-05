namespace Purchasing.Domain.Messages;

/// <summary>
/// The payment mark copied from the source sheet. It is only a note in the sheet: it is not a payment, does not create a
/// transaction and does not reduce a debt.
/// </summary>
public enum PaymentState
{
    /// <summary>Status is anything other than «СПЛАЧЕНО».</summary>
    Unconfirmed = 0,

    /// <summary>Status is exactly «СПЛАЧЕНО» after trimming and upper-casing.</summary>
    MarkedPaid = 1,
}
