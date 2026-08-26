using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class AssignmentRuleConfiguration : IEntityTypeConfiguration<AssignmentRule>
{
    public void Configure(EntityTypeBuilder<AssignmentRule> builder)
    {
        builder.ToTable("AssignmentRules");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ScopeType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.LastModifiedBy).HasMaxLength(100);

        builder.HasIndex(r => new { r.TenantId, r.ScopeType, r.ScopeId });

        builder.HasOne(r => r.AssignedToTeamMember)
            .WithMany()
            .HasForeignKey(r => r.AssignedToTeamMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
