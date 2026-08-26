using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using POSShopTicketing.Application.Tickets.Commands.IngestInboundEmail;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Shared.Wrappers;

namespace POSShopTicketing.Api.Controllers;

/// <summary>
/// POST /webhooks/inbound-email/{mailboxId} - the one public,
/// unauthenticated surface in the system (security note, spec section
/// 08): every other endpoint requires a bearer token, this one instead
/// verifies the target Mailbox's WebhookSecret (see
/// IngestInboundEmailCommandHandler) and is rate-limited (see
/// Program.cs) since it's reachable without a login. Point your email
/// provider's inbound-parse webhook (SendGrid Inbound Parse or Postmark
/// Inbound) here, normalized to this payload shape.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/webhooks")]
[EnableRateLimiting("webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly ISender _mediator;

    public WebhooksController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("inbound-email/{mailboxId:guid}")]
    public async Task<ActionResult<ApiResponse<IngestInboundEmailResult>>> InboundEmail(
        Guid mailboxId, [FromHeader(Name = "X-Webhook-Secret")] string webhookSecret, InboundEmailRequest request)
    {
        var result = await _mediator.Send(new IngestInboundEmailCommand
        {
            MailboxId = mailboxId,
            WebhookSecret = webhookSecret,
            SenderEmail = request.SenderEmail,
            SenderName = request.SenderName,
            Subject = request.Subject,
            BodyHtml = request.BodyHtml,
            MessageId = request.MessageId,
            InReplyTo = request.InReplyTo,
            References = request.References
        });

        return Ok(ApiResponse<IngestInboundEmailResult>.Success(
            result, result.IsNewTicket ? "New ticket created." : "Message appended to existing ticket."));
    }
}

/// <summary>Normalized inbound-parse payload - map your provider's
/// webhook body (SendGrid Inbound Parse / Postmark Inbound) to this
/// shape before forwarding, or adapt this action directly to their
/// native format.</summary>
public record InboundEmailRequest(
    string SenderEmail, string? SenderName, string Subject, string BodyHtml,
    string MessageId, string? InReplyTo, string? References);
