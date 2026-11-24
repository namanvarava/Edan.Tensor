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

## 3. Approved table mapping

The following mapping is the canonical physical-name target. The current name
is retained only during the compatibility migration window.

| Area | Current table | Approved physical table |
|---|---|---|
| Platform | platform_tenants | edpf_tnt |
| Platform | platform_organizations | edpf_org |
| Platform | platform_projects | edpf_prj |
| Platform | platform_users | edpf_usr |
| Platform | platform_tenant_memberships | edpf_tntmbr |
| Platform | platform_organization_memberships | edpf_orgmbr |
| Platform | platform_project_memberships | edpf_prjmbr |
| Platform | platform_resources | edpf_rsrc |
| Platform | platform_policies | edpf_plcy |
| Platform | platform_authorization_policies | edpf_authplcy |
| Platform | platform_authorization_audit | edpf_authaud |
| Platform | platform_authorization_policy_changes | edpf_authplcy_chg |
| Platform | platform_authorization_audit_retention | edpf_authaud_rtn |
| Platform | platform_authorization_audit_retention_changes | edpf_authaud_rtnchg |
| Platform | platform_authorization_audit_archive_jobs | edpf_authaud_arcjob |
| Platform | platform_authorization_audit_archive_attempts | edpf_authaud_arcattm |
| Platform | platform_authorization_audit_archive_schedules | edpf_authaud_arcschd |
| Platform | platform_storage_quota_policies | edpf_strqta_plcy |
| Platform | platform_storage_quota_policy_changes | edpf_strqta_plcychg |
| DataSet | datasets | edds_dset |
| DataSet | dataset_schema_revisions | edds_dsetschm_rev |
| DataSet | dataset_storage_usage_snapshots | edds_dsetstr_usgsnp |
| DataSet | dataset_quota_threshold_events | edds_dsetqta_threvt |
| DataSet | dataset_validation_profiles | edds_dsetvld_prf |
| DataSet | dataset_validation_profile_revisions | edds_dsetvld_prfrev |
| DataSet | dataset_validation_executions | edds_dsetvld_exc |
| DataSet | dataset_validation_details | edds_dsetvld_dtl |
| DataSet | dataset_validation_seen_keys | edds_dsetvld_seenkey |
| DataSetContent | DataSetContent | eddc_dsetctt |
| Worker | worker_tasks | edwk_wkrtsk |
| Worker | worker_task_replays | edwk_wkrrpl |
| Worker | worker_task_types | edwk_wkrtyp |
| Worker | external_deliveries | edwk_wkrdlv |

The platform binding, routine allowlist, and mutation audit tables are added by
the current application migrations and must be included in the same mapping
review before the rename migration is generated.

| Platform | platform_tenant_database_bindings | edpf_tntdbnd |
| Platform | platform_tenant_database_routine_allowlists | edpf_tntrtnalw |
| Platform | platform_tenant_database_mutation_audits | edpf_tntmut_aud |

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

For an operation that targets more than one database binding, every mutation
step must include the persisted `bindingId`. The same identifier may be used
for a compensation step. This prevents a later default-binding change from
rerouting a retry or compensation to a different provider. The legacy form
without `bindingId` remains supported for a tenant with one default active
binding. Consistency checks use the same binding identifiers, so PostgreSQL,
SQL Server, Oracle, and SQLite results can be compared for the same tenant.

## 5. Consistency policy

Consistency checks compare allowlisted, read-only routine results using a stable
business key and normalized values. Results are classified as `Consistent`,
`Inconsistent`, or `Unknown`. Differences include the tenant, provider, work
identifier, expected value, observed value, and retry/compensation status.
