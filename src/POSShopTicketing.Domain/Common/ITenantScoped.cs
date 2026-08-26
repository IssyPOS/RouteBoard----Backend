namespace POSShopTicketing.Domain.Common;

/// <summary>
/// Marks an entity as belonging to exactly one Tenant (Service Provider).
/// Every implementer gets an EF Core global query filter keyed on
/// TenantId (Infrastructure/Persistence/ApplicationDbContext), and every
/// implementer's table gets a Postgres Row-Level Security policy keyed on
/// the same column (Infrastructure/Persistence/Scripts) - belt and
/// suspenders per the spec's isolation design.
/// </summary>
public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
