# Platform Storage Quota Governance

Storage quota policies are Platform metadata, not DataSet persistence records. Each policy belongs to one Tenant and targets exactly one Tenant, Organization, Project, or DataSet identifier. The unique key is Tenant + scope + target, and policy changes are preserved in a separate append-only audit table.

The Platform database stores policy state and audit history. ApiHost implements the DataSet quota-provider port and translates active Platform policies into the DataSet Application contract. DataSet Domain/Application remain independent of Platform assemblies.

Policy records use optimistic concurrency (Version). Tenant foreign keys use restrictive deletion. Audit records reference both policy and Tenant through a composite key so a policy change cannot be attached to another Tenant.

PlanCode는 정책에 저장되어 계획별 기본값을 연결할 수 있습니다. 현재 단계는 Tenant 관리자가 정책을 페이지 단위로 조회·생성·수정·활성화하고 변경 이력을 조회하는 API까지 제공합니다. 계획 카탈로그에서 한도를 자동으로 해석하는 기능은 Commercial 계약이 승인된 뒤 별도 단계에서 구현합니다.
