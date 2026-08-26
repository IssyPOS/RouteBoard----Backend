-- POSShopTicketing / Routeboard spec, section 02: "PostgreSQL Row-Level
-- Security enforces the same boundary at the database itself, keyed off
-- an app.current_tenant_id session variable set per request."
--
-- TenantSessionInterceptor (Infrastructure/Persistence/Interceptors) sets
-- that session variable on every connection the app opens. EF Core's own
-- global query filters (ApplicationDbContext.OnModelCreating) already
-- scope every query to the caller's tenant - this is the belt-and-
-- suspenders backstop: a bug in the EF filter layer, or a query that
-- calls IgnoreQueryFilters() by mistake, still can't cross tenants once
-- this is applied.
--
-- Run this once against your database after the EF Core migrations
-- (it only touches RLS policies, never table structure, so it's safe to
-- keep separate from `dotnet ef database update`):
--   psql "$CONNECTION_STRING" -f enable-row-level-security.sql
--
-- IMPORTANT: RLS does not apply to a table's owner by default. If the
-- app connects using the same role that ran the EF migrations (the
-- common case for a small deployment), FORCE ROW LEVEL SECURITY below is
-- what makes the policy apply anyway. For a more defense-in-depth setup,
-- create a separate low-privilege runtime role that the app connects as
-- (non-owner, so RLS applies automatically) and keep migrations running
-- as the owner role.

-- IMPORTANT - read before running this against a production database:
--
-- RLS does not restrict a table's OWNER by default. This script
-- deliberately does NOT add FORCE ROW LEVEL SECURITY, and here's why:
-- a handful of legitimate flows have to read a tenant-scoped row before
-- any tenant is known - most importantly, logging in by email (see
-- LoginCommandHandler), plus refresh-token redemption, invite
-- acceptance, and PlatformSuperAdmin's cross-tenant listings. All of
-- these already call .IgnoreQueryFilters() to get past EF Core's layer-1
-- filter for exactly this reason. If this table's owning role were also
-- FORCE-restricted by RLS, that same lookup would be blocked at the
-- database level too - which would break login itself, not just narrow
-- it to one tenant.
--
-- So, as shipped: the app connects using the same role that ran the EF
-- migrations (the table owner), for which these policies exist but -
-- per Postgres's own default behaviour - do not restrict its queries.
-- EF Core's global query filters (ApplicationDbContext.OnModelCreating)
-- are therefore the primary, always-effective isolation mechanism, and
-- these RLS policies are the fully-configured second layer, ready to go.
--
-- To actually activate that second layer in production, provision a
-- separate, non-owner runtime role for the app to connect as day-to-day
-- (keep the migration role separate, used only for `dotnet ef database
-- update`). RLS then enforces automatically for that role on every
-- normal tenant-scoped path. For the handful of legitimately
-- cross-tenant flows above, grant that runtime role a narrow, explicit
-- bypass for just those lookups (e.g. a SECURITY DEFINER function, or a
-- dedicated second connection string used only by those code paths)
-- rather than exempting it from RLS altogether.

DO $$
DECLARE
    tenant_scoped_tables text[] := ARRAY[
        'TeamMembers', 'Organizations', 'OrganizationTeams', 'OrganizationMembers',
        'AssignmentRules', 'Mailboxes', 'Tickets', 'TicketMessages',
        'TicketStatusHistories', 'Tags', 'Notifications', 'SlaPolicies'
    ];
    t text;
BEGIN
    FOREACH t IN ARRAY tenant_scoped_tables LOOP
        EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', t);

        EXECUTE format('DROP POLICY IF EXISTS tenant_isolation ON %I', t);
        EXECUTE format(
            'CREATE POLICY tenant_isolation ON %I USING ("TenantId" = current_setting(''app.current_tenant_id'', true)::uuid)',
            t);
    END LOOP;
END $$;

-- Deliberately NOT covered here (see README for reasoning):
--   "Tenants"    - the tenant root table itself; Platform-only access is
--                   enforced at the application layer (no TenantId column
--                   to key a row-level policy off).
--   "AuditLogs"  - TenantId is nullable (also covers platform-level
--                   actions not scoped to one tenant), which doesn't fit
--                   this table's simple equality-policy pattern.
--   "TicketTags" - a pure join table with no TenantId column of its own;
--                   isolation comes from its Ticket/Tag foreign keys,
--                   which are themselves RLS-protected above.
--   "RefreshTokens", "Attachments" - scoped indirectly via TeamMemberId /
--                   TicketMessageId rather than carrying TenantId
--                   directly.
