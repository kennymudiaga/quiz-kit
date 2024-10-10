using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class QuizResultConfig : IEntityTypeConfiguration<QuizResult>
{
    public void Configure(EntityTypeBuilder<QuizResult> builder)
    {
        builder.HasKey(x => new { x.QuizId, x.UserId});
        builder.Property(x => x.QuizId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.AnswersCSV).IsRequired().HasMaxLength(360);
    }
}
