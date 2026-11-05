# Platform Storage Quota Governance

Storage quota policies are Platform metadata, not DataSet persistence records. Each policy belongs to one Tenant and targets exactly one Tenant, Organization, Project, or DataSet identifier. The unique key is Tenant + scope + target, and policy changes are preserved in a separate append-only audit table.

The Platform database stores policy state and audit history. ApiHost implements the DataSet quota-provider port and translates active Platform policies into the DataSet Application contract. DataSet Domain/Application remain independent of Platform assemblies.

Policy records use optimistic concurrency (Version). Tenant foreign keys use restrictive deletion. Audit records reference both policy and Tenant through a composite key so a policy change cannot be attached to another Tenant.

PlanCode is persisted with the policy for plan-aware defaults. This phase stores the plan association; catalog-driven plan limit resolution and management endpoints are the next implementation stage.
