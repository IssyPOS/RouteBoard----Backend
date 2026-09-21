using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Domain { get; set; } = string.Empty;

    public OrganizationStatus Status { get; set; }
}