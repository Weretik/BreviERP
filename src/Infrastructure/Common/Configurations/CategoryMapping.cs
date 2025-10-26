namespace Infrastructure.Common.Configurations;

public static class CategoryMapping
{
    public static void Configure<TNode, TId>(
        EntityTypeBuilder<TNode> builder,
        string tableName,
        int nameMaxLen = 100)
        where TNode : CategoryNode<TId, TNode>
        where TId   : struct
    {
        builder.ToTable(tableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(nameMaxLen)
            .IsRequired();

        builder.Property(x => x.Path)
            .HasColumnType("ltree")
            .IsRequired();
        builder.HasIndex(x => x.Path).HasMethod("gist");

        builder.Property(x => x.ParentId);
        builder.HasIndex(x => x.ParentId);


        builder.HasMany(x => x.Children)
            .WithOne()
            .HasForeignKey(x => x.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
