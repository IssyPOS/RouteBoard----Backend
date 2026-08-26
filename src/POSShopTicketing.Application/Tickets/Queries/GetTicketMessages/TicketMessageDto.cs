using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Queries.GetTicketMessages;

public record TicketMessageDto
{
    public Guid Id { get; init; }
    public MessageDirection Direction { get; init; }
    public MessageAuthorType AuthorType { get; init; }
    public string? AuthorName { get; init; }
    public string? AuthorEmail { get; init; }
    public string Body { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public static TicketMessageDto FromEntity(TicketMessage entity) => new()
    {
        Id = entity.Id,
        Direction = entity.Direction,
        AuthorType = entity.AuthorType,
        AuthorName = entity.AuthorType == MessageAuthorType.TeamMember
            ? entity.AuthorTeamMember?.FullName
            : entity.AuthorName,
        AuthorEmail = entity.AuthorType == MessageAuthorType.TeamMember
            ? entity.AuthorTeamMember?.Email
            : entity.AuthorEmail,
        Body = entity.Body,
        CreatedAt = entity.CreatedAt
    };
}
