using Ai.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ai.Infrastructure.Persistence.Prompts;

public sealed class AiPromptConfiguration : IEntityTypeConfiguration<AiPrompt>
{
    public void Configure(EntityTypeBuilder<AiPrompt> b)
    {
        b.ToTable("Prompts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(AiPrompt.KeyMaxLength);
        b.Property(x => x.Text).HasMaxLength(AiPrompt.TextMaxLength);
        b.Property(x => x.UpdatedBy).HasMaxLength(256);
    }
}
