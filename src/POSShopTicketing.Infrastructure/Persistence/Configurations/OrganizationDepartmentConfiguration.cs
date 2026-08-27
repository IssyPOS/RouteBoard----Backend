using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class OrganizationDepartmentConfiguration : IEntityTypeConfiguration<OrganizationDepartment>
{
    public void Configure(EntityTypeBuilder<OrganizationDepartment> builder)
    {
        builder.ToTable("OrganizationDepartments");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.CreatedBy).HasMaxLength(100);
        builder.Property(t => t.LastModifiedBy).HasMaxLength(100);

        builder.HasIndex(t => new { t.OrganizationId, t.Name }).IsUnique();

        builder.HasMany(t => t.Members)
            .WithOne(m => m.OrganizationDepartment)
            .HasForeignKey(m => m.OrganizationDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
