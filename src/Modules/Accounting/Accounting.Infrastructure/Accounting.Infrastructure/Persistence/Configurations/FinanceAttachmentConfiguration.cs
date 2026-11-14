using Accounting.Domain.Attachments;
using Accounting.Domain.Transactions;
using Accounting.Infrastructure.Persistence;


namespace Accounting.Infrastructure.Persistence.Configurations;

internal sealed class FinanceAttachmentConfiguration : IEntityTypeConfiguration<FinanceAttachment>
{
    public void Configure(EntityTypeBuilder<FinanceAttachment> builder)
    {
        builder.ToTable("Attachments", t =>
            t.HasCheckConstraint("CK_Attachments_Size", $"\"SizeBytes\" > 0 AND \"SizeBytes\" <= {FinanceAttachment.MaxBytes}"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AccountingConverters.AttachmentId).ValueGeneratedNever();
        builder.Property(x => x.TransactionId).HasConversion(AccountingConverters.TransactionId);
        builder.Property(x => x.FileName).HasMaxLength(FinanceAttachment.FileNameMaxLength).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(FinanceAttachment.ContentTypeMaxLength).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(FinanceAttachment.StorageKeyMaxLength).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(FinanceAttachment.ByMaxLength).IsRequired();
        builder.Property(x => x.DeletedBy).HasMaxLength(FinanceAttachment.ByMaxLength);

        // A link inside the module: an operation is never deleted, so its files stay with it.
        builder.HasOne<FinanceTransaction>().WithMany().HasForeignKey(x => x.TransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.StorageKey).IsUnique();
        builder.HasIndex(x => new { x.TransactionId, x.DeletedAt });
    }
}
