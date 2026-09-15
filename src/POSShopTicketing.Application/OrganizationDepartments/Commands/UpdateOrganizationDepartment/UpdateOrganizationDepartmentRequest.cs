using System;
using System.Collections.Generic;
using System.Text;

namespace POSShopTicketing.Application.OrganizationDepartments.Commands.UpdateOrganizationDepartment
{
    public record UpdateOrganizationDepartmentRequest
    {
        public string Name { get; init; } = string.Empty;
    }
}
