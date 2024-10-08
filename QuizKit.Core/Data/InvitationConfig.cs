using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class InvitationConfig : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(100);
        builder.Property(x => x.OrganizationId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Role).HasMaxLength(50);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.SenderId).HasMaxLength(36);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
