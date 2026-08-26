using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);

        // Without this, EF Core's default Guid-key convention silently
        // replaces any explicitly-set Guid.Empty with a freshly
        // generated one at save time (it can't tell "not set yet" apart
        // from "deliberately Guid.Empty") - exactly what broke the
        // Guid.Empty "Platform" tenant row ApplicationDbContextInitializer
        // relies on for PlatformSuperAdmin's TenantId sentinel. Safe for
        // every other Tenant too: BaseEntity already assigns a real
        // random Id at construction time, before EF Core ever sees it.
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(100);
        builder.Property(t => t.TicketPrefix).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Plan).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.CreatedBy).HasMaxLength(100);
        builder.Property(t => t.LastModifiedBy).HasMaxLength(100);

        builder.HasIndex(t => t.Slug).IsUnique();
    }
}
