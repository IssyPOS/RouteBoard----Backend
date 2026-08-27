using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Queries.GetTickets;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.Tickets.Queries.GetTicketById;

public record GetTicketByIdQuery(Guid Id) : IRequest<TicketDto>;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public GetTicketByIdQueryHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<TicketDto> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Tickets
            .Include(t => t.Organization)
            .Include(t => t.OrganizationDepartment)
            .Include(t => t.OrganizationContact)
            .Include(t => t.AssignedToTeamMember)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.Id);

        return TicketDto.FromEntity(entity, _dateTime.Now);
    }
}
