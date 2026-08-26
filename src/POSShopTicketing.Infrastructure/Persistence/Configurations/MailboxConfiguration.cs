using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Infrastructure.Persistence.Configurations;

public class MailboxConfiguration : IEntityTypeConfiguration<Mailbox>
{
    public void Configure(EntityTypeBuilder<Mailbox> builder)
    {
        builder.ToTable("Mailboxes");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.EmailAddress).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Provider).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.WebhookSecret).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Signature).HasMaxLength(2000);
        builder.Property(m => m.CreatedBy).HasMaxLength(100);
        builder.Property(m => m.LastModifiedBy).HasMaxLength(100);

        builder.HasIndex(m => m.EmailAddress).IsUnique();
    }
}
