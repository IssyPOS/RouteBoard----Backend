using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(t => t.TicketNumber).IsUnique();

        builder.Property(t => t.RawSenderEmail).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Subject).IsRequired().HasMaxLength(250);
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Priority).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(t => t.Source).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(t => t.CreatedBy).HasMaxLength(100);
        builder.Property(t => t.LastModifiedBy).HasMaxLength(100);

        builder.HasIndex(t => new { t.TenantId, t.Status });
        builder.HasIndex(t => new { t.TenantId, t.AssignedToTeamMemberId });
        builder.HasIndex(t => t.DueAt);

        builder.HasOne(t => t.Organization)
            .WithMany(o => o.Tickets)
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.OrganizationTeam)
            .WithMany()
            .HasForeignKey(t => t.OrganizationTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.OrganizationMember)
            .WithMany(m => m.Tickets)
            .HasForeignKey(t => t.OrganizationMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Mailbox)
            .WithMany(m => m.Tickets)
            .HasForeignKey(t => t.MailboxId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.AssignedToTeamMember)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToTeamMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Messages)
            .WithOne(m => m.Ticket)
            .HasForeignKey(m => m.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.StatusHistory)
            .WithOne(h => h.Ticket)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
