
namespace Identity.Infrastructure.Persistence.Access;

public class RoleAccessConfiguration : IEntityTypeConfiguration<RoleAccess>
{
    public void Configure(EntityTypeBuilder<RoleAccess> builder)
    {
        builder.ToTable("role_access");
        builder.HasKey(x => x.RoleId);
        builder.Property(x => x.RoleId).HasColumnName("role_id").ValueGeneratedNever();
        builder.Property(x => x.Grants).HasColumnName("grants").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Version).HasColumnName("version").IsRequired().IsConcurrencyToken();
        builder.Property(x => x.UpdatedByName).HasColumnName("updated_by_name").HasMaxLength(256);
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Deleting a role deletes its rights.
        builder.HasOne(x => x.Role)
            .WithOne()
            .HasForeignKey<RoleAccess>(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
