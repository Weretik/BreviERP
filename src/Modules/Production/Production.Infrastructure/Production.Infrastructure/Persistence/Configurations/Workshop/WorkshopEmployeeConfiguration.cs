using Production.Domain.Staff;


namespace Production.Infrastructure.Persistence.Configurations.Workshop;

internal sealed class WorkshopEmployeeConfiguration : IEntityTypeConfiguration<WorkshopEmployee>
{
    public void Configure(EntityTypeBuilder<WorkshopEmployee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.FullName).HasMaxLength(WorkshopEmployee.FullNameMaxLength).IsRequired();
        builder.Property(x => x.NameKey).HasMaxLength(WorkshopEmployee.FullNameMaxLength).IsRequired();
        builder.Property(x => x.Position).HasMaxLength(WorkshopEmployee.PositionMaxLength).IsRequired();
        builder.HasIndex(x => x.NameKey).IsUnique().HasFilter("\"IsActive\"");
    }
}
