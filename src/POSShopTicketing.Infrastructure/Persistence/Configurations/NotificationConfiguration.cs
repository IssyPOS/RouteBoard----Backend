using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Body).IsRequired().HasMaxLength(1000);

        builder.HasIndex(n => new { n.TeamMemberId, n.IsRead, n.CreatedAt });

        builder.HasOne(n => n.TeamMember)
            .WithMany()
            .HasForeignKey(n => n.TeamMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
