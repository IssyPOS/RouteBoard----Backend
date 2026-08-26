using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Application.Tickets.Commands.AddTicketMessage;
using POSShopTicketing.Infrastructure.Persistence;

namespace POSShopTicketing.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire-invoked: "Background jobs ... for ... the outbound send
/// queue" per the spec's architecture section - keeps the request path
/// for POST /tickets/{id}/messages fast, and Hangfire retries this
/// safely on failure instead of losing the reply if the email provider
/// has a blip.
/// </summary>
public class SendReplyEmailJob : ISendReplyEmailJob
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    public SendReplyEmailJob(ApplicationDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task SendAsync(
        Guid ticketMessageId, string toEmail, string? toName, string subject, string bodyHtml,
        string? inReplyToMessageId, string generatedMessageId, CancellationToken cancellationToken)
    {
        var message = await _context.TicketMessages
            .IgnoreQueryFilters()
            .Include(m => m.Ticket)
            .FirstOrDefaultAsync(m => m.Id == ticketMessageId, cancellationToken);

        if (message?.Ticket is null)
        {
            return;
        }

        await _emailSender.SendReplyAsync(
            new OutboundEmailMessage(
                toEmail, toName,
                FromAlias: $"ticket+{message.Ticket.Id}@mail.posshopticketing.app",
                subject, bodyHtml, inReplyToMessageId, generatedMessageId),
            cancellationToken);
    }
}
