using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class TicketStatusHistoryConfiguration : IEntityTypeConfiguration<TicketStatusHistory>
{
    public void Configure(EntityTypeBuilder<TicketStatusHistory> builder)
    {
        builder.ToTable("TicketStatusHistories");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.FromStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.ToStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.Note).HasMaxLength(1000);

        builder.HasIndex(h => new { h.TicketId, h.ChangedAt });

        builder.HasOne(h => h.ChangedByTeamMember)
            .WithMany()
            .HasForeignKey(h => h.ChangedByTeamMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
