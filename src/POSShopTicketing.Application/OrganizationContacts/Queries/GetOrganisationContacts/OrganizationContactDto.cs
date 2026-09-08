using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.OrganizationContacts.Queries.GetOrganizationContacts;

public record OrganizationContactDto
{
    public Guid Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public string OrganizationName { get; init; } = string.Empty;

    public string? DepartmentName { get; init; }

    public OrganizationContactStatus Status { get; init; }

    public DateTime CreatedAt { get; init; }

    public static OrganizationContactDto FromEntity(OrganizationContact entity)
        => new()
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Email = entity.Email,
            Phone = entity.Phone,
            OrganizationName = entity.Organization?.Name ?? string.Empty,
            DepartmentName = entity.OrganizationDepartment?.Name,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt
        };
}