using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Mailboxes.Queries.GetMailboxes;

public record MailboxDto
{
    public Guid Id { get; init; }
    public string EmailAddress { get; init; } = string.Empty;
    public MailboxProvider Provider { get; init; }
    public bool IsVerified { get; init; }
    public bool IsSystemProvided { get; init; }
    public bool IsDefault { get; init; }

    public static MailboxDto FromEntity(Mailbox entity) => new()
    {
        Id = entity.Id,
        EmailAddress = entity.EmailAddress,
        Provider = entity.Provider,
        IsVerified = entity.IsVerified,
        IsSystemProvided = entity.IsSystemProvided,
        IsDefault = entity.IsDefault
    };
}
