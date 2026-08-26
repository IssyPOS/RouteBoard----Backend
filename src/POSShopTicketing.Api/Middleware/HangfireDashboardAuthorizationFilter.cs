using Hangfire.Dashboard;

namespace POSShopTicketing.Api.Middleware;

/// <summary>Restricts /hangfire to Owner/Admin/PlatformSuperAdmin - the
/// dashboard shows every tenant's background jobs (outbound emails, SLA
/// sweeps), so it's platform/tenant-leadership-only, same spirit as the
/// roles table.</summary>
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var user = httpContext.User;

        return user.Identity?.IsAuthenticated == true &&
               (user.IsInRole("Owner") || user.IsInRole("Admin") || user.IsInRole("PlatformSuperAdmin"));
    }
}
