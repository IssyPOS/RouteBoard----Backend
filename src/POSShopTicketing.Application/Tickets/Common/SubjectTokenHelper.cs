using System.Text.RegularExpressions;

namespace POSShopTicketing.Application.Tickets.Common;

/// <summary>
/// Fallback threading mechanism used when a mail client strips
/// References / In-Reply-To: every outbound subject line carries the
/// ticket number in brackets (e.g. "Re: Printer jam [SP123-000456]"),
/// which this both writes and reads back - the same fallback Salesforce
/// Email-to-Case relies on, per the spec's research notes.
/// </summary>
public static partial class SubjectTokenHelper
{
    [GeneratedRegex(@"\[([A-Z0-9]{2,10}-\d{3,10})\]")]
    private static partial Regex TokenPattern();

    public static string AppendToken(string subject, string ticketNumber)
    {
        return TokenPattern().IsMatch(subject) ? subject : $"{subject} [{ticketNumber}]";
    }

    public static string? TryExtractTicketNumber(string subject)
    {
        var match = TokenPattern().Match(subject);
        return match.Success ? match.Groups[1].Value : null;
    }
}
