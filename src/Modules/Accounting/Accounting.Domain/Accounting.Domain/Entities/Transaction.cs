using Accounting.Domain.Enumerations;
using Accounting.Domain.ValueObjects;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Domain.Accounting.ValueObjects;
using NodaMoney;
using TransactionId = Accounting.Domain.ValueObjects.TransactionId;

namespace Accounting.Domain.Entities;

public class Transaction: BaseEntity<TransactionId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public WalletId WalletId { get; private set; }
    public Money Amount { get; private set; }
    public string? Notes { get; private set; }
    public TransactionType Type { get; private set; } = null!;
    public TransactionCategoryId CategoryId { get; private set; }
    public TransactionCategoryId ParentCategoryId { get; private set; }

    #endregion
}
