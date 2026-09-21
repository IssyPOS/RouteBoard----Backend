using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Organizations.Commands.UpdateOrganization;

public record UpdateOrganizationRequest
{
    public string Name { get; init; } = string.Empty;

    public string Domain { get; init; } = string.Empty;

    public OrganizationStatus Status { get; init; }
}