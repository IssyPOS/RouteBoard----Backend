using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.OrganizationMembers.Queries.GetOrganizationMembers;

public record OrganizationContactDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string? OrganizationName { get; init; }
    public Guid? OrganizationDepartmentId { get; init; }
    public string? OrganizationDepartmentName { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public OrganizationContactStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }

    public static OrganizationContactDto FromEntity(OrganizationContact entity) => new()
    {
        Id = entity.Id,
        OrganizationId = entity.OrganizationId,
        OrganizationName = entity.Organization?.Name,
        OrganizationDepartmentId = entity.OrganizationDepartmentId,
        OrganizationDepartmentName = entity.OrganizationDepartment?.Name,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Email = entity.Email,
        Phone = entity.Phone,
        Status = entity.Status,
        JobTitle = entity.JobTitle,
        CreatedAt = entity.CreatedAt
    };
}
