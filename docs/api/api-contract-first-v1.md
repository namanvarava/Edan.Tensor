# EDAN.Tensor API Contract-First v1

## Compatibility

Existing `/api/platform/*`, `/health`, and `/health/database` routes remain unchanged. New module routes are additive and use `/api/v1/`.

Every module route requires a valid bearer token and an `X-Tenant-Id` header equal to the `tenantId` query/route value. A mismatch returns `400` with `tenant.context_mismatch`.

## Groups

| Group | Endpoint | Purpose | Required permission |
|---|---|---|---|
| System v1 | `GET/POST /api/v1/system/menus` | Tenant system menu catalog | `system.menu.read` / `system.menu.write` |
| Master v1 | `GET/POST /api/v1/master/codes` | Tenant common-code catalog | `master.code.read` / `master.code.write` |
| Business v1 | `GET/POST /api/v1/business/work-items` | Tenant business work intake | `business.work.read` / `business.work.write` |

The current development implementation applies the authenticated route boundary, requires active Tenant membership for reads, requires Tenant administrator membership for writes, and uses an in-memory adapter. A persistent provider adapter must be supplied before production use; the API contract and application interfaces do not expose a DbContext or provider type.

## Request conventions

- `tenantId` is a GUID query parameter for v1 collection routes.
- `page` defaults to `1`; `pageSize` defaults to `50` and is capped at `200`.
- `search`, module-specific filters, and `sort` are optional.
- JSON property names use the existing web serializer policy.
- Errors use `{ "code": "...", "description": "..." }` where the existing exception pipeline applies.

## OpenAPI and versioning

Development exposes `/openapi/v1.json` and Swagger UI. New endpoints have explicit OpenAPI tags and stable operation names. v2 must be additive or introduced under a new route; existing response shapes and status codes must not be changed.

## Client package

`EDAN.Tensor.Client.Contracts` contains v1 DTOs and `EDAN.Tensor.Client` contains typed read/write methods. The generated package artifacts use version `1.0.0`. Client package versioning is independent from server route compatibility; a package may consume v1 without referencing Framework DbContext or provider assemblies.
