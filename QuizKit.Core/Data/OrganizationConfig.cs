using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizKit.Common.Models.Users;

namespace QuizKit.Core.Data;

public record OrganizationConfig : IEntityTypeConfiguration<OrganizationModel>
{
    public void Configure(EntityTypeBuilder<OrganizationModel> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(250);
        builder.Property(x => x.ImageUrl).HasMaxLength(250);
        builder.Property(x => x.Website).HasMaxLength(250);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.Address).HasMaxLength(250);
    }
}
