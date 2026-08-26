using Ganss.Xss;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Infrastructure.Services;

/// <summary>Security note from the spec: every inbound message body is
/// run through this before it's stored - the thread viewer renders raw
/// HTML, so unsanitized input would be a stored-XSS vector.</summary>
public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer = new();

    public string Sanitize(string html) => _sanitizer.Sanitize(html);
}
