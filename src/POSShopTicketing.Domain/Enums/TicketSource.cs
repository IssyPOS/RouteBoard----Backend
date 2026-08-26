namespace POSShopTicketing.Domain.Enums;

/// <summary>
/// Client organizations never get a portal - the only channel in or out
/// is email - so sources are deliberately narrow: an inbound Email, a
/// ticket an agent opened by hand (Manual), or one created by a caller
/// through the API (Api, e.g. a partner integration).
/// </summary>
public enum TicketSource
{
    Email = 0,
    Manual = 1,
    Api = 2
}
