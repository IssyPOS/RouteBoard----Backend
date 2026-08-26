using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Queries.GetTicketMessages;

/// <summary>The ticket's single thread, oldest first. Internal notes are
/// included by default (this endpoint is for the agent-facing thread
/// view) but can be excluded for anything customer-facing.</summary>
public record GetTicketMessagesQuery(Guid TicketId, bool IncludeInternalNotes = true) : IRequest<List<TicketMessageDto>>;

public class GetTicketMessagesQueryHandler : IRequestHandler<GetTicketMessagesQuery, List<TicketMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTicketMessagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketMessageDto>> Handle(GetTicketMessagesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TicketMessages
            .Include(m => m.AuthorTeamMember)
            .AsNoTracking()
            .Where(m => m.TicketId == request.TicketId);

        if (!request.IncludeInternalNotes)
        {
            query = query.Where(m => m.Direction != MessageDirection.InternalNote);
        }

        var messages = await query.OrderBy(m => m.CreatedAt).ToListAsync(cancellationToken);

        return messages.Select(TicketMessageDto.FromEntity).ToList();
    }
}
