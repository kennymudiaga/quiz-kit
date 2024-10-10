using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class UserProfileConfig : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PasswordHash).HasMaxLength(256);

        builder.HasIndex(x => x.Email).IsUnique();

        builder.HasMany<UserRole>("_roles").WithOne().HasForeignKey(x => x.UserProfileId);
        builder.Ignore(x => x.Roles);

        builder.HasMany<UserOrganization>("_organizations").WithOne().HasForeignKey(x => x.UserProfileId);
        builder.Ignore(x => x.Organizations);

        builder.Ignore(x => x.IsAccountLocked);
    }
}
