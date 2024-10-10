using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class PracticeQuestionConfig : IEntityTypeConfiguration<PracticeQuestion>
{
    public void Configure(EntityTypeBuilder<PracticeQuestion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.QuestionText).HasMaxLength(250);
        builder.Property(x => x.Answer).HasMaxLength(1);
        builder.Property(x => x.A).HasMaxLength(100);
        builder.Property(x => x.B).HasMaxLength(100);
        builder.Property(x => x.C).HasMaxLength(100);
        builder.Property(x => x.D).HasMaxLength(100);
        builder.Property(x => x.CategoryId).HasMaxLength(36);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
