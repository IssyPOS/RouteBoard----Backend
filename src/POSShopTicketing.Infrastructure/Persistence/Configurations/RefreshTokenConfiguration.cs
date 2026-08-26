using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenz>
{
    public void Configure(EntityTypeBuilder<RefreshTokenz> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(200);

        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.Ignore(t => t.IsActive);
    }
}
