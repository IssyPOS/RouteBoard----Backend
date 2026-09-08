using MediatR;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Application.OrganizationMembers.Queries.GetOrganizationMembers;

namespace POSShopTicketing.Application.OrganizationContacts.Queries.GetOrganizationContacts;

public record GetOrganizationContactsQuery : IRequest<PaginatedList<OrganizationContactDto>>
{
    public string? SearchTerm { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}