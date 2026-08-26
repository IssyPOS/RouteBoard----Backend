namespace POSShopTicketing.Domain.Enums;

/// <summary>
/// Unverified -> New -> Open -> Pending <-> Open -> Resolved -> Closed.
/// "Reopened" from the spec's lifecycle diagram is not a resting status
/// here - a customer reply on a Resolved/Closed ticket moves Status
/// straight back to Open, and the transition is recorded in
/// TicketStatusHistory with a "Reopened by customer reply" note, which
/// carries the same information without a redundant status value.
/// </summary>
public enum TicketStatus
{
    Unverified = 0,
    New = 1,
    Open = 2,
    Pending = 3,
    Resolved = 4,
    Closed = 5
}
