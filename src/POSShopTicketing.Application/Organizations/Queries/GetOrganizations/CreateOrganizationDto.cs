using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

public record CreateOrganizationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;

    public string Domain { get; init; } = string.Empty;
    public OrganizationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }

    public static CreateOrganizationDto FromEntity(Organization entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Domain = entity.Domain,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt
    };
}
