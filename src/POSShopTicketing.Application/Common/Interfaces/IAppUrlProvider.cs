namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>
/// Builds links this app puts into emails, back to a frontend it
/// doesn't itself serve (the spec's Angular SPA isn't part of this
/// backend deliverable). If no frontend URL is configured yet (see
/// Infrastructure/Services/AppUrlSettings), BuildInviteAcceptUrl returns
/// an empty string and the invite email falls back to just showing the
/// raw token with instructions for calling the API directly.
/// </summary>
public interface IAppUrlProvider
{
    string BuildInviteAcceptUrl(string inviteToken);
}
