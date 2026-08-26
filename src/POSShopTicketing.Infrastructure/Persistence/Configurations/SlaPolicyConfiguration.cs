using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.ToTable("SlaPolicies");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Priority).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.LastModifiedBy).HasMaxLength(100);

        builder.HasIndex(p => new { p.TenantId, p.Priority }).IsUnique();
    }
}
