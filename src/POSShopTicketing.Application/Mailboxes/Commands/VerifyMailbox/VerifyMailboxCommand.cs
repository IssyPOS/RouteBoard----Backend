using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.Mailboxes.Commands.VerifyMailbox;

/// <summary>Marks a Mailbox verified once its DNS/forwarding setup (or
/// provider domain authentication) has been confirmed. A real
/// implementation would check SPF/DKIM/MX records here; this MVP trusts
/// the caller (an Admin) to confirm it manually.</summary>
public record VerifyMailboxCommand(Guid MailboxId) : IRequest;

public class VerifyMailboxCommandHandler : IRequestHandler<VerifyMailboxCommand>
{
    private readonly IApplicationDbContext _context;

    public VerifyMailboxCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(VerifyMailboxCommand request, CancellationToken cancellationToken)
    {
        var mailbox = await _context.Mailboxes.FindAsync(new object[] { request.MailboxId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Mailbox), request.MailboxId);

        mailbox.IsVerified = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
