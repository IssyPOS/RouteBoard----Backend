using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Common;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Commands.AddTicketMessage;

/// <summary>
/// POST /tickets/{id}/messages - an agent's reply or internal note.
/// A reply (Direction=Outbound) stamps FirstResponseAt the first time it
/// happens, and is enqueued through IEmailSender/IBackgroundJobScheduler
/// so the customer actually receives it - threaded via a per-ticket
/// Reply-To alias and In-Reply-To set to the last inbound Message-ID, and
/// with the subject-token fallback embedded too. An internal note never
/// leaves POSShopTicketing.
/// </summary>
public record AddTicketMessageCommand : IRequest<Guid>
{
    public Guid TicketId { get; init; }
    public MessageDirection Direction { get; init; } = MessageDirection.Outbound;
    public string Body { get; init; } = string.Empty;
}

public class AddTicketMessageCommandHandler : IRequestHandler<AddTicketMessageCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IDateTime _dateTime;

    public AddTicketMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHtmlSanitizerService htmlSanitizer,
        IBackgroundJobScheduler backgroundJobScheduler,
        IDateTime dateTime)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
        _backgroundJobScheduler = backgroundJobScheduler;
        _dateTime = dateTime;
    }

    public async Task<Guid> Handle(AddTicketMessageCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets
            .Include(t => t.OrganizationContact)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        var lastInboundMessageId = request.Direction == MessageDirection.Outbound
            ? await _context.TicketMessages
                .Where(m => m.TicketId == ticket.Id && m.Direction == MessageDirection.Inbound)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.MessageId)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var generatedMessageId = request.Direction == MessageDirection.Outbound
            ? $"<{Guid.NewGuid()}@{ticket.TenantId}.posshopticketing>"
            : null;

        var message = new TicketMessage
        {
            TenantId = ticket.TenantId,
            TicketId = ticket.Id,
            Direction = request.Direction,
            AuthorType = MessageAuthorType.TeamMember,
            AuthorTeamMemberId = _currentUserService.TeamMemberId,
            Body = _htmlSanitizer.Sanitize(request.Body),
            MessageId = generatedMessageId,
            InReplyToMessageId = lastInboundMessageId
        };

        _context.TicketMessages.Add(message);

        if (request.Direction == MessageDirection.Outbound)
        {
            if (ticket.FirstResponseAt is null)
            {
                ticket.FirstResponseAt = _dateTime.Now;
            }

            if (ticket.OrganizationContact is not null)
            {
                var subjectWithToken = SubjectTokenHelper.AppendToken($"Re: {ticket.Subject}", ticket.TicketNumber);

                _backgroundJobScheduler.Enqueue<ISendReplyEmailJob>(job => job.SendAsync(
                    message.Id, ticket.OrganizationContact.Email, ticket.OrganizationContact.FullName,
                    subjectWithToken, message.Body, lastInboundMessageId, generatedMessageId!, CancellationToken.None));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return message.Id;
    }
}

/// <summary>Hangfire-invokable job interface - kept in Application so
/// the enqueue call above doesn't reference Infrastructure directly;
/// implemented in Infrastructure/BackgroundJobs.</summary>
public interface ISendReplyEmailJob
{
    Task SendAsync(
        Guid ticketMessageId, string toEmail, string? toName, string subject, string bodyHtml,
        string? inReplyToMessageId, string generatedMessageId, CancellationToken cancellationToken);
}
