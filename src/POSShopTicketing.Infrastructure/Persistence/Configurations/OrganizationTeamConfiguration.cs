using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class OrganizationTeamConfiguration : IEntityTypeConfiguration<OrganizationTeam>
{
    public void Configure(EntityTypeBuilder<OrganizationTeam> builder)
    {
        builder.ToTable("OrganizationTeams");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.CreatedBy).HasMaxLength(100);
        builder.Property(t => t.LastModifiedBy).HasMaxLength(100);

        builder.HasIndex(t => new { t.OrganizationId, t.Name }).IsUnique();

        builder.HasMany(t => t.Members)
            .WithOne(m => m.OrganizationTeam)
            .HasForeignKey(m => m.OrganizationTeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
