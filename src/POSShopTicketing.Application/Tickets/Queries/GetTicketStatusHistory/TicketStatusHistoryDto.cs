using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Queries.GetTicketStatusHistory;

public record TicketStatusHistoryDto
{
    public TicketStatus FromStatus { get; init; }
    public TicketStatus ToStatus { get; init; }
    public string? ChangedByName { get; init; }
    public string? Note { get; init; }
    public DateTime ChangedAt { get; init; }

    public static TicketStatusHistoryDto FromEntity(TicketStatusHistory entity) => new()
    {
        FromStatus = entity.FromStatus,
        ToStatus = entity.ToStatus,
        ChangedByName = entity.ChangedByTeamMember?.FullName,
        Note = entity.Note,
        ChangedAt = entity.ChangedAt
    };
}
