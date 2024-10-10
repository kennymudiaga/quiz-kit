using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class QuizConfig : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);

        builder.Property(x => x.Title).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(250);
        builder.Property(x => x.CategoryId).HasMaxLength(36);
        builder.Property(x => x.OrganizationId).HasMaxLength(36);
        builder.Property(x => x.ImageUrl).HasMaxLength(250);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany<QuizQuestion>("_questions")
            .WithOne()
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(x => x.Questions);
    }
}
