using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class QuizQuestionConfig : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.HasKey(x => new { x.Id, x.QuizId });
        builder.Property(x => x.QuizId).HasMaxLength(36);
        builder.Property(x => x.QuestionText).HasMaxLength(250);
        builder.Property(x => x.Answer).HasMaxLength(1);
        builder.Property(x => x.A).HasMaxLength(100);
        builder.Property(x => x.B).HasMaxLength(100);
        builder.Property(x => x.C).HasMaxLength(100);
        builder.Property(x => x.D).HasMaxLength(100);
    }
}
