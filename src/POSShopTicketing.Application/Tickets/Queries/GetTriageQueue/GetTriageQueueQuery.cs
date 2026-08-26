using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Application.Tickets.Queries.GetTickets;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Queries.GetTriageQueue;

/// <summary>GET /tickets/triage - Manager+ only. Every ticket from an
/// unrecognized sender, waiting on Register/Link/Anonymize/Reject.</summary>
public record GetTriageQueueQuery : IRequest<PaginatedList<TicketDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetTriageQueueQueryHandler : IRequestHandler<GetTriageQueueQuery, PaginatedList<TicketDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public GetTriageQueueQueryHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<PaginatedList<TicketDto>> Handle(GetTriageQueueQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Where(t => t.Status == TicketStatus.Unverified)
            .OrderBy(t => t.CreatedAt)
            .AsQueryable();

        var paged = await PaginatedList<Domain.Entities.Ticket>.CreateAsync(query, request.PageNumber, request.PageSize);

        var now = _dateTime.Now;
        var dtos = paged.Items.Select(t => TicketDto.FromEntity(t, now)).ToList();

        return new PaginatedList<TicketDto>(dtos, paged.TotalCount, paged.PageNumber, request.PageSize);
    }
}
