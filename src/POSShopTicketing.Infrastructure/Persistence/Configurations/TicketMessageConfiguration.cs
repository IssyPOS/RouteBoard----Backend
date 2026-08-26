using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class TicketMessageConfiguration : IEntityTypeConfiguration<TicketMessage>
{
    public void Configure(EntityTypeBuilder<TicketMessage> builder)
    {
        builder.ToTable("TicketMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Direction).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.AuthorType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.AuthorEmail).HasMaxLength(200);
        builder.Property(m => m.AuthorName).HasMaxLength(200);
        builder.Property(m => m.Body).IsRequired();
        builder.Property(m => m.MessageId).HasMaxLength(500);
        builder.Property(m => m.InReplyToMessageId).HasMaxLength(500);
        builder.Property(m => m.CreatedBy).HasMaxLength(100);
        builder.Property(m => m.LastModifiedBy).HasMaxLength(100);

        builder.HasIndex(m => m.MessageId);
        builder.HasIndex(m => new { m.TicketId, m.CreatedAt });

        builder.HasOne(m => m.AuthorTeamMember)
            .WithMany()
            .HasForeignKey(m => m.AuthorTeamMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Attachments)
            .WithOne(a => a.TicketMessage)
            .HasForeignKey(a => a.TicketMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
