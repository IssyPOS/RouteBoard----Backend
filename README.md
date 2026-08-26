# POSShopTicketing

A multi-tenant helpdesk platform for service providers to run ticketing for
the organizations they support - ASP.NET Core Web API on **.NET 10**,
**PostgreSQL**, and **JWT auth** required on every request. Built from a
technical specification for a Zoho Desk / Salesforce Service Cloud-style
product; the namespace and product name stay **POSShopTicketing**
throughout, independent of the source document's own working title.

## Architecture

Clean/onion architecture, five projects:

```
POSShopTicketing.sln
src/
  POSShopTicketing.Domain          # Entities, enums, no dependencies on anything else
  POSShopTicketing.Application     # Use cases (CQRS via MediatR), validation, domain events, interfaces
  POSShopTicketing.Infrastructure  # EF Core + PostgreSQL, JWT/password hashing, Hangfire, RLS interceptor
  POSShopTicketing.Api             # Controllers, auth middleware, composition root
  POSShopTicketing.Shared          # Cross-cutting response wrappers (ApiResponse, PaginatedResponse)
```

Dependency direction: `Api → Application/Infrastructure/Shared`,
`Infrastructure → Application`, `Application → Domain/Shared`. Each
Application feature (Auth, Platform, TeamMembers, Organizations,
OrganizationTeams, OrganizationMembers, AssignmentRules, Mailboxes,
Tickets, Reports) is a vertical slice: `Commands/<Verb><Entity>/` and
`Queries/<Verb><Entity>/`, each folder holding its request, handler, and
validator together. `Tickets/Events/` holds MediatR notifications
(`TicketCreatedEvent`, `TicketAssignedEvent`, `TicketStatusChangedEvent`,
`TicketEscalatedEvent`, `SlaBreachedEvent`) that command handlers publish
and alert handlers consume.

## Actors & domain model

One login-bearing type, one silent data-only type - client organizations
never get credentials, a portal, or an app; every interaction on their
side is email.

| Entity | Belongs to | Signs in? | Role |
|---|---|---|---|
| **Tenant** | - | - | The Service Provider - the company running support. Owns everything below it. |
| **TeamMember** | Tenant | Yes | Owner / Admin / Manager / Agent - an employee working tickets. Also doubles as the "Agent" tickets get assigned to. |
| **Organization** | Tenant | No | A client company being supported. Pure attribution/routing data. |
| **OrganizationTeam** | Organization | No | A department (IT, Sales...) so tickets can be attributed within a client org. |
| **OrganizationMember** | OrganizationTeam (optional) | No | A person at the client who emails in - identified by email address alone. |
| **AssignmentRule** | Tenant | - | "This org / org team / member always routes to this Team Member." |
| **Mailbox** | Tenant | - | One inbound address, tied to a provider (SendGrid/Postmark) and a webhook secret. |
| **Ticket** | Tenant | - | The work item. Optionally linked to an org, org team, and member. |
| **TicketMessage** | Ticket | - | One entry in the thread: Inbound / Outbound / InternalNote. |
| **TicketStatusHistory** | Ticket | - | One row per status transition - the audit trail behind every triage decision too. |

Plus **RefreshToken**, **Attachment**, **Tag**/**TicketTag**,
**Notification**, **AuditLog**, and **SlaPolicy** (Phase 5 in the roadmap,
already implemented here).

## Roles & permissions

| Role | Can | Notably cannot |
|---|---|---|
| **Owner** | Everything, incl. billing & plan changes, grant Owner/Admin | Be removed from the tenant |
| **Admin** | Manage team members & roles (except granting Owner), orgs, org teams, routing rules, mailboxes | Billing |
| **Manager** | Reassign any ticket, resolve the triage queue, view reports | Manage other teams' routing rules |
| **Agent** | Work tickets assigned to them | See tickets outside their own assignment, resolve the triage queue |
| **PlatformSuperAdmin** | Manage tenants & suspend/reactivate them | See any tenant's ticket content day-to-day |

`PlatformSuperAdmin` is never scoped to a tenant (`TeamMember.TenantId` is
the `Guid.Empty` sentinel for this role - see "Multi-tenancy" below) and
lives entirely behind `PlatformController` (`/api/platform/tenants`).

## Multi-tenancy

Shared database, shared schema - every tenant-scoped table carries a
`TenantId`. Isolation in two layers:

1. **EF Core global query filters** - every entity implementing
   `ITenantScoped` automatically gets `.HasQueryFilter(e => e.TenantId ==
   CurrentTenantIdOrEmpty)`, applied via reflection in
   `ApplicationDbContext.OnModelCreating` so no entity can be added
   without picking up the filter. This is the **primary, always-on**
   enforcement - it works regardless of how you deploy or connect.
2. **PostgreSQL Row-Level Security** - `Persistence/Scripts/enable-row-
   level-security.sql` adds a `tenant_isolation` policy to every
   `ITenantScoped` table, keyed off an `app.current_tenant_id` session
   variable that `TenantSessionInterceptor` keeps in sync with
   `ICurrentTenantService` before every command executes (not just at
   connection-open - two flows, inbound email ingestion and tenant
   self-registration, only learn which tenant they're operating on
   partway through the request). **Read the comment block at the top of
   that script before running it** - it's deliberately not `FORCE`d onto
   the table owner, because a couple of legitimate flows (login by
   email, refresh-token redemption, invite acceptance) have to look up a
   tenant-scoped row *before* any tenant is known, which is fundamentally
   incompatible with a fully-forced RLS policy on the same connection
   role. The script explains the production-hardening path (a separate,
   non-owner runtime role) in full.

Primary keys are UUIDs; a separate human-readable `TicketNumber` (e.g.
`ACME-000456`, from one shared Postgres sequence prefixed per tenant) is
what agents and email subject lines actually show.

## Email ingestion & threading

`POST /api/webhooks/inbound-email/{mailboxId}` is the **one public,
unauthenticated endpoint in the system** (everything else needs a bearer
token) - authenticated instead by an `X-Webhook-Secret` header checked
against that `Mailbox`'s stored secret, and separately rate-limited.
Point your provider's inbound-parse webhook here (SendGrid Inbound Parse
or Postmark Inbound recommended over polling IMAP or per-provider OAuth
for an MVP - **neither is required**, see "Two ways to get a Mailbox"
below), normalized to the payload `WebhooksController` expects.

### Two ways to get a Mailbox

`POST /api/mailboxes` (Owner/Admin), matching the spec's "the address a
client emails is one you control" section:

- **Omit `emailAddress`** → a zero-setup **system-provided address**,
  `{tenant-slug}@{InboundEmail:SystemDomain}` (e.g.
  `acme-support@support.your-platform-domain.example`), verified
  immediately - no DNS action from the tenant at all. You (the platform
  operator) configure MX/inbound-parse for **one** domain, once (set
  `InboundEmail:SystemDomain` in `appsettings.json`), and every tenant
  gets a working address instantly from then on.
- **Supply `emailAddress`** → the tenant's own domain, forwarded to this
  app's webhook (one MX/forwarding change on their end, works with any
  mail provider on their side) - requires a follow-up `POST
  /mailboxes/{id}/verify` once that's live.

Either way, **you aren't tied to SendGrid** - `Provider` on a `Mailbox`
is just a label (`SendGridInboundParse` / `PostmarkInbound` / `Other`)
recording which inbound-parse webhook format to expect; nothing in the
ticketing pipeline itself requires a specific vendor. Postmark's own
`@inbound.postmarkapp.com` addresses are worth knowing about too - they
need **zero DNS from anyone**, not even the platform operator, which
makes them the fastest way to get a real end-to-end test working before
you've set up your own domain at all.

For pure local testing with no public URL yet (nothing on the internet
can reach `localhost`), skip real email entirely - `POSShopTicketing.http`
already has webhook requests that POST directly to
`/api/webhooks/inbound-email/{mailboxId}`, simulating exactly what a
provider would send. An [ngrok](https://ngrok.com)/Cloudflare Tunnel is
the other option if you want to test a real provider's actual payload
shape against a not-yet-deployed API.

The pipeline (`IngestInboundEmailCommandHandler`), exactly as specified:

1. **Dedupe by Message-ID** - inbound-parse webhooks retry on timeout and
   would otherwise double-post a reply into the thread.
2. **Sender matches a registered Organization Member?**
   - **Yes** → does `In-Reply-To`/`References` match a Message-ID this
     app sent? → append to that ticket, reopening it if it was
     Resolved/Closed. If headers were stripped, fall back to a
     ticket-number token embedded in the subject line (`[ACME-000456]`)
     - the same fallback Salesforce Email-to-Case relies on. Neither
     matches → create a new ticket, `Status: New`, run Assignment Rules
     (member → org team → org → unassigned).
   - **No** → create a ticket anyway (never silently dropped), `Status:
     Unverified`, no organization/member link, Assignment Rules skipped
     entirely, and it surfaces only in the Manager+ triage queue - see
     "A ticket I just created isn't showing up" below if that catches
     you off guard.

Outbound replies (`POST /tickets/{id}/messages` with `Direction:
Outbound`) get a per-ticket `Reply-To` alias and `In-Reply-To`/subject
token set to keep the thread intact in the customer's own mail client,
and are sent via a Hangfire background job so the request path stays
fast.

## Outbound email (SMTP - not tied to any one provider)

Nothing in `POSShopTicketing` requires SendGrid, or any specific email
platform, for **sending** mail (ticket replies, invite links, and the
activity audit trail below) - `IEmailSender` is a plain interface with
two implementations:

- **`EmailSender`** (default, zero config) - logs the message instead of
  sending it. Nothing to set up; safe out of the box.
- **`SmtpEmailSender`** - a generic SMTP client (via
  [MailKit](https://github.com/jstedfast/MailKit), the maintained
  replacement for .NET's own deprecated `SmtpClient`) that works with
  **any** SMTP-capable provider: Hostinger's own email hosting (since
  you're likely already paying for it on the same VPS), Gmail with an
  app password, Outlook, Amazon SES, Mailgun, Postmark, or SendGrid's
  SMTP relay if you'd rather keep using it. Set the `Smtp` section in
  `appsettings.json` (`Host`, `Port`, `Username`, `Password`,
  `FromAddress`, ...) and `SmtpEmailSender` is picked up automatically -
  no code change (see `Infrastructure/DependencyInjection.cs`:
  `Smtp:Host` empty → log-based sender, set → SMTP sender).

This is entirely separate from **inbound** parsing (turning an email
into a ticket), which does need a provider whose webhook this app can
receive - see "Two ways to get a Mailbox" above.

### Filled-in examples

Never put real credentials directly in `appsettings.json` - use `dotnet
user-secrets` locally, or the `SMTP_*` variables in your `.env` file for
Docker (see `.env.example`; `docker-compose.yml` maps them to
`Smtp__Host` etc. automatically, standard ASP.NET Core config-from-env
convention). The values below are what actually goes in each of those,
per provider:

**Hostinger's own email hosting** (likely simplest, since you're
already on their VPS - create the mailbox first in hPanel → Emails):
```
SMTP_HOST=smtp.hostinger.com
SMTP_PORT=465
SMTP_USERNAME=no-reply@your-domain.example    # the full mailbox address
SMTP_PASSWORD=<that mailbox's password>
SMTP_USE_START_TLS=false                       # port 465 is implicit TLS, not STARTTLS
SMTP_FROM_ADDRESS=no-reply@your-domain.example # must match SMTP_USERNAME
```

**Gmail** (fine for personal testing; requires 2-Step Verification
turned on, then a 16-character **App Password** from
[myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
- your normal Gmail password will not work here):
```
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=youraddress@gmail.com
SMTP_PASSWORD=<16-character App Password, no spaces>
SMTP_USE_START_TLS=true
SMTP_FROM_ADDRESS=youraddress@gmail.com
```

**SendGrid's SMTP relay** (if you'd rather keep using the account you
already set up for inbound parsing - Settings → API Keys → create one
scoped to "Mail Send"):
```
SMTP_HOST=smtp.sendgrid.net
SMTP_PORT=587
SMTP_USERNAME=apikey                           # this is literally the string "apikey", not your username
SMTP_PASSWORD=<your SendGrid API key, starts with SG.>
SMTP_USE_START_TLS=true
SMTP_FROM_ADDRESS=<a sender you've verified in SendGrid>
```

Amazon SES, Mailgun, Postmark, and Outlook/Office365 all work the same
way - host/port/username/password from that provider's SMTP
credentials page, same four fields.

**Verifying it's working without waiting on real credentials**: even
with `SMTP_HOST` left blank, every outbound email attempt still gets
logged (see "Logging") - look for `OUTBOUND EMAIL to ...` lines in
`logs/posshopticketing-*.log` or the console. That confirms the
audit/reply pipeline itself is firing correctly before you spend time
debugging SMTP credentials.

## Team member invites

`POST /auth/invite` (Owner/Admin) creates the invited `TeamMember` row
and a single-use token, then **emails that token directly to the
invitee** via `IEmailSender` (`InviteTeamMemberCommand`) - it is never
returned in the API response. This is deliberate, not an oversight: the
person who calls `/auth/invite` is the *inviter*, not the *invitee* -
if the invite secret came back in that response, the inviter would be
able to set the new person's password themselves before that person
even knew an account existed. Compare to how Slack, GitHub, or Notion
invites work: the person who sends the invite never sees the invite
link either, only "invitation sent."

The response you actually get back is just `{ teamMemberId, expiresAt }`
- enough to show "invitation sent, expires on {date}" without ever
holding the secret. To get the real token for testing:

- **With `Smtp` configured** (see "Outbound email" above) - check the
  invitee's real inbox.
- **Without it** (the default) - the log-based `IEmailSender` still logs
  every attempt; look for an `OUTBOUND EMAIL to ...` line in
  `logs/posshopticketing-*.log` or the console, right after the invite
  call. The token is right there in the body.

The email includes a clickable link if `AppUrls:InviteAcceptUrl` is set
(pointing at wherever a frontend eventually lives, e.g.
`https://app.your-domain.example/accept-invite`, with `?token=...`
appended automatically) - left blank by default, in which case the
email just shows the raw token with instructions to call `POST
/auth/invite/accept` directly, which is all this backend-only
deliverable can meaningfully offer without a frontend of its own.

## Activity audit trail

Every login, logout, and ticket operation is recorded to the `AuditLogs`
table and - if `AuditEmail:Enabled` (default `true`) and outbound email
is configured per the section above - emailed to every active
Owner/Admin on the tenant, so there's a running "who did what and when"
trail without anyone needing to check the database:

- `TeamMember.LoggedIn` - on `POST /auth/login`, `/auth/register-tenant`,
  and `/auth/invite/accept` (each starts a real session). Deliberately
  **not** raised on `/auth/refresh`, which just extends an existing
  session silently.
- `TeamMember.LoggedOut` - on the new `POST /auth/logout`, which revokes
  the presented refresh token (JWTs are stateless, so there's nothing
  server-side to revoke for the access token itself - it just expires on
  its own short timer).
- `Ticket.Created`, `Ticket.StatusChanged`, `Ticket.Assigned`,
  `Ticket.Escalated`, `Ticket.SlaBreached` - the existing ticket domain
  events, each with an additional handler
  (`Tickets/Events/*AuditHandler.cs`) purely for this trail, separate
  from the ones already driving in-app `Notification`s. Who gets
  credited: the authenticated Team Member on the request for
  everything that happens through the normal API, the customer's own
  email address for tickets created (or reopened) via the inbound
  webhook (`TicketAuditActor.cs`), "Automated assignment rule" when a
  new ticket's Assignment Rule fires with no Team Member involved at
  all, and "System" for the Hangfire SLA sweep.

**Who actually receives the email** - this is the part that trips
people up: it's **not** sent to whoever just logged in. It goes to every
active Owner/Admin **on that tenant**, every time, regardless of which
role triggered the event - the whole point is oversight, an Agent's
login is exactly the kind of thing an Owner would want to see. If the
Owner or Admin themselves logs in, they're on that recipient list too,
so yes, you can end up emailing yourself about your own login - expected,
not a bug.

This means **the seeded demo accounts (`owner@acme-support.test`,
`admin@acme-support.test`, ...) will never actually deliver anything**,
SMTP configured or not - `.test` is a reserved, non-routable TLD (nothing
sends real mail there on purpose). To see a real email land in a real
inbox: register your own tenant with your real address as the Owner
(`POST /auth/register-tenant`), then log in as that account - or invite
a real address into an existing tenant and accept that invite instead.
Either way, once SMTP is configured (see above), that login is what
triggers the send.

`AuditLog` rows are always written regardless of `AuditEmail:Enabled` -
that flag only controls whether an email goes out too. Worth knowing:
this fires **per event**, not batched - a busy tenant will generate a
steady stream of these emails; a daily-digest option would be a
reasonable follow-up if that turns out to be too chatty in practice.

## Unregistered-sender triage

`GET /api/tickets/triage` (Manager+) lists every `Unverified` ticket.
Four actions, each written to `TicketStatusHistory` with the acting
Manager:

- **Register** (`POST .../triage/register`) - create a new Organization
  Member on an existing org/team or a brand-new one, ticket moves to New.
- **Link** (`.../triage/link`) - attach the sender to an *existing*
  member (alternate/typo'd address) - same effect, no duplicate.
- **Anonymize** (`.../triage/anonymize`) - hand it to an Agent directly,
  no org/member attribution, an unattributed one-off.
- **Reject** (`.../triage/reject`) - close without engaging.

## Ticket lifecycle

`Unverified → New → Open → Pending ↔ Open → Resolved → Closed`. A
customer reply on a Resolved/Closed ticket moves it straight back to
Open, auto-assigned to its last owner - recorded in
`TicketStatusHistory` with a "Reopened by customer reply" note rather
than as a separate persisted status. `Escalated` is a boolean flag, set
manually via `PATCH /tickets/{id}/escalate` for now and SLA-driven once
business-hours-aware escalation lands. Priority is `Low`/`Medium`/`High`/
`Urgent` (`Medium` default), set explicitly or auto-classified by
keyword when omitted on ticket creation.

## Core ticketing features, mapped

- **Omnichannel conversion** → the email ingestion pipeline above (the
  spec deliberately narrows "omnichannel" to email-only, since client
  orgs never get any other channel in - see the design principle in
  "Actors & domain model").
- **Automated workflows** → `ITicketAssignmentService` (rule cascade),
  `ITicketPriorityClassifier` (keyword-based), and the whole
  domain-event → `IAlertNotifier` pipeline (writes an in-app
  `Notification` row and logs - swap the registration for a real
  email/Slack/SignalR channel without touching any handler).
- **Status tracking** → `TicketStatusHistory` + `GET
  /tickets/{id}/status-history`.
- **SLA management** → `SlaPolicy` per tenant/priority, `DueAt` stamped
  on creation and recomputed on priority change, `SlaBreachRecurringJob`
  (Hangfire, every 5 minutes) sweeping for breaches, `GET
  /tickets/sla-breaches`.

## Logging

[Serilog](https://serilog.net) replaces the default ASP.NET Core logger
entirely (`Program.cs` calls `Host.UseSerilog(...)`) - every existing
`ILogger<T>` injection throughout the app (audit logging, alert
notifications, the SLA sweep, the exception middleware, EF Core's own
query logging) keeps working unchanged, it just now flows through
Serilog's sinks instead.

**One log file per run**: the moment the process starts, `Program.cs`
captures that timestamp and writes to `logs/posshopticketing-
{yyyyMMdd-HHmmss}.log` for the rest of that run - restart the app and
you get a brand-new file, so a specific run's activity is never mixed in
with a previous one. (A 100 MB size-based cap with a 31-file retention
limit protects disk space if a single run stays up a very long time - a
long-lived deployment, not a `dotnet run` you restart often - without
otherwise rolling the file by date.) A second sink writes the same
events to the console for whatever's watching it live (`dotnet run`,
`docker compose logs -f`, etc.).

`UseSerilogRequestLogging()` adds one clean structured line per HTTP
request (method, path, status code, timing) - placed after
`ExceptionHandlingMiddleware` in the pipeline so it still reports the
correct final status code even for exceptions that middleware translates
into a 4xx/5xx `ApiResponse`.

Log levels are configured via the `Serilog:MinimumLevel` section in
`appsettings.json`/`appsettings.Development.json` (`Debug` overall in
Development, with `Microsoft.EntityFrameworkCore.Database.Command` also
turned up so you can see the actual SQL EF Core runs) - change the
`Override` entries there rather than in code. In Docker, the log
directory is `/app/logs`, created and `chown`'d to the non-root
`appuser` in the `Dockerfile`, and mounted back to `./logs` on the host
via `docker-compose.yml` so you can read a run's log without shelling
into the container.

## API surface

Every response is wrapped in `ApiResponse<T>` / `PaginatedResponse<T>`.
Enums serialize as strings. See `src/POSShopTicketing.Api/
POSShopTicketing.http` for runnable examples of everything below.

```
POST   /api/auth/register-tenant          (public) bootstrap a new tenant + Owner
POST   /api/auth/login                    (public)
POST   /api/auth/refresh                  (public)
POST   /api/auth/logout                   (public) revokes the refresh token
POST   /api/auth/invite                   Owner/Admin
POST   /api/auth/invite/accept            (public, token-based)

GET    /api/teammembers
PATCH  /api/teammembers/{id}/role         Owner/Admin
PATCH  /api/teammembers/{id}/disable      Owner/Admin

GET    /api/organizations
POST   /api/organizations                 Owner/Admin
PUT    /api/organizations/{id}            Owner/Admin
GET    /api/organizations/{id}/teams
POST   /api/organizations/{id}/teams      Owner/Admin
GET    /api/organizations/{id}/members
POST   /api/organizations/{id}/members    Owner/Admin/Manager
POST   /api/organizations/{id}/members/{memberId}/merge

GET    /api/assignmentrules
POST   /api/assignmentrules               Owner/Admin
GET    /api/mailboxes
POST   /api/mailboxes                     Owner/Admin
POST   /api/mailboxes/{id}/verify         Owner/Admin

GET    /api/tickets?status=&priority=&organizationId=&assignedToTeamMemberId=&escalated=
GET    /api/tickets/{id}
POST   /api/tickets                       manual creation
PATCH  /api/tickets/{id}/status
PATCH  /api/tickets/{id}/priority
PATCH  /api/tickets/{id}/assignee         Manager+
PATCH  /api/tickets/{id}/escalate         Manager+
GET    /api/tickets/{id}/status-history
GET    /api/tickets/{id}/messages
POST   /api/tickets/{id}/messages         reply or internal note
GET    /api/tickets/sla-breaches
GET    /api/tickets/triage                Manager+
POST   /api/tickets/{id}/triage/register  Manager+
POST   /api/tickets/{id}/triage/link      Manager+
POST   /api/tickets/{id}/triage/anonymize Manager+
POST   /api/tickets/{id}/triage/reject    Manager+

POST   /api/webhooks/inbound-email/{mailboxId}   (public, webhook-secret auth)

GET    /api/reports/volume                Manager+
GET    /api/reports/response-times        Manager+

GET    /api/platform/tenants              PlatformSuperAdmin
GET    /api/platform/tenants/{id}         PlatformSuperAdmin
PATCH  /api/platform/tenants/{id}/suspend PlatformSuperAdmin
PATCH  /api/platform/tenants/{id}/reactivate PlatformSuperAdmin
```

## Seeded demo data

On first run in `Development`, the app migrates and seeds:

| Email | Password | Role | Tenant |
|---|---|---|---|
| `superadmin@posshopticketing.platform` | `Passw0rd!123` | PlatformSuperAdmin | - |
| `owner@acme-support.test` | `Passw0rd!123` | Owner | Acme Support |
| `admin@acme-support.test` | `Passw0rd!123` | Admin | Acme Support |
| `manager@acme-support.test` | `Passw0rd!123` | Manager | Acme Support |
| `agent1@acme-support.test` / `agent2@acme-support.test` | `Passw0rd!123` | Agent | Acme Support |

...plus two client organizations (Contoso Ltd, Fabrikam Inc) with teams
and members, one Assignment Rule, four SLA policies, a verified demo
Mailbox (webhook secret: `dev-webhook-secret-do-not-use-in-production` -
used directly in the `.http` file's webhook examples), and four tickets
spanning New/Pending/Resolved/**Unverified** so the triage queue has
something in it immediately. **Change or remove all of this before any
real deployment.**

## Running it locally

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) and a
PostgreSQL instance.

```bash
cd POSShopTicketing
dotnet restore
dotnet ef migrations add InitialCreate \
  --project src/POSShopTicketing.Infrastructure \
  --startup-project src/POSShopTicketing.Api

dotnet run --project src/POSShopTicketing.Api
```

Update `ConnectionStrings:DefaultConnection` and the `Jwt` section in
`appsettings.Development.json` first if you're not using the defaults.
Then (once) apply the RLS script - read its header comment first:

```bash
psql "$CONNECTION_STRING" -f src/POSShopTicketing.Infrastructure/Persistence/Scripts/enable-row-level-security.sql
```

Log in with a seeded account, use Swagger's "Authorize" button (or the
`.http` file) with the returned `accessToken` for everything else.

## Deploying with Docker (Hostinger)

Hostinger's shared **Web App Hosting** (Node.js/PHP/Python/website
builder) does not run an ASP.NET Core/Kestrel app - the right product is
a **Hostinger VPS with Docker** (any of their VPS KVM plans support
Docker; the control panel also offers one-click Docker/Coolify/Dokploy
templates if you'd rather manage it through a UI than raw `docker
compose`). This repo ships a multi-stage `Dockerfile` (`.NET 10 SDK` →
`ASP.NET runtime`, Alpine, non-root) and a `docker-compose.yml` (API +
Postgres).

```bash
# On the VPS, after cloning this repo:
cp .env.example .env        # fill in POSTGRES_PASSWORD and JWT_SECRET
docker compose up -d --build

# Apply migrations once (first deploy, and again after any migration-adding change):
docker compose exec api dotnet ef database update \
  --project /src/POSShopTicketing.Infrastructure --startup-project /src/POSShopTicketing.Api
# (or bake `dotnet ef database update` into a startup script/init
# container for a fully automated deploy - the sample compose file keeps
# it manual and explicit for a first deployment)

# Then apply the RLS script once too:
docker compose exec postgres psql -U posshopticketing -d posshopticketing \
  -f /path/to/enable-row-level-security.sql   # mount it as a volume, or paste it into psql interactively
```

Put a reverse proxy (Caddy, Nginx, or Hostinger's built-in one if your
plan includes it) in front of port 8080 for TLS - the container itself
just speaks plain HTTP internally. Point your domain's mail
forwarding/MX or your email provider's inbound-parse webhook at
`https://your-domain/api/webhooks/inbound-email/{mailboxId}` once a
`Mailbox` is created and verified through the API.

## Troubleshooting

**A ticket I just created (via the webhook) isn't showing up in `GET
/api/tickets`.** Check the webhook's own response first - it already
tells you: `"isUnverified": true` means that's exactly why. `GET
/api/tickets` **excludes `Unverified` tickets by default** ("kept out of
agents' normal queues until a Manager decides what it is" - see
"Unregistered-sender triage"). A ticket lands `Unverified` whenever the
inbound sender doesn't match a registered `OrganizationMember` on that
tenant - very easy to hit while testing, since the `.http` file's sample
senders (`priya.kapoor@contoso.example`, etc.) only exist in the
**seeded demo tenant**, not in a tenant you registered yourself. Two ways
to actually see it:

- `GET /api/tickets?status=Unverified` (works today, any authenticated
  tenant role can call it)
- `GET /api/tickets/triage` (Manager+, the canonical queue - this is
  where you'd Register/Link/Anonymize/Reject it)

If neither shows it, the more likely cause is a **tenant/mailbox
mismatch** rather than the status filter: the `Ticket.TenantId` is
whichever tenant owns the `mailboxId` you posted the webhook to, not
whichever tenant your bearer token belongs to. Decode your access token
at [jwt.io](https://jwt.io) and compare its `tenant_id` claim against
`GET /api/mailboxes` (called with that same token) - if the mailbox
you're posting to isn't in that list, that's the mismatch: the ticket
was created, just under a different tenant than the one you're querying.
A quick tell without decoding anything: the `ticketNumber` in the
webhook's response is prefixed with the *owning* tenant's `TicketPrefix`
(e.g. `ACME-000123` for the seeded demo tenant) - if that prefix doesn't
match the tenant you expect, that confirms it.

**`SeedAsync` throws `DbUpdateException` / Postgres error `23503`
("violates foreign key constraint FK_TeamMembers_Tenants_TenantId").**
Already fixed in this codebase (`TenantConfiguration.Property(t =>
t.Id).ValueGeneratedNever()`), but worth understanding if you ever hit
something like it again with your own seed data: EF Core's default
convention for a `Guid` primary key auto-generates a new value whenever
the property is still `Guid.Empty` at save time, because it can't tell
"forgot to set this" apart from "deliberately using the empty Guid".
`ApplicationDbContextInitializer` deliberately sets a reserved Tenant's
`Id` to `Guid.Empty` (the sentinel `TeamMember.TenantId` uses for
`PlatformSuperAdmin`, who isn't scoped to any real tenant - see
`Domain.Common.ITenantScoped`) - without `ValueGeneratedNever()`, EF
Core silently swaps that in for a random Guid on the way to the
database, the `TeamMembers` insert right after it (correctly holding
`TenantId = Guid.Empty`) then finds no matching `Tenants` row, and the
foreign key check fails. If you hit this on a database that already has
a bad partial-seed row from before this fix existed, drop and recreate
the dev database (`dotnet ef database drop ... --force`, see "Running it
locally") rather than trying to patch the row by hand - it's dev/seed
data either way, and the leftover row would also collide with the
unique `Slug` constraint on retry.

## Notes, assumptions, and deviations from the spec

- **This code was not compiled or `dotnet restore`-d in the environment
  that produced it** - no NuGet/container-registry network access here.
  I traced every EF Core / MediatR / Hangfire / ASP.NET extension method
  used back to the package that provides it (plain class libraries don't
  get the shared-framework references `Microsoft.NET.Sdk.Web` gets for
  free) and fixed two real design bugs during review (a duplicate-key
  race in ticket numbering from an earlier iteration of this project, and
  the RLS/login chicken-and-egg problem described above) - but please run
  `dotnet restore && dotnet build` yourself as the first step.
- **Email uniqueness**: the spec's schema says `TeamMember.Email` is
  "unique per tenant" (a person could work for two different Service
  Providers). I made it **globally unique** instead, because `POST
  /auth/login` as specified takes only `{ email, password }` with no
  tenant selector, which only resolves to one account if email is
  globally unique.
- **`Mailbox.MailboxId` on Ticket** is nullable, not the spec's literal
  "not null" - a Manual or Api-sourced ticket has no inbound mailbox to
  attribute.
- **"Reopened"** from the lifecycle diagram isn't a persisted `Status`
  value here - seeded modeling choice, see "Ticket lifecycle" above.
- **Omnichannel** is email-only by design (see "Email ingestion" above) -
  the spec's own design principle rules out a customer-facing web-form or
  chat surface.
- **Alerts** are logged + written to `Notification` rows, not sent
  anywhere real yet. Swap `IAlertNotifier`/`IEmailSender` in
  `Infrastructure/DependencyInjection.cs` for real providers.
- **Reports** (Phase 6) and **Knowledge base/CSAT/billing** (Phase 7) are
  out of scope here, consistent with the spec's own "MVP → phased
  roadmap" framing - `GetVolumeReport`/`GetResponseTimesReport` are
  implemented with real (if basic) aggregation; the rest isn't.
- **`TicketNumberGenerator`** draws from one shared Postgres sequence
  across all tenants (prefixed per-tenant for readability) rather than a
  sequence per tenant - simpler, still safe under concurrency.
- No AutoMapper - DTOs use plain static `FromEntity(...)` factory
  methods, to keep the dependency list (and the risk of a
  package-version mismatch I can't verify here) small.
