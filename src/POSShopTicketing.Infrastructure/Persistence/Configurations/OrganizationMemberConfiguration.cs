using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable("OrganizationMembers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FullName).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Email).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Phone).HasMaxLength(30);
        builder.Property(m => m.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.CreatedBy).HasMaxLength(100);
        builder.Property(m => m.LastModifiedBy).HasMaxLength(100);

        // Identified by email address alone - unique per organization,
        // matching the spec's "unique(...)" note on this table (a person
        // could legitimately be a member of two different client orgs
        // with the same address, so this isn't globally unique).
        builder.HasIndex(m => new { m.OrganizationId, m.Email }).IsUnique();
    }
}
