using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Common;
using POSShopTicketing.Application.Tickets.Events;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Tickets.Commands.IngestInboundEmail;

/// <summary>
/// The inbound pipeline from spec section 03, implemented exactly as
/// diagrammed: dedupe by Message-ID, resolve the sender against a
/// registered Organization Member, thread against a Message-ID we sent
/// (falling back to a ticket-number token in the subject line), and
/// either append to an existing ticket (reopening it if it was
/// Resolved/Closed) or create a new one - Unverified and
/// unassigned if the sender didn't match anyone, New and routed through
/// Assignment Presets if they did.
///
/// Runs with no tenant/JWT context (this is the one public,
/// unauthenticated surface in the system) - the caller authenticates by
/// presenting the target Mailbox's WebhookSecret instead, and every
/// query here explicitly scopes to the resolved TenantId rather than
/// relying on the (absent) ICurrentTenantService claim.
/// </summary>
public record IngestInboundEmailCommand : IRequest<IngestInboundEmailResult>
{
    public Guid MailboxId { get; init; }
    public string WebhookSecret { get; init; } = string.Empty;
    public string SenderEmail { get; init; } = string.Empty;
    public string? SenderName { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string BodyHtml { get; init; } = string.Empty;
    public string MessageId { get; init; } = string.Empty;
    public string? InReplyTo { get; init; }
    public string? References { get; init; }
}

public class IngestInboundEmailCommandHandler : IRequestHandler<IngestInboundEmailCommand, IngestInboundEmailResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ITicketNumberGenerator _ticketNumberGenerator;
    private readonly ITicketPriorityClassifier _priorityClassifier;
    private readonly ITicketAssignmentService _assignmentService;
    private readonly ISlaCalculator _slaCalculator;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IDateTime _dateTime;
    private readonly IPublisher _publisher;

    public IngestInboundEmailCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        ITicketNumberGenerator ticketNumberGenerator,
        ITicketPriorityClassifier priorityClassifier,
        ITicketAssignmentService assignmentService,
        ISlaCalculator slaCalculator,
        IHtmlSanitizerService htmlSanitizer,
        IDateTime dateTime,
        IPublisher publisher)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _ticketNumberGenerator = ticketNumberGenerator;
        _priorityClassifier = priorityClassifier;
        _assignmentService = assignmentService;
        _slaCalculator = slaCalculator;
        _htmlSanitizer = htmlSanitizer;
        _dateTime = dateTime;
        _publisher = publisher;
    }

    public async Task<IngestInboundEmailResult> Handle(IngestInboundEmailCommand request, CancellationToken cancellationToken)
    {
        var mailbox = await _context.Mailboxes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == request.MailboxId, cancellationToken)
            ?? throw new NotFoundException(nameof(Mailbox), request.MailboxId);

        if (mailbox.WebhookSecret != request.WebhookSecret)
        {
            throw new UnauthorizedException("Invalid webhook secret for this mailbox.");
        }

        var tenantId = mailbox.TenantId;

        // See ICurrentTenantService.SetTenant: RLS's session variable
        // otherwise stays "no tenant" for this whole request, since
        // there's no JWT to read a tenant_id claim from - this webhook
        // resolves its tenant from the Mailbox instead, right here.
        _currentTenantService.SetTenant(tenantId);

        // Dedup: inbound-parse webhooks retry on timeout and would
        // otherwise double-post a customer's reply into the thread.
        var alreadyProcessed = await _context.TicketMessages
            .IgnoreQueryFilters()
            .Include(m => m.Ticket)
            .FirstOrDefaultAsync(m => m.MessageId == request.MessageId, cancellationToken);

        if (alreadyProcessed is not null)
        {
            return new IngestInboundEmailResult(
                alreadyProcessed.TicketId, alreadyProcessed.Ticket!.TicketNumber, alreadyProcessed.Id, false,
                alreadyProcessed.Ticket.Status == TicketStatus.Unverified);
        }

        var normalizedSenderEmail = request.SenderEmail.Trim().ToLowerInvariant();
        var sanitizedBody = _htmlSanitizer.Sanitize(request.BodyHtml);
        var now = _dateTime.Now;

        var contact = await _context.OrganizationContacts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Email == normalizedSenderEmail, cancellationToken);

        Ticket ticket;
        bool isNewTicket;

        if (contact is not null)
        {
            var existingTicket = await FindThreadedTicketAsync(tenantId, request, cancellationToken);

            if (existingTicket is not null)
            {
                ticket = existingTicket;
                isNewTicket = false;

                if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed)
                {
                    var previousStatus = ticket.Status;
                    ticket.Status = TicketStatus.Open;
                    ticket.ClosedAt = null;

                    _context.TicketStatusHistories.Add(new TicketStatusHistory
                    {
                        TenantId = tenantId,
                        TicketId = ticket.Id,
                        FromStatus = previousStatus,
                        ToStatus = TicketStatus.Open,
                        ChangedAt = now,
                        Note = "Reopened by customer reply"
                    });

                    await _publisher.Publish(
                        new TicketStatusChangedEvent(tenantId, ticket.Id, ticket.TicketNumber, previousStatus, TicketStatus.Open),
                        cancellationToken);

                    // "auto-assigned to its last owner" - only resolve a
                    // new one if it somehow has none.
                    ticket.AssignedToTeamMemberId ??= await _assignmentService.ResolveAssigneeAsync(
                        tenantId, ticket.OrganizationId, ticket.OrganizationDepartmentId, ticket.OrganizationContactId, cancellationToken);
                }
            }
            else
            {
                ticket = await CreateTicketAsync(
                    tenantId, mailbox, request, normalizedSenderEmail, sanitizedBody, now,
                    status: TicketStatus.New,
                    organizationId: contact.OrganizationId,
                    organizationTeamId: contact.OrganizationDepartmentId,
                    organizationMemberId: contact.Id,
                    runAssignment: true,
                    cancellationToken);
                isNewTicket = true;
            }
        }
        else
        {
            // Unrecognized sender: never silently dropped - it still
            // becomes a ticket, just Unverified and kept out of normal
            // queues, with Assignment Presets skipped entirely (nothing
            // registered to match against).
            ticket = await CreateTicketAsync(
                tenantId, mailbox, request, normalizedSenderEmail, sanitizedBody, now,
                status: TicketStatus.Unverified,
                organizationId: null,
                organizationTeamId: null,
                organizationMemberId: null,
                runAssignment: false,
                cancellationToken);
            isNewTicket = true;
        }

        var message = new TicketMessage
        {
            TenantId = tenantId,
            TicketId = ticket.Id,
            Direction = MessageDirection.Inbound,
            AuthorType = MessageAuthorType.OrganizationContact,
            AuthorEmail = normalizedSenderEmail,
            AuthorName = request.SenderName,
            Body = sanitizedBody,
            MessageId = request.MessageId,
            InReplyToMessageId = request.InReplyTo
        };

        _context.TicketMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        if (isNewTicket)
        {
            await _publisher.Publish(
                new TicketCreatedEvent(tenantId, ticket.Id, ticket.TicketNumber, ticket.Subject, ticket.Status == TicketStatus.Unverified),
                cancellationToken);

            if (ticket.AssignedToTeamMemberId.HasValue)
            {
                await _publisher.Publish(
                    new TicketAssignedEvent(tenantId, ticket.Id, ticket.TicketNumber, ticket.AssignedToTeamMemberId.Value),
                    cancellationToken);
            }
        }

        return new IngestInboundEmailResult(ticket.Id, ticket.TicketNumber, message.Id, isNewTicket, ticket.Status == TicketStatus.Unverified);
    }

    /// <summary>Headers match a Message-ID we sent? -> append/reopen.
    /// Falls back to the ticket-number token embedded in the subject
    /// when a mail client strips References/In-Reply-To.</summary>
    private async Task<Ticket?> FindThreadedTicketAsync(
        Guid tenantId, IngestInboundEmailCommand request, CancellationToken cancellationToken)
    {
        var candidateMessageIds = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.InReplyTo))
        {
            candidateMessageIds.Add(request.InReplyTo.Trim());
        }
        if (!string.IsNullOrWhiteSpace(request.References))
        {
            candidateMessageIds.AddRange(
                request.References.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        if (candidateMessageIds.Count > 0)
        {
            var byHeader = await _context.TicketMessages
                .IgnoreQueryFilters()
                .Include(m => m.Ticket)
                .Where(m => m.TenantId == tenantId && m.MessageId != null && candidateMessageIds.Contains(m.MessageId))
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.Ticket)
                .FirstOrDefaultAsync(cancellationToken);

            if (byHeader is not null)
            {
                return byHeader;
            }
        }

        var ticketNumberToken = SubjectTokenHelper.TryExtractTicketNumber(request.Subject);
        if (ticketNumberToken is null)
        {
            return null;
        }

        return await _context.Tickets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.TicketNumber == ticketNumberToken, cancellationToken);
    }

    private async Task<Ticket> CreateTicketAsync(
        Guid tenantId, Mailbox mailbox, IngestInboundEmailCommand request,
        string normalizedSenderEmail, string sanitizedBody, DateTime now,
        TicketStatus status, Guid? organizationId, Guid? organizationTeamId, Guid? organizationMemberId,
        bool runAssignment, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.IgnoreQueryFilters().FirstAsync(t => t.Id == tenantId, cancellationToken);

        var priority = _priorityClassifier.Classify(request.Subject, sanitizedBody);
        var dueAt = await _slaCalculator.CalculateDueDateAsync(tenantId, priority, now, cancellationToken);

        Guid? assignedToTeamMemberId = null;
        if (runAssignment)
        {
            assignedToTeamMemberId = await _assignmentService.ResolveAssigneeAsync(
                tenantId, organizationId, organizationTeamId, organizationMemberId, cancellationToken);
        }

        var ticket = new Ticket
        {
            TenantId = tenantId,
            TicketNumber = await _ticketNumberGenerator.NextAsync(tenant.TicketPrefix, cancellationToken),
            OrganizationId = organizationId,
            OrganizationDepartmentId = organizationTeamId,
            OrganizationContactId = organizationMemberId,
            RawSenderEmail = normalizedSenderEmail,
            MailboxId = mailbox.Id,
            Subject = request.Subject.Trim(),
            Status = status,
            Priority = priority,
            Source = TicketSource.Email,
            AssignedToTeamMemberId = assignedToTeamMemberId,
            DueAt = dueAt
        };

        ticket.StatusHistory.Add(new TicketStatusHistory
        {
            TenantId = tenantId,
            TicketId = ticket.Id,
            FromStatus = TicketStatus.Unverified,
            ToStatus = status,
            ChangedAt = now,
            Note = status == TicketStatus.Unverified ? "Created from unrecognized sender" : "Created from inbound email"
        });

        _context.Tickets.Add(ticket);
        return ticket;
    }
}
