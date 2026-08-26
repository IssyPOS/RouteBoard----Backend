using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.Mailboxes.Queries.GetMailboxes;

public record GetMailboxesQuery : IRequest<List<MailboxDto>>;

public class GetMailboxesQueryHandler : IRequestHandler<GetMailboxesQuery, List<MailboxDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMailboxesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MailboxDto>> Handle(GetMailboxesQuery request, CancellationToken cancellationToken)
    {
        var mailboxes = await _context.Mailboxes
            .AsNoTracking()
            .OrderByDescending(m => m.IsDefault)
            .ThenBy(m => m.EmailAddress)
            .ToListAsync(cancellationToken);

        return mailboxes.Select(MailboxDto.FromEntity).ToList();
    }
}
