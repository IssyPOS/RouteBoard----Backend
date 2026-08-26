namespace POSShopTicketing.Infrastructure.Services;

/// <summary>
/// Bound from the "AppUrls" section of appsettings.json. Links this
/// backend puts into emails, pointing at a frontend it doesn't itself
/// serve (the spec's Angular SPA isn't part of this deliverable).
/// InviteAcceptUrl empty (the default) means "no frontend deployed yet" -
/// the invite email then falls back to showing the raw token with
/// instructions for calling POST /auth/invite/accept directly.
/// </summary>
public class AppUrlSettings
{
    public const string SectionName = "AppUrls";

    public string InviteAcceptUrl { get; set; } = string.Empty;
}
