using POSShopTicketing.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace POSShopTicketing.Application.Auth.Queries.RegisterTenantDto
{
    public record SignupInviteDto
    {
        public string? Email { get; init; } = string.Empty;
        public TeamMemberRole? Role { get; init; }
    }
}
