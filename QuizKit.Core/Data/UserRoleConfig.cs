using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class UserRoleConfig : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(x => new { x.UserProfileId, x.Role });
        builder.Property(x => x.UserProfileId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.Role).IsRequired().HasMaxLength(50);
    }
}
