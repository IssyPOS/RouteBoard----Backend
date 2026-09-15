using MediatR;
using POSShopTicketing.Application.OrganizationMembers.Queries.GetOrganizationContacts;
using System;
using System.Collections.Generic;
using System.Text;

namespace POSShopTicketing.Application.OrganizationContacts.Commands.UpdateOrganizationContact
{
    public record UpdateOrganizationContactRequest
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string JobTitle { get; init; } = string.Empty;
        public string? Phone { get; init; }
    }
}
