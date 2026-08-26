using Microsoft.Extensions.Options;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Infrastructure.Services;

public class SystemMailboxAddressProvider : ISystemMailboxAddressProvider
{
    private readonly InboundEmailSettings _settings;

    public SystemMailboxAddressProvider(IOptions<InboundEmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public string BuildAddress(string tenantSlug) => $"{tenantSlug}@{_settings.SystemDomain}";

    public bool IsSystemDomain(string emailAddress) =>
        !string.IsNullOrWhiteSpace(_settings.SystemDomain) &&
        emailAddress.EndsWith($"@{_settings.SystemDomain}", StringComparison.OrdinalIgnoreCase);
}
