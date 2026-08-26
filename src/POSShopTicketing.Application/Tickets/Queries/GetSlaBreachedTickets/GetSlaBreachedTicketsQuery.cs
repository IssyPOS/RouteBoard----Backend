using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Queries.GetTickets;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Queries.GetSlaBreachedTickets;

public record GetSlaBreachedTicketsQuery : IRequest<List<TicketDto>>;

public class GetSlaBreachedTicketsQueryHandler : IRequestHandler<GetSlaBreachedTicketsQuery, List<TicketDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public GetSlaBreachedTicketsQueryHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<TicketDto>> Handle(GetSlaBreachedTicketsQuery request, CancellationToken cancellationToken)
    {
        var now = _dateTime.Now;

        var candidates = await _context.Tickets
            .Include(t => t.Organization)
            .Include(t => t.OrganizationMember)
            .Include(t => t.AssignedToTeamMember)
            .AsNoTracking()
            .Where(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed
                        && t.DueAt != null && t.DueAt < now)
            .OrderBy(t => t.DueAt)
            .ToListAsync(cancellationToken);

        return candidates.Select(t => TicketDto.FromEntity(t, now)).ToList();
    }
}
