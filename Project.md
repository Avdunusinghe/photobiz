# Photographer Business Website — Project Spec (.NET Stack)

## Overview

A three-part system for a photography business:

1. **Public website** — portfolio, contact, booking flow (SEO-critical)
2. **Admin web app** — gallery management, booking management, business operations (no SEO needed)
3. **API backend** — shared by both frontends, self-hosted (not serverless)

This version uses a directly-hosted stack (no containerization) — suited for a VPS/Windows Server/IIS deployment, similar to your existing FOF platform deployment pattern.

---

## Tech Stack

| Layer         | Choice                           | Notes                                            |
| ------------- | -------------------------------- | ------------------------------------------------ |
| Public site   | Next.js (App Router)             | SSR/SSG/ISR for SEO                              |
| Admin app     | Angular                          | Standalone SPA, no SEO requirement               |
| API           | .NET (Clean Architecture)        | CQRS/MediatR pattern, same style as FOF platform |
| Database      | MSSQL (SQL Server)               | Relational data — bookings, galleries, clients   |
| ORM           | EF Core                          | Code-first migrations                            |
| Auth          | JWT + bcrypt (custom)            | Self-issued tokens, refresh token rotation       |
| Image storage | AWS S3                           | Buckets for original + resized image variants    |
| Hosting       | IIS / Windows Server (or any VM) | Direct deployment, no container runtime required |

---

## Architecture

```
┌─────────────────┐     ┌──────────────────┐
│  Next.js Public  │     │  Angular Admin    │
│  (yourdomain.com)│     │ (admin.yourdomain)│
└────────┬─────────┘     └─────────┬─────────┘
         │                          │
         └──────────┬───────────────┘
                     │  REST calls (HTTPS)
                     ▼
         ┌───────────────────────────┐
         │   .NET API (Clean Arch)    │
         │   api.yourdomain.com       │
         │   Hosted on IIS / VM       │
         └───────────┬─────────────────┘
                     │  EF Core
                     ▼
         ┌───────────────────────┐        ┌──────────────┐
         │   MSSQL Server          │        │   AWS S3      │
         │   (same VM or           │        │  (image files)│
         │    managed RDS)         │        └──────────────┘
         └───────────────────────┘
```

The .NET API is published and deployed directly to IIS (or run as a Windows Service / Kestrel behind a reverse proxy), the same deployment pattern you already use for the FOF platform — no container runtime needed.

---

## Solution Structure (.NET Clean Architecture)

```
/src
  /PhotoBiz.Domain            → Entities, enums, domain logic, no dependencies
  /PhotoBiz.Application        → CQRS commands/queries (MediatR), DTOs, interfaces, validation (FluentValidation)
  /PhotoBiz.Infrastructure      → EF Core DbContext, repositories, S3 service, email service, JWT service
  /PhotoBiz.API                 → Controllers, middleware, DI wiring, Swagger, Program.cs
/tests
  /PhotoBiz.Application.Tests
  /PhotoBiz.Infrastructure.Tests
```

Dependency direction: `API → Application → Domain`, `Infrastructure → Application → Domain` (Infrastructure implements interfaces defined in Application).

---

## Domain Model

Core entities:

- `AdminUser` — `Id`, `Email`, `PasswordHash`, `PasswordSalt`, `Role`, `RefreshToken`, `RefreshTokenExpiry`
- `Client` — people who book sessions: `Id`, `Name`, `Email`, `Phone`
- `Gallery` — a shoot/collection: `Id`, `Title`, `Slug`, `Description`, `IsPublished`, `CreatedByAdminId`
- `Photo` — belongs to one `Gallery`: `Id`, `GalleryId`, `ThumbnailUrl`, `MediumUrl`, `FullUrl`, `AltText`, `SortOrder`
- `SessionType` — e.g. "Portrait", "Wedding": `Id`, `Name`, `Description`, `DurationMinutes`, `Price`
- `Booking` — belongs to one `Client` and one `SessionType`: `Id`, `ClientId`, `SessionTypeId`, `RequestedDate`, `Status` (enum: `Pending`, `Confirmed`, `Paid`, `Cancelled`), `Notes`

Relationships:

- `Client` 1—many `Booking`
- `Gallery` 1—many `Photo`
- `SessionType` 1—many `Booking`
- `AdminUser` 1—many `Gallery`

---

## CQRS Pattern (matches FOF platform conventions)

Each feature organized as Command/Query + Handler, e.g.:

```
/Application
  /Galleries
    /Commands
      CreateGalleryCommand.cs
      CreateGalleryCommandHandler.cs
      PublishGalleryCommand.cs
    /Queries
      GetGalleryBySlugQuery.cs
      GetGalleryBySlugQueryHandler.cs
  /Bookings
    /Commands
      CreateBookingCommand.cs
      CreateBookingCommandHandler.cs   ← must check slot availability inside a transaction
      UpdateBookingStatusCommand.cs
    /Queries
      GetBookingsByDateRangeQuery.cs
```

Use two-phase validation pattern (consistent with your FOF work): FluentValidation for input shape, handler-level validation for business rules (e.g. no double-booking same date/slot).

---

## Auth: JWT + bcrypt (rolling your own)

Since this stack skips a managed auth provider, implement:

1. **Registration/seed**: admin user created via seed script or one-time setup endpoint — hash password with `BCrypt.Net-Next` before storing
2. **Login endpoint** (`POST /api/auth/login`): verify password with `BCrypt.Verify`, issue short-lived access JWT (~15 min) + long-lived refresh token (stored hashed in DB, ~7-30 days)
3. **Refresh endpoint** (`POST /api/auth/refresh`): validates refresh token, issues new access token, rotates refresh token
4. **Middleware**: `[Authorize]` attribute on admin-only controllers/endpoints, JWT bearer validation configured in `Program.cs`
5. **Password requirements**: enforce minimum complexity server-side (length, not on a blocklist) — do this validation in the Application layer command validator, not the controller

Security notes:

- Store JWT signing key in environment variable / secrets manager (or `appsettings.Production.json` excluded from source control), never in source
- Use `HttpOnly`, `Secure`, `SameSite=Strict` cookies for refresh token if feasible, rather than local storage, to reduce XSS token theft risk
- Rate-limit the login endpoint to reduce brute-force risk

---

## Image Storage: AWS S3

Flow:

1. Admin uploads photo via Angular → sent to .NET API
2. API uses `ImageSharp` (or similar) to generate thumbnail/medium/full-size variants
3. All three variants uploaded to S3 via AWS SDK for .NET (`AWSSDK.S3`)
4. Resulting S3 URLs (or CloudFront-fronted URLs, recommended for CDN caching) saved to `Photo` table via EF Core

Bucket structure suggestion:

```
s3://photobiz-images/
  galleries/{galleryId}/thumb-{photoId}.webp
  galleries/{galleryId}/medium-{photoId}.webp
  galleries/{galleryId}/full-{photoId}.webp
```

Recommend: put **CloudFront** in front of the S3 bucket for CDN delivery + to avoid exposing raw S3 URLs, and to enable caching/compression at the edge.

---

## Deployment

Given no containerization, deployment follows a traditional publish-and-copy (or CI/CD-driven) pattern:

- **.NET API**: `dotnet publish` → deploy output to IIS site (or run as a Kestrel process behind IIS/Nginx as reverse proxy) — same pattern as your FOF platform's IIS deployment, can reuse existing CodeBuild/CodeDeploy/CodePipeline knowledge if hosting on AWS EC2 with IIS
- **MSSQL**: either installed directly on the same VM (simplest, lowest cost) or a managed instance (AWS RDS for SQL Server) for production reliability/backups
- **Next.js public site**: deploy separately to Vercel (recommended, keeps SEO/ISR benefits) or self-host via `next start` on a Node-capable VM/IIS with iisnode
- **Angular admin app**: build (`ng build --configuration production`) and deploy static output to IIS as a site, or host alongside the API

Suggested environment split:

- **QA**: single VM running API + MSSQL, manual or pipeline-triggered deploys
- **LIVE**: API on IIS (App Pool per environment, matching your existing FOF conventions), MSSQL as managed RDS instance for backup/reliability

---

## Environment Variables / App Settings

### .NET API (`appsettings.json` / environment variables)

```
ConnectionStrings__DefaultConnection=
Jwt__SigningKey=
Jwt__AccessTokenExpiryMinutes=15
Jwt__RefreshTokenExpiryDays=30
AWS__AccessKey=
AWS__SecretKey=
AWS__Region=
AWS__BucketName=
Cors__AllowedOrigins=https://yourdomain.com,https://admin.yourdomain.com
```

### Next.js public site

```
NEXT_PUBLIC_API_URL=https://api.yourdomain.com
```

### Angular admin app

```
apiUrl: 'https://api.yourdomain.com'
```

---

## SEO Requirements (Next.js public site)

- Metadata API for per-page title/description/OG tags
- Gallery pages statically generated (SSG) or ISR — revalidate when admin publishes new photos via a webhook/revalidation endpoint call from the .NET API
- `schema.org` structured data: `LocalBusiness` + `Photographer` on homepage, `ImageObject` on gallery photos
- `sitemap.xml` and `robots.txt`
- Descriptive `alt` text pulled from `Photo.AltText`
- Next.js `<Image>` component for lazy-loading/responsive sizing, pointed at CloudFront-fronted S3 URLs

---

## Build Order (suggested phases)

1. **Phase 1 — Foundation**: Solution scaffold (Clean Architecture layers), MSSQL instance provisioned (local or dev VM), EF Core initial migration, health check endpoint, basic IIS/publish pipeline verified end-to-end.
2. **Phase 2 — Auth**: JWT + bcrypt implementation, login/refresh endpoints, `[Authorize]` middleware, seed initial admin user.
3. **Phase 3 — Admin core**: Gallery CRUD, photo upload with S3 + resizing, admin Angular app wired to API.
4. **Phase 4 — Public site**: Homepage, gallery listing/detail pages (SSG/ISR), SEO metadata, contact form.
5. **Phase 5 — Booking system**: Session types, booking creation with transactional availability check, admin booking management, email notifications.
6. **Phase 6 — Deployment & polish**: Production IIS/VM hosting finalized, CloudFront in front of S3, Lighthouse/performance pass, structured data, monitoring/logging.

---

## Hosting Cost Note

This stack has **real ongoing infrastructure cost** once deployed to production (no free serverless tier):

- VM/VPS hosting (EC2, Lightsail, or similar) running IIS — smallest viable instance typically $5-15/month
- MSSQL: run on the same VM to minimize cost early on, or managed RDS for SQL Server (~$15-30/month) once reliability/backups matter more
- S3 + CloudFront — usage-based, low cost at small scale (a few dollars/month for a portfolio site)

This is a reasonable trade-off given it matches your professional stack directly (transferable skills, same IIS/AWS deployment patterns as FOF), but worth setting expectations that it isn't a $0 stack like the earlier Next.js/Express/Supabase proposal.
