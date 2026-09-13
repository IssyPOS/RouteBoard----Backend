using System;
using System.Collections.Generic;
using System.Text;

namespace POSShopTicketing.Application.OrganizationDepartments.Queries.GetOrganizationDepartments
{
    public class UpdateOrganizationDepartmentDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
