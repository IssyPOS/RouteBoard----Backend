using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.Tickets.Queries.GetTicketStatusHistory;

public record GetTicketStatusHistoryQuery(Guid TicketId) : IRequest<List<TicketStatusHistoryDto>>;

public class GetTicketStatusHistoryQueryHandler : IRequestHandler<GetTicketStatusHistoryQuery, List<TicketStatusHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTicketStatusHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketStatusHistoryDto>> Handle(GetTicketStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await _context.TicketStatusHistories
            .Include(h => h.ChangedByTeamMember)
            .AsNoTracking()
            .Where(h => h.TicketId == request.TicketId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync(cancellationToken);

        return history.Select(TicketStatusHistoryDto.FromEntity).ToList();
    }
}
