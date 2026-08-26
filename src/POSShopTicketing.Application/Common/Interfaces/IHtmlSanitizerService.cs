namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>
/// Security note from the spec: "Sanitize inbound email HTML before
/// storing or rendering it - the thread viewer is otherwise a
/// stored-XSS vector." Every inbound TicketMessage body is run through
/// this before it's persisted.
/// </summary>
public interface IHtmlSanitizerService
{
    string Sanitize(string html);
}
