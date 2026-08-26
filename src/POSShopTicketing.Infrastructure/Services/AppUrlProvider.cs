using Microsoft.Extensions.Options;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Infrastructure.Services;

public class AppUrlProvider : IAppUrlProvider
{
    private readonly AppUrlSettings _settings;

    public AppUrlProvider(IOptions<AppUrlSettings> settings)
    {
        _settings = settings.Value;
    }

    public string BuildInviteAcceptUrl(string inviteToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.InviteAcceptUrl))
        {
            return string.Empty;
        }

        var separator = _settings.InviteAcceptUrl.Contains('?') ? "&" : "?";
        return $"{_settings.InviteAcceptUrl}{separator}token={Uri.EscapeDataString(inviteToken)}";
    }
}
