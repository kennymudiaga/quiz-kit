using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class UserOrganizationConfig :IEntityTypeConfiguration<UserOrganization>
{
    public void Configure(EntityTypeBuilder<UserOrganization> builder)
    {
        builder.HasKey(x => new { x.UserProfileId, x.OrganizationId });
        builder.Property(x => x.UserProfileId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.OrganizationId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.Role).HasMaxLength(50);
    }
}
