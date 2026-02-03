namespace Reference.Infrastructure.Congigurations;

public sealed class FabricConfiguration : IEntityTypeConfiguration<Fabric>
{
    public void Configure(EntityTypeBuilder<Fabric> builder)
    {
        builder.ToTable("FabricsReference");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.FabricIdConvert)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CounterpartyId)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasConversion(ReferenceConverters.MoneyConvert)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
