using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public record CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.Description).HasMaxLength(250);
        builder.Property(x => x.LastUpdateTime).IsRequired(false);
    }
}