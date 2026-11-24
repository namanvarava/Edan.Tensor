# EDAN.Tensor Database Naming Standard

Status: Approved project baseline

This document is the implementation baseline for the database naming, migration,
provider mapping, and consistency work. It does not change a live database by
itself. Physical schema changes are applied only by reviewed provider migrations.

## 1. Naming rules

- Physical identifiers use lowercase English letters, digits, and underscores.
- Domain abbreviations use the approved two-to-four character dictionary terms.
- Table names use the `ed<area>_<business>` form where the dictionary defines a
  product table identifier. Existing compatibility names remain readable during
  the expand/migrate/switch/contract window.
- Column names use an explicit business prefix or semantic suffix. Bare names
  such as `id`, `code`, and `name` are not introduced for new schema objects.
- UTC timestamps use the `*_utc` suffix.
- Primary keys use `<table>_PK`; non-primary indexes use `<table>_N01`,
  `<table>_N02`, and so on.
- Foreign-key names use `<table>_<referenced-table>_FK` when a provider supports
  the full identifier length; provider-specific shortening must be documented.

## 2. Approved dictionary terms

| Meaning | Full name | Abbreviation | Examples |
|---|---|---|---|
| Tenant | Tenant | tnt | tntid, tntcd, tntnm, tntst |
| Organization | Organization | org | orgid, orgcd, orgnm |
| Project | Project | pjt | pjtid, pjtcd, pjtnm |
| User | User | usr | usrid, eml, dspnm |
| Membership | Membership | mbr | tntmbrid, orgmbrid, prjmbrid |
| Authentication subject | Subject | sub | subid, sub_orgnm |
| Dataset | DataSet | dset | dsetid, dsetnm, dsetst |
| Worker | Worker | wkr | wkrid, wkrst |
| Task | Task | tsk | tskid, tskst |
| Provider | Provider | prv | prvcd, prvnm |
| Status | Status | st | tntst, dsetst, wkrst |
| Created UTC | Created UTC | crt_utc | crt_utc |
| Updated UTC | Updated UTC | upd_utc | upd_utc |

The canonical project membership identifier is `prjmbrid` in code and migration
specifications.

## 3. Compatibility policy

- C# property names and public JSON names remain unchanged unless a separately
  versioned API contract is approved.
- EF Core Fluent mappings define the physical names.
- Existing data is preserved. No destructive drop-and-recreate migration is
  allowed for a populated database.
- Every rename is implemented as expand, copy/backfill, switch, verify, then
  contract.
- SQLite, PostgreSQL, SQL Server, and Oracle receive equivalent logical models;
  provider-specific types and identifier limits are handled in each migration.

## 4. Multi-provider transaction policy

Cross-database work is coordinated by the application. Each provider uses its
own local transaction. The coordinator records step state, retries transient
failures, applies idempotency keys, and invokes a compensation step when the
overall operation cannot complete. A distributed two-phase commit is not used.

## 5. Consistency policy

Consistency checks compare allowlisted, read-only routine results using a stable
business key and normalized values. Results are classified as `Consistent`,
`Inconsistent`, or `Unknown`. Differences include the tenant, provider, work
identifier, expected value, observed value, and retry/compensation status.
