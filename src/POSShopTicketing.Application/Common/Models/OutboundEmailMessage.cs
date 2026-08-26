namespace POSShopTicketing.Application.Common.Models;

/// <summary>Everything IEmailSender needs to send one threaded reply.</summary>
public record OutboundEmailMessage(
    string ToEmail,
    string? ToName,
    string FromAlias,
    string Subject,
    string BodyHtml,
    string? InReplyToMessageId,
    string GeneratedMessageId);
