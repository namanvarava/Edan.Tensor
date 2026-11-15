# EDAN.Tensor 데이터베이스 명명 규칙과 전자사전

## 1. 문서 목적

이 문서는 EDAN.Tensor의 로컬 SQLite 데이터베이스 이름, 테이블 이름, 컬럼 이름, 기본 키와 인덱스 이름을 일관되게 관리하기 위한 권장 기준을 정의합니다. 문서의 용어는 국제적으로 널리 사용되는 OIDC, OAuth 2.0, RBAC, UUID, UTC, JSON, URI, SHA-256 용어와 일치하도록 구성합니다.

현재 데이터베이스의 설명은 각 SQLite 파일에 있는 `__schema_comments` 메타데이터 테이블에도 저장되어 있습니다. 이 문서는 새 스키마를 설계할 때 사용하는 기준이며, 기존 API와 Migration의 호환성을 깨는 일괄 이름 변경을 의미하지 않습니다.

## 2. 데이터베이스 파일과 책임

데이터베이스 파일명은 현재 이름을 그대로 유지합니다. 파일명은 운영 설정과 로컬 실행 안내에서 이미 사용되므로, 파일명을 바꾸려면 연결 문자열·백업·복구·배포 문서를 함께 변경해야 합니다.

| 파일명 | 영어 풀네임 | 약어 | 한글명 | 현재 개발 파일 위치 | 현재 용도 |
|---|---|---|---|---|---|
| `edan-tensor-platform.db` | EDAN Tensor Platform Database | Platform DB | 플랫폼 데이터베이스 | `src/Hosts/EDAN.Tensor.ApiHost/App_Data/edan-tensor-platform.db` | Tenant, 조직, 프로젝트, 사용자, RBAC 정책, 권한 감사, 저장소 한도를 관리합니다. |
| `edan-tensor-dataset.db` | EDAN Tensor DataSet Metadata Database | DataSet DB | 데이터셋 메타데이터 데이터베이스 | `src/Hosts/EDAN.Tensor.ApiHost/App_Data/edan-tensor-dataset.db` | DataSet 기본 정보, 스키마 버전, 검증 프로필, 검증 실행과 결과를 관리합니다. |
| `edan-tensor-dataset-content.db` | EDAN Tensor DataSet Content Database | Content DB | 데이터셋 콘텐츠 데이터베이스 | `src/Hosts/EDAN.Tensor.ApiHost/App_Data/edan-tensor-dataset-content.db` | DataSet 콘텐츠 본문과 Tenant·조직·프로젝트 범위의 저장 참조를 관리합니다. |
| `edan-worker.db` | EDAN Tensor Worker Database | Worker DB | 워커 데이터베이스 | `src/Hosts/EDAN.Tensor.ApiHost/edan-worker.db` | 작업 유형, 작업 큐, Lease, 재시도, 재실행, 외부 전달 상태를 관리합니다. |

네 데이터베이스는 논리적 책임을 분리하지만, Tenant 식별자와 조직·프로젝트 범위는 서로 같은 계약을 사용해야 합니다. 어느 데이터베이스도 다른 Tenant의 데이터를 조회하거나 저장하면 안 됩니다.

## 3. 명명 규칙

### 3.1 테이블

- 새 테이블은 영문 소문자 `lower_snake_case`를 사용합니다.
- 테이블 이름은 일반적으로 복수형 명사로 작성합니다. 예를 들어 `platform_tenants`, `worker_tasks`를 사용합니다.
- 테이블 이름은 기능 영역 접두사를 포함합니다. Platform은 `platform_`, DataSet은 `dataset_`, Worker는 `worker_`를 사용합니다.
- 여러 영역에서 공유되는 외부 전달 테이블은 기존 계약인 `external_deliveries`를 유지합니다. 새 공유 테이블은 책임 영역을 먼저 검토한 후 접두사를 결정합니다.
- 예약어, 공백, 대문자, 의미 없는 약어, 물리 DB마다 다른 이름은 사용하지 않습니다.
- `DataSetContent`는 현재 어댑터와 Migration이 사용하는 기존 호환성 이름입니다. 새 테이블에서는 `dataset_contents`를 권장하지만, 기존 이름은 별도 승인된 Migration 없이 변경하지 않습니다.

### 3.2 컬럼

- 새 컬럼은 영문 소문자 `lower_snake_case`를 사용합니다.
- 식별자는 `id`, 외래 식별자는 `{대상}_id`를 사용합니다. 예를 들어 `tenant_id`, `organization_id`, `project_id`를 사용합니다.
- 컬럼명을 줄여 쓸 때는 전자사전에 등록된 국제 표준 약어 또는 제품 전체에서 승인된 약어만 사용합니다. 임의로 만든 첫 글자 조합은 사용하지 않습니다.
- 승인된 약어를 조합할 때는 의미가 사라지지 않도록 조합 순서를 유지합니다. 예를 들어 Organization Name은 `orgnm`, Subject Organization Name은 `sub_orgnm`으로 작성합니다.
- 시각은 의미와 표준을 함께 표시합니다. UTC 시각은 `{event}_at_utc`, Unix 밀리초는 `{event}_at_unix_milliseconds`를 사용합니다.
- Boolean 값은 `is_`, `has_`, `can_` 중 의미에 맞는 접두사를 사용합니다.
- JSON 문서는 `_json` 접미사를 사용합니다. URI는 `_uri`, 해시는 `_hash`, 바이트 수량은 `_bytes`, 백분율은 `_percent`를 사용합니다.
- 상태와 역할은 자유로운 문자열을 추가하지 않고 해당 도메인의 승인된 열거값을 사용합니다.
- 기존 Migration이 만든 PascalCase 컬럼과 일부 `organization_id`, `project_id`, `tenant_id` 혼용 이름은 현재 호환성 이름으로 취급합니다. 신규 Migration에서 이름을 변경하려면 데이터 변환과 롤백 절차를 함께 제공해야 합니다.

### 3.3 기본 키와 인덱스

- 기본 키 이름은 `{table_name}_PK`로 작성합니다. 예를 들어 `platform_tenants_PK`를 사용합니다.
- 일반 인덱스 이름은 `{table_name}_N01`, `{table_name}_N02`와 같이 작성합니다.
- 유일 인덱스도 같은 번호 체계를 사용하고, 유일성은 Migration 정의에서 명시합니다.
- 인덱스 번호는 테이블별로 01부터 증가시키며, 삭제한 번호를 다른 의미로 재사용하지 않습니다.
- Tenant 범위를 포함하는 조회는 가능하면 인덱스의 앞쪽에 `tenant_id`를 배치합니다.
- Lease와 Worker 큐 조회는 상태와 다음 실행 시각을 함께 고려하고, Idempotency 조회는 Tenant와 idempotency key를 함께 인덱싱합니다.
- 현재 EF Core가 생성한 `sqlite_autoindex_*`와 `IX_*` 이름은 기존 Migration의 결과입니다. 이번 설명 추가 작업에서는 이를 직접 변경하지 않았습니다.

### 3.4 데이터 형식

- UUID는 문자열 또는 UUID 계약으로 저장하되, API·Tenant·외부 주체에서 같은 표현을 사용합니다.
- UTC 시각은 ISO 8601 형식으로 기록합니다.
- 금액·용량·횟수는 의미에 맞는 정수 또는 소수 형식을 사용하고, 바이트 수량은 음수가 되지 않도록 검증합니다.
- 바이너리 콘텐츠는 콘텐츠 데이터베이스에만 저장하고, 메타데이터 데이터베이스에는 저장 참조와 범위 식별자만 저장합니다.

## 4. 권장 전자사전

전자사전은 영문 풀네임, 약어, 한글명을 함께 기록합니다. 영문 풀네임을 그대로 사용하지 않을 때에는 세계적으로 통용되는 약어를 우선 사용하고, 국제적으로 통용되는 약어가 없으면 임의의 첫 글자 조합을 만들지 않습니다.

약어는 아래 사전에 등록된 값만 사용합니다. `org`, `sub`, `id`, `uri`, `uuid`, `utc`, `json`, `sha256`은 국제 표준이나 국제 개발 관행에서 널리 사용되는 값입니다. `nm`은 이번 EDAN.Tensor 명명 계약에서 Name에 대해 승인한 제품 약어이므로 `orgnm`, `sub_orgnm` 예시에만 사용하고 다른 단어로 확대하지 않습니다. 약어를 새로 만들려면 영문 풀네임, 약어의 국제적 사용 근거, 한글명, 적용 범위와 Migration 영향을 문서에 기록하고 승인해야 합니다.

| 영문 풀네임 | 약어 | 한글명 | 사용 기준 |
|---|---|---|---|
| Tenant | Tenant | 테넌트 | 데이터와 권한의 최상위 격리 범위입니다. |
| Organization | Org | 조직 | Tenant 안에서 프로젝트를 묶는 범위입니다. |
| Organization Name | Org Name / `orgnm` | 조직 이름 | 조직의 표시 이름을 나타냅니다. |
| Project | Project | 프로젝트 | 조직 안에서 작업과 리소스를 묶는 범위입니다. |
| DataSet | DataSet | 데이터셋 | 구조화된 데이터의 논리 단위입니다. |
| DataSet Content | Content | 데이터셋 콘텐츠 | DataSet의 실제 콘텐츠 본문입니다. |
| User | User | 사용자 | 제품 내부 사용자 프로필입니다. |
| Subject | Sub | 인증 주체 | IdP가 발급한 안정적인 `sub` 식별자입니다. |
| Subject Organization Name | `sub_orgnm` | 인증 주체 조직 이름 | 인증 주체와 조직 이름을 결합해 표현하는 제품 계약명입니다. |
| Membership | Membership | 소속 | 주체와 Tenant·조직·프로젝트 사이의 역할 관계입니다. |
| Role Based Access Control | RBAC | 역할 기반 접근 제어 | 역할을 통해 최소 권한을 적용하는 모델입니다. |
| Authorization Policy | AuthZ Policy | 권한 정책 | 요청을 허용하거나 거부하는 정책입니다. |
| Authorization Audit | AuthZ Audit | 권한 감사 | 권한 판정과 근거를 기록하는 이력입니다. |
| Storage Quota | Quota | 저장소 한도 | Tenant와 하위 범위의 저장 용량 정책입니다. |
| Validation | Validation | 검증 | DataSet의 구조와 값 규칙을 검사하는 기능입니다. |
| Validation Profile | Profile | 검증 프로필 | 검증 규칙의 논리 단위입니다. |
| Validation Execution | Execution | 검증 실행 | 한 번 수행한 검증 작업의 실행 단위입니다. |
| Validation Detail | Detail | 검증 상세 | 검증에서 발견한 개별 오류 또는 경고입니다. |
| Schema | Schema | 스키마 | DataSet의 필드와 형식 정의입니다. |
| Schema Revision | Revision | 스키마 개정 | 스키마의 버전별 보존본입니다. |
| Worker Task | Task | 워커 작업 | Worker가 실행하는 작업 단위입니다. |
| Task Type | Task Type | 작업 유형 | Worker가 처리할 작업 종류입니다. |
| Task Replay | Replay | 작업 재실행 | 기존 작업에서 생성한 재실행 작업입니다. |
| External Delivery | Delivery | 외부 전달 | 외부 수신 시스템에 전달하는 메시지입니다. |
| Lease | Lease | 임대 | Worker가 작업을 처리할 수 있는 일시적인 소유권입니다. |
| Fencing Token | Fencing Token | 펜싱 토큰 | 오래된 Worker가 쓰기를 수행하지 못하게 하는 단조 증가 값입니다. |
| Idempotency Key | Idempotency Key | 멱등성 키 | 같은 요청의 중복 처리를 막는 키입니다. |
| Checkpoint | Checkpoint | 체크포인트 | 작업 재개를 위한 진행 위치입니다. |
| JSON Document | JSON | JSON 문서 | 구조화된 설정과 결과를 저장하는 문서입니다. |
| Uniform Resource Identifier | URI | 균일 자원 식별자 | 외부 대상이나 저장 위치를 식별하는 문자열입니다. |
| Universally Unique Identifier | UUID | 범용 고유 식별자 | 분산 환경에서 충돌을 줄이는 식별자입니다. |
| Coordinated Universal Time | UTC | 협정 세계시 | 서버와 데이터베이스가 사용하는 기준 시간입니다. |
| Secure Hash Algorithm 256 | SHA-256 | SHA-256 해시 | 무결성 검증에 사용하는 해시 알고리즘입니다. |

### 4.2 약어 조합 예시

| 의미 | 영문 구성 | 약어 조합 | 권장 명칭 |
|---|---|---|---|
| 조직 이름 | Organization + Name | `org` + `nm` | `orgnm` |
| 인증 주체 조직 이름 | Subject + Organization + Name | `sub` + `_` + `org` + `nm` | `sub_orgnm` |
| Tenant 식별자 | Tenant + Identifier | `tenant` + `_` + `id` | `tenant_id` |
| 프로젝트 식별자 | Project + Identifier | `project` + `_` + `id` | `project_id` |

`orgnm`과 `sub_orgnm`은 전자사전에서 승인된 조합의 예시입니다. 동일한 의미를 가진 컬럼을 다른 표기인 `organization_name`, `org_name`, `subject_org_name`으로 혼용하지 않습니다. 다만 기존 Migration과 공개 API가 이미 사용하는 이름은 호환성을 위해 유지합니다.

### 4.1 공통 컬럼 전자사전

| 영문 풀네임 | 약어 | 한글명 | 권장 컬럼명 | 현재 의미 |
|---|---|---|---|---|
| Identifier | ID | 식별자 | `id` | 레코드를 고유하게 식별합니다. |
| Tenant Identifier | Tenant ID | 테넌트 식별자 | `tenant_id` | 레코드가 속한 Tenant를 식별합니다. |
| Organization Identifier | Org ID | 조직 식별자 | `organization_id` | 레코드가 속한 조직을 식별합니다. |
| Project Identifier | Project ID | 프로젝트 식별자 | `project_id` | 레코드가 속한 프로젝트를 식별합니다. |
| Subject Identifier | Sub | 인증 주체 식별자 | `subject_id` | IdP의 안정적인 `sub` 값을 저장합니다. |
| Created At Coordinated Universal Time | Created At UTC | 생성 UTC 시각 | `created_at_utc` | 레코드 생성 시각을 저장합니다. |
| Updated At Coordinated Universal Time | Updated At UTC | 변경 UTC 시각 | `updated_at_utc` | 마지막 변경 시각을 저장합니다. |
| Occurred At Coordinated Universal Time | Occurred At UTC | 발생 UTC 시각 | `occurred_at_utc` | 이벤트 발생 시각을 저장합니다. |
| Completed At Coordinated Universal Time | Completed At UTC | 완료 UTC 시각 | `completed_at_utc` | 작업 완료 시각을 저장합니다. |
| Status | Status | 상태 | `status` | 레코드의 현재 상태를 저장합니다. |
| Role | Role | 역할 | `role` | 범위 안에서 주체에게 부여된 역할을 저장합니다. |
| Enabled Flag | Enabled | 활성화 여부 | `is_enabled` | 기능이나 정책의 활성화 여부를 저장합니다. |
| Version | Version | 버전 | `version` | 낙관적 동시성 제어 버전을 저장합니다. |
| Idempotency Key | Idempotency Key | 멱등성 키 | `idempotency_key` | 중복 요청을 식별합니다. |
| Payload Reference | Payload Reference | 본문 참조 | `payload_reference` | 실제 작업 본문 또는 저장 위치를 참조합니다. |
| Result Reference | Result Reference | 결과 참조 | `result_reference` | 작업 결과의 저장 위치를 참조합니다. |
| Error Code | Error Code | 오류 코드 | `last_error_code` | 마지막 실패 원인을 기계 판독 가능한 코드로 저장합니다. |
| JSON Document | JSON | JSON 문서 | `*_json` | 구조화된 정의나 결과를 저장합니다. |
| Byte Count | Bytes | 바이트 수 | `*_bytes` | 저장 용량이나 본문 크기를 저장합니다. |
| Percentage | Percent | 백분율 | `*_percent` | 사용률이나 임계값을 저장합니다. |

## 5. 현재 테이블과 사용 목적

### 5.1 Platform DB

| 테이블 | 사용 목적 |
|---|---|
| `platform_tenants` | Tenant의 코드, 이름, 상태와 생성 정보를 저장합니다. |
| `platform_organizations` | Tenant 안의 조직을 저장합니다. |
| `platform_projects` | 조직과 Tenant에 속한 프로젝트를 저장합니다. |
| `platform_users` | Tenant에 연결된 사용자 프로필을 저장합니다. |
| `platform_tenant_memberships` | Tenant 소속과 Tenant 범위 역할을 저장합니다. |
| `platform_organization_memberships` | 조직 소속과 조직 범위 역할을 저장합니다. |
| `platform_project_memberships` | 프로젝트 소속과 프로젝트 범위 역할을 저장합니다. |
| `platform_resources` | Tenant·조직·프로젝트가 소유한 리소스를 저장합니다. |
| `platform_policies` | Platform 설정 정책과 범위를 저장합니다. |
| `platform_authorization_policies` | RBAC와 범위 기반 권한 정책을 저장합니다. |
| `platform_authorization_audit` | 권한 판정 결과와 판정 근거를 저장합니다. |
| `platform_authorization_policy_changes` | 권한 정책 변경 이력을 저장합니다. |
| `platform_authorization_audit_retention` | 권한 감사 보존 정책을 저장합니다. |
| `platform_authorization_audit_retention_changes` | 보존 정책 변경 이력을 저장합니다. |
| `platform_authorization_audit_archive_jobs` | 감사 로그 아카이브 작업 상태를 저장합니다. |
| `platform_authorization_audit_archive_attempts` | 아카이브 작업별 시도 결과를 저장합니다. |
| `platform_authorization_audit_archive_schedules` | Tenant별 아카이브 실행 일정을 저장합니다. |
| `platform_storage_quota_policies` | Tenant·조직·프로젝트의 저장소 한도를 저장합니다. |
| `platform_storage_quota_policy_changes` | 저장소 한도 변경 이력을 저장합니다. |

### 5.2 DataSet DB

| 테이블 | 사용 목적 |
|---|---|
| `datasets` | DataSet의 이름, 상태, 버전, 저장 어댑터와 범위를 저장합니다. |
| `dataset_schema_revisions` | DataSet 스키마의 버전별 JSON 정의를 저장합니다. |
| `dataset_storage_usage_snapshots` | Tenant별 저장소 사용량의 시점별 스냅샷을 저장합니다. |
| `dataset_quota_threshold_events` | 저장소 한도 초과와 회복 이벤트를 저장합니다. |
| `dataset_validation_profiles` | 검증 프로필의 기본 정보를 저장합니다. |
| `dataset_validation_profile_revisions` | 검증 프로필의 버전별 JSON 정의를 저장합니다. |
| `dataset_validation_executions` | 검증 실행 상태, 집계, 멱등성, 펜싱 정보를 저장합니다. |
| `dataset_validation_details` | 검증에서 발견한 개별 오류와 경고를 저장합니다. |
| `dataset_validation_seen_keys` | 검증 중복 검사를 위한 키 해시를 저장합니다. |

### 5.3 DataSet Content DB

| 테이블 | 사용 목적 |
|---|---|
| `DataSetContent` | DataSet 콘텐츠 본문과 Tenant·조직·프로젝트 범위를 연결합니다. 현재 어댑터 호환성 이름이므로 즉시 변경하지 않습니다. |
| `__schema_comments` | SQLite가 제공하지 않는 테이블·컬럼 설명을 보관합니다. 애플리케이션 업무 데이터가 아닙니다. |

### 5.4 Worker DB

| 테이블 | 사용 목적 |
|---|---|
| `worker_tasks` | 작업 상태, Lease, 재시도, 진행률, 펜싱 토큰을 저장합니다. |
| `worker_task_types` | 처리 가능한 작업 유형과 활성화 여부를 저장합니다. |
| `worker_task_replays` | 원본 작업과 재실행 작업의 관계를 저장합니다. |
| `external_deliveries` | 외부 수신 시스템 전달, 재시도, 멱등성 상태를 저장합니다. |

각 EF Core 데이터베이스에는 Migration 관리용 `__EFMigrationsHistory`가 있을 수 있고, Migration 동시성 제어가 활성화된 경우 `__EFMigrationsLock`이 있을 수 있습니다. 이 두 테이블은 프레임워크 관리 테이블이므로 제품 업무 테이블과 다르게 취급합니다.

### 5.4.1 2026-11-15 확정 명세 적용 기록

`outputs/edan-schema-final-20261115/edan-tensor-schema-dictionary-final.xlsx`를 승인된 검토본으로 확정했습니다. 이 파일에는 전자사전, 테이블 목록, 컬럼 명세, 인덱스 명세, 변경 영향, 33개 제품 테이블별 명세 시트와 테이블 시트 목록이 포함됩니다. 컬럼의 사용자 확정명, 자료유형, PK, FK, NULL 허용 여부와 전자사전 근거는 현재 SQLite Migration·스키마와 대조해 기록했습니다.

이번 확정은 논리 명세와 문서의 확정입니다. 현행 EF Core Migration과 공개 API의 호환성을 보존하기 위해 `Id`, `TenantId`, `DataSetContent` 등 기존 물리 이름은 유지합니다. 권장 lower_snake_case 이름을 실제 SQLite·PostgreSQL·SQL Server·Oracle 물리 이름으로 변경하는 작업은 별도 Migration 승인 없이는 수행하지 않습니다. 해당 변경이 승인되면 데이터 변환, PK·인덱스 재생성, 기존 데이터 건수·해시, Tenant 격리, API·Worker, 백업·복구와 네 DB 회귀 검증을 별도 수용 기준으로 실행합니다.

로컬 실행과 테스트에서 생성되는 `*.log`, `*.err.log`, `*.out.log` 파일은 `.gitignore`에 등록하여 저장소 병합에서 제외했습니다. 로그 파일은 장애 분석에 필요할 수 있으므로 로컬에서는 삭제하지 않고, 보존 기간과 민감정보 포함 여부를 확인한 뒤 운영 정책에 따라 관리합니다.

## 5.5 테이블별 컬럼 명세

다음 명세는 현재 EF Core Migration과 개발용 SQLite 파일에 실제로 반영된 컬럼을 기준으로 작성했습니다. `Id`, `TenantId`와 같이 대문자가 포함된 이름은 현재 공개 API와 Migration 호환성을 위해 유지된 물리 컬럼명입니다. 신규 테이블을 만들 때는 3장의 `lower_snake_case` 규칙을 적용합니다.

### 5.5.1 형식 표기

| 표기 | 의미 |
|---|---|
| `UUID/TEXT` | UUID를 문자열로 저장하는 식별자입니다. |
| `TEXT` | 코드, 이름, 상태, 역할, JSON, URI 또는 설명 문자열입니다. |
| `INTEGER` | 정수, 개수, 버전, Unix 밀리초 또는 Boolean 저장값입니다. |
| `BOOLEAN` | SQLite에서는 보통 `INTEGER` 0/1로 저장되는 논리값입니다. |
| `UTC` | UTC 기준 시각을 문자열 또는 DateTime 변환값으로 저장합니다. |
| `BLOB` | SQLite에 저장하는 바이너리 본문입니다. |
| `FK` | 다른 테이블의 식별자를 참조하는 논리 외래 키입니다. SQLite 파일을 분리하므로 물리 외래 키가 없는 경우에도 애플리케이션이 관계를 검증합니다. |

### 5.5.2 `edan-tensor-platform.db` 명세

이 파일은 제품의 소유권과 권한 경계를 결정합니다. API가 `TenantId`, 조직, 프로젝트와 인증 주체를 함께 검사할 때 사용하는 기준 데이터베이스입니다.

| 테이블 | 컬럼별 명세 |
|---|---|
| `platform_tenants` | `Id` (UUID/TEXT, PK): Tenant 식별자입니다.<br>`Code` (TEXT, unique): API와 운영자가 사용하는 Tenant 코드입니다.<br>`Name` (TEXT): 표시 이름입니다.<br>`Status` (TEXT): Tenant 상태입니다.<br>`CreatedAtUtc` (UTC): 생성 시각입니다. |
| `platform_organizations` | `Id` (UUID/TEXT, PK): 조직 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 소속 Tenant입니다.<br>`Code` (TEXT): Tenant 안에서 사용하는 조직 코드입니다.<br>`Name` (TEXT): 조직 표시 이름입니다.<br>`Status` (TEXT): 조직 상태입니다. |
| `platform_projects` | `Id` (UUID/TEXT, PK): 프로젝트 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 소속 Tenant입니다.<br>`OrganizationId` (UUID/TEXT, FK): 소속 조직입니다.<br>`Code` (TEXT): 조직 안에서 사용하는 프로젝트 코드입니다.<br>`Name` (TEXT): 프로젝트 표시 이름입니다.<br>`Status` (TEXT): 프로젝트 상태입니다. |
| `platform_users` | `Id` (UUID/TEXT, PK): 제품 사용자 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 사용자 프로필의 Tenant 범위입니다.<br>`Email` (TEXT): 연락용 이메일입니다. 인증 주체의 고유 식별자로 사용하지 않습니다.<br>`DisplayName` (TEXT): 표시 이름입니다.<br>`Status` (TEXT): 사용자 상태입니다. |
| `platform_tenant_memberships` | `Id` (UUID/TEXT, PK): 소속 레코드 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 소속 Tenant입니다.<br>`SubjectId` (TEXT, FK 논리 관계): OIDC 토큰의 안정적인 `sub` 값입니다.<br>`Role` (TEXT): Tenant 범위 역할입니다.<br>`Status` (TEXT): 소속 상태입니다. |
| `platform_organization_memberships` | `Id` (UUID/TEXT, PK): 조직 소속 레코드 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 상위 Tenant입니다.<br>`OrganizationId` (UUID/TEXT, FK): 조직 식별자입니다.<br>`SubjectId` (TEXT): 인증 주체의 안정적인 `sub` 값입니다.<br>`Role` (TEXT): 조직 범위 역할입니다.<br>`Status` (TEXT): 소속 상태입니다. |
| `platform_project_memberships` | `Id` (UUID/TEXT, PK): 프로젝트 소속 레코드 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 상위 Tenant입니다.<br>`OrganizationId` (UUID/TEXT, FK): 상위 조직입니다.<br>`ProjectId` (UUID/TEXT, FK): 프로젝트 식별자입니다.<br>`SubjectId` (TEXT): 인증 주체의 안정적인 `sub` 값입니다.<br>`Role` (TEXT): 프로젝트 범위 역할입니다.<br>`Status` (TEXT): 소속 상태입니다. |
| `platform_resources` | `Id` (UUID/TEXT, PK): 리소스 식별자입니다.<br>`tenant_id` (UUID/TEXT, FK): 소유 Tenant입니다.<br>`organization_id` (UUID/TEXT, FK, nullable): 소유 조직입니다.<br>`project_id` (UUID/TEXT, FK, nullable): 소유 프로젝트입니다.<br>`ResourceType` (TEXT): 리소스 종류입니다.<br>`Name` (TEXT): 리소스 이름입니다.<br>`Status` (TEXT): 리소스 상태입니다. |
| `platform_policies` | `Id` (UUID/TEXT, PK): 정책 식별자입니다.<br>`tenant_id` (UUID/TEXT, FK): 정책 Tenant 범위입니다.<br>`organization_id` (UUID/TEXT, FK, nullable): 정책 조직 범위입니다.<br>`project_id` (UUID/TEXT, FK, nullable): 정책 프로젝트 범위입니다.<br>`Key` (TEXT): 정책 이름 또는 키입니다.<br>`Value` (TEXT): 정책 값입니다.<br>`Version` (INTEGER): 정책 버전입니다.<br>`IsEnabled` (BOOLEAN): 정책 활성화 여부입니다. |
| `platform_authorization_policies` | `Id` (UUID/TEXT, PK): 권한 정책 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 정책 Tenant 범위입니다.<br>`OrganizationId` (UUID/TEXT, FK, nullable): 조직 범위입니다.<br>`ProjectId` (UUID/TEXT, FK, nullable): 프로젝트 범위입니다.<br>`SubjectId` (TEXT, nullable): 특정 인증 주체에 한정할 때 사용하는 `sub` 값입니다.<br>`Action` (TEXT): 수행하려는 작업입니다.<br>`ResourceType` (TEXT): 대상 리소스 종류입니다.<br>`Effect` (TEXT): 허용 또는 거부 효과입니다.<br>`Priority` (INTEGER): 정책 평가 우선순위입니다.<br>`IsEnabled` (BOOLEAN): 정책 활성화 여부입니다.<br>`CreatedAtUtc` (UTC): 정책 생성 시각입니다. |
| `platform_authorization_audit` | `Id` (UUID/TEXT, PK): 감사 레코드 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 판정 Tenant 범위입니다.<br>`SubjectId` (TEXT): 판정 대상 인증 주체입니다.<br>`Action` (TEXT): 요청 작업입니다.<br>`ResourceType` (TEXT): 요청 대상 종류입니다.<br>`Scope` (TEXT): 판정 범위입니다.<br>`IsAllowed` (BOOLEAN): 최종 허용 여부입니다.<br>`ReasonCode` (TEXT): 판정 이유 코드입니다.<br>`MatchedPolicyId` (UUID/TEXT, nullable): 적용된 정책 식별자입니다.<br>`OccurredAtUtc` (UTC): 판정 발생 시각입니다. |
| `platform_authorization_audit_retention` | `Id` (UUID/TEXT, PK): 보존 정책 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 보존 정책 Tenant입니다.<br>`RetentionDays` (INTEGER): 감사 기록 보존 일수입니다.<br>`IsLegalHold` (BOOLEAN): 법적 보존 상태입니다.<br>`LegalHoldReason` (TEXT, nullable): 법적 보존 사유입니다.<br>`UpdatedBy` (TEXT): 변경 주체입니다.<br>`UpdatedAtUtc` (UTC): 변경 시각입니다.<br>`Version` (INTEGER): 정책 버전입니다. |
| `platform_authorization_audit_retention_changes` | `Id` (UUID/TEXT, PK): 변경 이력 식별자입니다.<br>`PolicyId` (UUID/TEXT, FK): 변경된 보존 정책입니다.<br>`TenantId` (UUID/TEXT, FK): 변경 Tenant입니다.<br>`ActorSubjectId` (TEXT): 변경을 수행한 `sub` 값입니다.<br>`ChangeType` (TEXT): 변경 종류입니다.<br>`BeforeJson` (TEXT/JSON): 변경 전 정책입니다.<br>`AfterJson` (TEXT/JSON): 변경 후 정책입니다.<br>`OccurredAtUtc` (UTC): 변경 발생 시각입니다. |
| `platform_authorization_policy_changes` | `Id` (UUID/TEXT, PK): 변경 이력 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 변경 Tenant입니다.<br>`PolicyId` (UUID/TEXT, FK): 변경된 권한 정책입니다.<br>`ActorSubjectId` (TEXT): 변경 주체의 `sub` 값입니다.<br>`ChangeType` (TEXT): 변경 종류입니다.<br>`BeforeJson` (TEXT/JSON): 변경 전 정책입니다.<br>`AfterJson` (TEXT/JSON): 변경 후 정책입니다.<br>`OccurredAtUtc` (UTC): 변경 발생 시각입니다. |
| `platform_authorization_audit_archive_jobs` | `Id` (UUID/TEXT, PK): 아카이브 작업 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 아카이브 Tenant입니다.<br>`FromUtc`, `ToUtc` (UTC): 아카이브 대상 시간 범위입니다.<br>`RequestedBy` (TEXT): 요청 주체입니다.<br>`CreatedAtUtc` (UTC): 작업 생성 시각입니다.<br>`Status` (TEXT): 작업 상태입니다.<br>`ArchiveId` (TEXT, nullable): 생성된 아카이브 식별자입니다.<br>`RecordCount` (INTEGER): 아카이브 레코드 수입니다.<br>`Sha256` (TEXT, nullable): 아카이브 무결성 해시입니다.<br>`SizeBytes` (INTEGER, nullable): 아카이브 크기입니다.<br>`CompletedAtUtc` (UTC, nullable): 완료 시각입니다.<br>`FailureReason` (TEXT, nullable): 실패 사유입니다.<br>`AttemptCount` (INTEGER): 시도 횟수입니다.<br>`Version` (INTEGER): 동시성 버전입니다. |
| `platform_authorization_audit_archive_attempts` | `Id` (UUID/TEXT, PK): 아카이브 시도 식별자입니다.<br>`JobId` (UUID/TEXT, FK): 대상 아카이브 작업입니다.<br>`TenantId` (UUID/TEXT, FK): 시도 Tenant입니다.<br>`AttemptNumber` (INTEGER): 시도 순번입니다.<br>`Operation` (TEXT): 수행한 작업입니다.<br>`StartedAtUtc`, `FinishedAtUtc` (UTC): 시도 시작·종료 시각입니다.<br>`IsSuccessful` (BOOLEAN): 시도 성공 여부입니다.<br>`ResultCode` (TEXT): 결과 코드입니다. |
| `platform_authorization_audit_archive_schedules` | `Id` (UUID/TEXT, PK): 스케줄 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 스케줄 Tenant입니다.<br>`IntervalHours` (INTEGER): 실행 간격입니다.<br>`IsEnabled` (BOOLEAN): 스케줄 활성화 여부입니다.<br>`NextRunAtUtc`, `LastRunAtUtc` (UTC, nullable): 다음·마지막 실행 시각입니다.<br>`LeaseId` (TEXT, nullable): 실행 Lease 식별자입니다.<br>`LeaseExpiresAtUtc` (UTC, nullable): Lease 만료 시각입니다.<br>`Version` (INTEGER): 동시성 버전입니다. |
| `platform_storage_quota_policies` | `Id` (UUID/TEXT, PK): 한도 정책 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 정책 Tenant입니다.<br>`Scope` (TEXT): Tenant·조직·프로젝트·DataSet 범위입니다.<br>`TargetId` (UUID/TEXT, nullable): 범위 대상 식별자입니다.<br>`TenantLimitBytes` (INTEGER): Tenant 한도입니다.<br>`OrganizationLimitBytes` (INTEGER, nullable): 조직 한도입니다.<br>`ProjectLimitBytes` (INTEGER, nullable): 프로젝트 한도입니다.<br>`DataSetLimitBytes` (INTEGER, nullable): DataSet 한도입니다.<br>`WarningThresholdPercent` (INTEGER): 경고 임계값입니다.<br>`PlanCode` (TEXT): 적용 요금제 또는 계획 코드입니다.<br>`IsEnabled` (BOOLEAN): 정책 활성화 여부입니다.<br>`Version` (INTEGER): 정책 버전입니다.<br>`UpdatedBy` (TEXT): 변경 주체입니다.<br>`UpdatedAtUtc` (UTC): 변경 시각입니다. |
| `platform_storage_quota_policy_changes` | `Id` (UUID/TEXT, PK): 변경 이력 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 변경 Tenant입니다.<br>`PolicyId` (UUID/TEXT, FK): 변경된 한도 정책입니다.<br>`ActorSubjectId` (TEXT): 변경 주체의 `sub` 값입니다.<br>`ChangeType` (TEXT): 변경 종류입니다.<br>`BeforeJson` (TEXT/JSON): 변경 전 정책입니다.<br>`AfterJson` (TEXT/JSON): 변경 후 정책입니다.<br>`OccurredAtUtc` (UTC): 변경 시각입니다. |

### 5.5.3 `edan-tensor-dataset.db` 명세

이 파일은 DataSet 자체와 검증 결과를 관리합니다. 실제 콘텐츠 본문은 저장하지 않고, 콘텐츠 파일의 위치와 범위는 별도 Content DB에서 관리합니다.

| 테이블 | 컬럼별 명세 |
|---|---|
| `datasets` | `Id` (UUID/TEXT, PK): DataSet 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 소유 Tenant입니다.<br>`OrganizationId` (UUID/TEXT, FK): 소유 조직입니다.<br>`ProjectId` (UUID/TEXT, FK): 소유 프로젝트입니다.<br>`Name` (TEXT): DataSet 이름입니다.<br>`StorageReference` (TEXT): 콘텐츠 저장 위치를 식별하는 참조값입니다.<br>`StorageAdapter` (TEXT): SQLite·Parquet·DuckDB 등 저장 어댑터 식별자입니다.<br>`ContentFormat` (TEXT): JSON, Parquet 등 콘텐츠 형식입니다.<br>`Status` (TEXT): DataSet 상태입니다.<br>`CreatedAtUtc` (UTC): 생성 시각입니다.<br>`Version` (INTEGER): DataSet 변경 버전입니다. |
| `dataset_schema_revisions` | `DataSetId` (UUID/TEXT, PK 일부, FK): 대상 DataSet입니다.<br>`Version` (INTEGER, PK 일부): 스키마 버전입니다.<br>`SchemaJson` (TEXT/JSON): 해당 버전의 스키마 정의입니다.<br>`CreatedAtUtc` (UTC): 스키마 버전 생성 시각입니다. |
| `dataset_storage_usage_snapshots` | `Id` (INTEGER, PK): 스냅샷 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 집계 Tenant입니다.<br>`PeriodStartUtc` (UTC): 집계 기간 시작 시각입니다.<br>`CapturedAtUtc` (UTC): 스냅샷 생성 시각입니다.<br>`UsedBytes` (INTEGER): 사용한 바이트 수입니다.<br>`DataSetCount` (INTEGER): 집계 DataSet 수입니다. |
| `dataset_quota_threshold_events` | `Id` (INTEGER, PK): 임계값 이벤트 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 이벤트 Tenant입니다.<br>`PolicyId` (UUID/TEXT, FK): 적용된 한도 정책입니다.<br>`Scope` (TEXT): 이벤트 범위입니다.<br>`TargetId` (UUID/TEXT, nullable): 범위 대상입니다.<br>`EventType` (TEXT): 초과·경고·회복 등 이벤트 종류입니다.<br>`UsedBytes` (INTEGER): 이벤트 당시 사용량입니다.<br>`LimitBytes` (INTEGER): 이벤트 당시 한도입니다.<br>`UtilizationPercent` (INTEGER): 사용률입니다.<br>`PolicyVersion` (INTEGER): 적용 정책 버전입니다.<br>`OccurredAtUtc` (UTC): 이벤트 발생 시각입니다. |
| `dataset_validation_profiles` | `Id` (UUID/TEXT, PK): 검증 프로필 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 프로필 Tenant입니다.<br>`Name` (TEXT): 프로필 이름입니다.<br>`CreatedAtUtc` (UTC): 프로필 생성 시각입니다. |
| `dataset_validation_profile_revisions` | `ProfileId` (UUID/TEXT, PK 일부, FK): 검증 프로필입니다.<br>`TenantId` (UUID/TEXT, FK): 프로필 Tenant입니다.<br>`Revision` (INTEGER, PK 일부): 프로필 개정 번호입니다.<br>`DefinitionJson` (TEXT/JSON): 검증 규칙 정의입니다.<br>`CreatedAtUtc` (UTC): 개정 생성 시각입니다. |
| `dataset_validation_executions` | `Id` (UUID/TEXT, PK): 검증 실행 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 실행 Tenant입니다.<br>`DataSetId` (UUID/TEXT, FK): 검증 대상 DataSet입니다.<br>`ExpectedDataSetVersion` (INTEGER): 실행 시 기대한 DataSet 버전입니다.<br>`SchemaVersion` (INTEGER): 사용한 스키마 버전입니다.<br>`ProfileId` (UUID/TEXT, FK): 사용한 프로필입니다.<br>`ProfileRevision` (INTEGER): 사용한 프로필 개정입니다.<br>`Status` (TEXT): 검증 실행 상태입니다.<br>`DetailReference` (TEXT, nullable): 상세 결과 저장 참조입니다.<br>`ProcessedRecords` (INTEGER): 처리한 레코드 수입니다.<br>`TotalRecords` (INTEGER): 전체 레코드 수입니다.<br>`ErrorCount` (INTEGER): 오류 수입니다.<br>`WarningCount` (INTEGER): 경고 수입니다.<br>`DetailItemCount` (INTEGER): 상세 항목 수입니다.<br>`DetailTruncated` (BOOLEAN): 상세 결과가 제한되었는지 나타냅니다.<br>`RuleCountsJson` (TEXT/JSON): 규칙별 집계입니다.<br>`FailureCode`, `FailureMessage` (TEXT, nullable): 실패 코드와 설명입니다.<br>`CreatedAtUtc`, `CompletedAtUtc` (UTC): 시작·완료 시각입니다.<br>`Version` (INTEGER): 동시성 버전입니다.<br>`FencingToken` (INTEGER): 오래된 실행자의 쓰기를 차단하는 펜싱 값입니다.<br>`IdempotencyKey` (TEXT, nullable): 중복 실행 방지 키입니다. |
| `dataset_validation_details` | `Id` (UUID/TEXT, PK): 상세 결과 식별자입니다.<br>`ExecutionId` (UUID/TEXT, FK): 검증 실행입니다.<br>`TenantId` (UUID/TEXT, FK): 결과 Tenant입니다.<br>`DetailReference` (TEXT): 상세 결과 묶음 참조입니다.<br>`RowNumber` (INTEGER): 오류가 발생한 행 번호입니다.<br>`FieldName` (TEXT): 오류 필드명입니다.<br>`Code` (TEXT): 검증 규칙 코드입니다.<br>`Severity` (TEXT): 오류·경고 심각도입니다.<br>`Message` (TEXT): 사용자에게 제공할 설명입니다. |
| `dataset_validation_seen_keys` | `Id` (INTEGER, PK): 중복 검사 항목 식별자입니다.<br>`ExecutionId` (UUID/TEXT, FK): 검증 실행입니다.<br>`RuleCode` (TEXT): 중복 검사 규칙입니다.<br>`KeyHash` (TEXT): 중복 비교용 키 해시입니다. 원문 민감값을 저장하지 않습니다. |

### 5.5.4 `edan-tensor-dataset-content.db` 명세

이 파일은 DataSet의 실제 본문을 저장하는 영역입니다. 본문 레코드는 반드시 Tenant·조직·프로젝트·DataSet 범위를 함께 저장하여 잘못된 범위 조회를 방지합니다.

| 테이블 | 컬럼별 명세 |
|---|---|
| `DataSetContent` | `Id` (UUID/TEXT, PK): 콘텐츠 레코드 식별자입니다.<br>`StorageReference` (TEXT): 콘텐츠 저장 참조입니다.<br>`DataSetId` (UUID/TEXT, FK 논리 관계): 콘텐츠가 속한 DataSet입니다.<br>`TenantId` (UUID/TEXT, FK 논리 관계): 콘텐츠 Tenant 범위입니다.<br>`OrganizationId` (UUID/TEXT, FK 논리 관계): 콘텐츠 조직 범위입니다.<br>`ProjectId` (UUID/TEXT, FK 논리 관계): 콘텐츠 프로젝트 범위입니다.<br>`Content` (BLOB): 실제 콘텐츠 본문입니다. 현재 `DataSetContent`는 기존 코드와 Migration이 사용하는 호환성 이름입니다. |
| `__schema_comments` | `ObjectType` (TEXT): 설명 대상이 테이블인지 컬럼인지 나타냅니다.<br>`TableName` (TEXT): 설명 대상 테이블입니다.<br>`ColumnName` (TEXT, nullable): 컬럼 설명일 때 컬럼명입니다.<br>`Comment` (TEXT): 한국어 설명입니다. 이 테이블은 제품 업무 데이터가 아니라 SQLite 설명 메타데이터입니다. |

### 5.5.5 `edan-worker.db` 명세

이 파일은 비동기 작업의 상태와 소유권을 관리합니다. `LeaseId`, `LeaseExpiresAtUtc`, `FencingToken`, `Version`은 여러 Worker가 동시에 실행될 때 오래된 Worker가 결과를 덮어쓰지 못하게 하는 핵심 컬럼입니다.

| 테이블 | 컬럼별 명세 |
|---|---|
| `worker_tasks` | `Id` (UUID/TEXT, PK): 작업 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 작업 Tenant입니다.<br>`TaskType` (TEXT): 처리할 작업 유형입니다.<br>`IdempotencyKey` (TEXT): 중복 enqueue 방지 키입니다.<br>`PayloadReference` (TEXT): 작업 본문 참조입니다.<br>`Status` (TEXT): 대기·실행·완료·실패·Dead Letter 상태입니다.<br>`AttemptCount`, `MaxAttempts` (INTEGER): 현재·최대 시도 횟수입니다.<br>`LeaseId` (TEXT, nullable): 현재 작업 소유권입니다.<br>`WorkerId` (TEXT, nullable): 작업을 점유한 Worker 식별자입니다.<br>`FencingToken` (INTEGER): 작업 점유 세대 값입니다.<br>`LeaseExpiresAtUtc` (UTC, nullable): Lease 만료 시각입니다.<br>`Checkpoint` (TEXT, nullable): 재개 위치입니다.<br>`CheckpointVersion` (INTEGER): 체크포인트 버전입니다.<br>`CreatedAtUtc`, `CompletedAtUtc`, `NextAttemptAtUtc` (UTC): 생성·완료·다음 시도 시각입니다.<br>`LastErrorCode` (TEXT, nullable): 마지막 오류 코드입니다.<br>`Version` (INTEGER): 동시성 버전입니다.<br>`ProgressPercent` (INTEGER): 진행률입니다.<br>`ProgressStage` (TEXT, nullable): 현재 처리 단계입니다.<br>`ProgressMessage` (TEXT, nullable): 진행 설명입니다.<br>`ResultReference` (TEXT, nullable): 결과 저장 참조입니다.<br>`ReadyAtUtcUnixMilliseconds` (INTEGER): 큐 조회용 준비 시각의 Unix 밀리초 보조값입니다.<br>`LeaseExpiresAtUtcUnixMilliseconds` (INTEGER, nullable): 만료 작업 조회용 Unix 밀리초 보조값입니다.<br>`CreatedAtUtcUnixMilliseconds` (INTEGER): 정렬·페이징용 생성 시각의 Unix 밀리초 보조값입니다. 마지막 세 컬럼은 UTC 컬럼과 동기화되는 성능용 보조 컬럼입니다. |
| `worker_task_types` | `TaskType` (TEXT, PK): 작업 유형 코드입니다.<br>`Description` (TEXT): 작업 유형 설명입니다.<br>`IsEnabled` (BOOLEAN): 작업 유형 활성화 여부입니다.<br>`UpdatedAtUtc` (UTC): 변경 시각입니다.<br>`Version` (INTEGER): 동시성 버전입니다. |
| `worker_task_replays` | `Id` (UUID/TEXT, PK): 재실행 이력 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 재실행 Tenant입니다.<br>`OriginalTaskId` (UUID/TEXT, FK): 원본 작업입니다.<br>`ReplayTaskId` (UUID/TEXT, FK): 새로 생성된 재실행 작업입니다.<br>`RequestedBy` (TEXT): 재실행 요청 주체입니다.<br>`CreatedAtUtc` (UTC): 재실행 요청 시각입니다.<br>`ReplayRequestId` (UUID/TEXT, unique): 같은 재실행 요청의 중복 방지 키입니다. |
| `external_deliveries` | `Id` (UUID/TEXT, PK): 외부 전달 식별자입니다.<br>`TenantId` (UUID/TEXT, FK): 전달 Tenant입니다.<br>`TargetUri` (TEXT/URI): 외부 수신 시스템 주소입니다.<br>`Payload` (TEXT/JSON): 전송 본문입니다. 비밀값을 포함하지 않아야 합니다.<br>`IdempotencyKey` (TEXT): 외부 전달 중복 방지 키입니다.<br>`Status` (TEXT): 대기·전송·성공·실패 상태입니다.<br>`AttemptCount`, `MaxAttempts` (INTEGER): 현재·최대 전송 시도 횟수입니다.<br>`LastErrorCode` (TEXT, nullable): 마지막 전송 오류 코드입니다.<br>`CreatedAtUtc`, `CompletedAtUtc` (UTC, nullable): 생성·완료 시각입니다.<br>`Version` (INTEGER): 동시성 버전입니다. |

### 5.5.6 프레임워크 관리 테이블

| 테이블 | 사용 목적 | 관리 주체 |
|---|---|---|
| `__EFMigrationsHistory` | EF Core가 어느 Migration까지 적용했는지 기록합니다. | EF Core |
| `__EFMigrationsLock` | Migration을 동시에 실행하지 않도록 잠금을 관리합니다. | EF Core |

이 테이블들은 제품 기능의 업무 데이터가 아니므로 테이블 명명 규칙을 적용해 임의로 변경하거나 삭제하지 않습니다. 데이터베이스를 초기화할 때에는 해당 DB의 Migration을 다시 적용해야 합니다.

## 5.6 행 단위 컬럼 명세

앞 절은 DB 파일과 테이블별 목적을 빠르게 확인하기 위한 요약입니다. 실제 컬럼을 확인할 때에는 아래 행 단위 표를 기준으로 합니다. 한 행은 하나의 컬럼을 나타내며, `형식/제약`은 SQLite에 저장되는 대표 형식과 논리 제약을 함께 표시합니다. 대문자 컬럼명은 현재 Migration 호환성을 위해 유지하는 물리 이름이고, 신규 컬럼은 3장의 `lower_snake_case` 규칙을 사용합니다.

### 5.6.1 `edan-tensor-platform.db`

| 테이블명 | 컬럼명 | 형식/제약 | 용도와 주의사항 |
|---|---|---|---|
| `platform_tenants` | `Id` | UUID/TEXT, PK | Tenant를 고유하게 식별합니다. |
| `platform_tenants` | `Code` | TEXT, UNIQUE | API와 운영자가 사용하는 Tenant 코드입니다. |
| `platform_tenants` | `Name` | TEXT | Tenant 표시 이름입니다. |
| `platform_tenants` | `Status` | TEXT, 열거값 | Tenant 활성화 상태입니다. |
| `platform_tenants` | `CreatedAtUtc` | UTC | Tenant 생성 시각입니다. |
| `platform_organizations` | `Id` | UUID/TEXT, PK | 조직을 고유하게 식별합니다. |
| `platform_organizations` | `TenantId` | UUID/TEXT, FK 논리 관계 | 조직이 속한 Tenant입니다. |
| `platform_organizations` | `Code` | TEXT | Tenant 안에서 사용하는 조직 코드입니다. |
| `platform_organizations` | `Name` | TEXT | 조직 표시 이름입니다. 신규 약어 조합을 별도 승인할 때는 전자사전의 `orgnm` 계약을 검토합니다. |
| `platform_organizations` | `Status` | TEXT, 열거값 | 조직 활성화 상태입니다. |
| `platform_projects` | `Id` | UUID/TEXT, PK | 프로젝트를 고유하게 식별합니다. |
| `platform_projects` | `TenantId` | UUID/TEXT, FK 논리 관계 | 프로젝트가 속한 Tenant입니다. |
| `platform_projects` | `OrganizationId` | UUID/TEXT, FK 논리 관계 | 프로젝트가 속한 조직입니다. |
| `platform_projects` | `Code` | TEXT | 조직 안에서 사용하는 프로젝트 코드입니다. |
| `platform_projects` | `Name` | TEXT | 프로젝트 표시 이름입니다. |
| `platform_projects` | `Status` | TEXT, 열거값 | 프로젝트 활성화 상태입니다. |
| `platform_users` | `Id` | UUID/TEXT, PK | 제품 내부 사용자 프로필을 식별합니다. |
| `platform_users` | `TenantId` | UUID/TEXT, FK 논리 관계 | 사용자 프로필의 Tenant 범위입니다. |
| `platform_users` | `Email` | TEXT | 연락용 이메일이며 인증 주체의 고유 식별자로 사용하지 않습니다. |
| `platform_users` | `DisplayName` | TEXT | 화면에 표시할 사용자 이름입니다. |
| `platform_users` | `Status` | TEXT, 열거값 | 사용자 상태입니다. |
| `platform_tenant_memberships` | `Id` | UUID/TEXT, PK | Tenant 소속 레코드를 식별합니다. |
| `platform_tenant_memberships` | `TenantId` | UUID/TEXT, FK 논리 관계 | 소속 Tenant입니다. |
| `platform_tenant_memberships` | `SubjectId` | TEXT | OIDC 토큰의 안정적인 `sub` 값입니다. |
| `platform_tenant_memberships` | `Role` | TEXT, 열거값 | Tenant 범위 역할입니다. |
| `platform_tenant_memberships` | `Status` | TEXT, 열거값 | 소속 상태입니다. |
| `platform_organization_memberships` | `Id` | UUID/TEXT, PK | 조직 소속 레코드를 식별합니다. |
| `platform_organization_memberships` | `TenantId` | UUID/TEXT, FK 논리 관계 | 상위 Tenant입니다. |
| `platform_organization_memberships` | `OrganizationId` | UUID/TEXT, FK 논리 관계 | 소속 조직입니다. |
| `platform_organization_memberships` | `SubjectId` | TEXT | 인증 주체의 안정적인 `sub` 값입니다. |
| `platform_organization_memberships` | `Role` | TEXT, 열거값 | 조직 범위 역할입니다. |
| `platform_organization_memberships` | `Status` | TEXT, 열거값 | 조직 소속 상태입니다. |
| `platform_project_memberships` | `Id` | UUID/TEXT, PK | 프로젝트 소속 레코드를 식별합니다. |
| `platform_project_memberships` | `TenantId` | UUID/TEXT, FK 논리 관계 | 상위 Tenant입니다. |
| `platform_project_memberships` | `OrganizationId` | UUID/TEXT, FK 논리 관계 | 상위 조직입니다. |
| `platform_project_memberships` | `ProjectId` | UUID/TEXT, FK 논리 관계 | 소속 프로젝트입니다. |
| `platform_project_memberships` | `SubjectId` | TEXT | 인증 주체의 안정적인 `sub` 값입니다. |
| `platform_project_memberships` | `Role` | TEXT, 열거값 | 프로젝트 범위 역할입니다. |
| `platform_project_memberships` | `Status` | TEXT, 열거값 | 프로젝트 소속 상태입니다. |
| `platform_resources` | `Id` | UUID/TEXT, PK | 보호 대상 리소스를 식별합니다. |
| `platform_resources` | `tenant_id` | UUID/TEXT, FK 논리 관계 | 리소스 소유 Tenant입니다. |
| `platform_resources` | `organization_id` | UUID/TEXT, FK 논리 관계, NULL 허용 | 리소스 소유 조직입니다. |
| `platform_resources` | `project_id` | UUID/TEXT, FK 논리 관계, NULL 허용 | 리소스 소유 프로젝트입니다. |
| `platform_resources` | `ResourceType` | TEXT | 리소스 종류입니다. |
| `platform_resources` | `Name` | TEXT | 리소스 이름입니다. |
| `platform_resources` | `Status` | TEXT, 열거값 | 리소스 상태입니다. |
| `platform_policies` | `Id` | UUID/TEXT, PK | 일반 정책을 식별합니다. |
| `platform_policies` | `tenant_id` | UUID/TEXT, FK 논리 관계 | 정책 Tenant 범위입니다. |
| `platform_policies` | `organization_id` | UUID/TEXT, FK 논리 관계, NULL 허용 | 정책 조직 범위입니다. |
| `platform_policies` | `project_id` | UUID/TEXT, FK 논리 관계, NULL 허용 | 정책 프로젝트 범위입니다. |
| `platform_policies` | `Key` | TEXT | 정책 이름 또는 키입니다. |
| `platform_policies` | `Value` | TEXT | 정책 값입니다. |
| `platform_policies` | `Version` | INTEGER | 정책 동시성 버전입니다. |
| `platform_policies` | `IsEnabled` | BOOLEAN | 정책 활성화 여부입니다. |
| `platform_authorization_policies` | `Id` | UUID/TEXT, PK | 권한 정책을 식별합니다. |
| `platform_authorization_policies` | `TenantId` | UUID/TEXT, FK 논리 관계 | 정책 Tenant 범위입니다. |
| `platform_authorization_policies` | `OrganizationId` | UUID/TEXT, FK 논리 관계, NULL 허용 | 정책 조직 범위입니다. |
| `platform_authorization_policies` | `ProjectId` | UUID/TEXT, FK 논리 관계, NULL 허용 | 정책 프로젝트 범위입니다. |
| `platform_authorization_policies` | `SubjectId` | TEXT, NULL 허용 | 특정 인증 주체에 한정할 때 사용하는 `sub` 값입니다. |
| `platform_authorization_policies` | `Action` | TEXT | 수행하려는 작업입니다. |
| `platform_authorization_policies` | `ResourceType` | TEXT | 대상 리소스 종류입니다. |
| `platform_authorization_policies` | `Effect` | TEXT, 열거값 | 허용 또는 거부 효과입니다. |
| `platform_authorization_policies` | `Priority` | INTEGER | 정책 평가 우선순위입니다. |
| `platform_authorization_policies` | `IsEnabled` | BOOLEAN | 정책 활성화 여부입니다. |
| `platform_authorization_policies` | `CreatedAtUtc` | UTC | 정책 생성 시각입니다. |
| `platform_authorization_audit` | `Id` | UUID/TEXT, PK | 권한 감사 레코드를 식별합니다. |
| `platform_authorization_audit` | `TenantId` | UUID/TEXT, FK 논리 관계 | 판정 Tenant 범위입니다. |
| `platform_authorization_audit` | `SubjectId` | TEXT | 판정 대상 인증 주체입니다. |
| `platform_authorization_audit` | `Action` | TEXT | 요청 작업입니다. |
| `platform_authorization_audit` | `ResourceType` | TEXT | 요청 대상 종류입니다. |
| `platform_authorization_audit` | `Scope` | TEXT | 판정 범위입니다. |
| `platform_authorization_audit` | `IsAllowed` | BOOLEAN | 최종 허용 여부입니다. |
| `platform_authorization_audit` | `ReasonCode` | TEXT | 판정 이유 코드입니다. |
| `platform_authorization_audit` | `MatchedPolicyId` | UUID/TEXT, NULL 허용 | 적용된 정책 식별자입니다. |
| `platform_authorization_audit` | `OccurredAtUtc` | UTC | 판정 발생 시각입니다. |
| `platform_authorization_audit_retention` | `Id` | UUID/TEXT, PK | 감사 보존 정책을 식별합니다. |
| `platform_authorization_audit_retention` | `TenantId` | UUID/TEXT, FK 논리 관계 | 보존 정책 Tenant입니다. |
| `platform_authorization_audit_retention` | `RetentionDays` | INTEGER | 감사 기록 보존 일수입니다. |
| `platform_authorization_audit_retention` | `IsLegalHold` | BOOLEAN | 법적 보존 상태입니다. |
| `platform_authorization_audit_retention` | `LegalHoldReason` | TEXT, NULL 허용 | 법적 보존 사유입니다. |
| `platform_authorization_audit_retention` | `UpdatedBy` | TEXT | 변경 주체입니다. |
| `platform_authorization_audit_retention` | `UpdatedAtUtc` | UTC | 변경 시각입니다. |
| `platform_authorization_audit_retention` | `Version` | INTEGER | 정책 동시성 버전입니다. |
| `platform_authorization_audit_retention_changes` | `Id` | UUID/TEXT, PK | 보존 정책 변경 이력을 식별합니다. |
| `platform_authorization_audit_retention_changes` | `PolicyId` | UUID/TEXT, FK 논리 관계 | 변경된 보존 정책입니다. |
| `platform_authorization_audit_retention_changes` | `TenantId` | UUID/TEXT, FK 논리 관계 | 변경 Tenant입니다. |
| `platform_authorization_audit_retention_changes` | `ActorSubjectId` | TEXT | 변경을 수행한 `sub` 값입니다. |
| `platform_authorization_audit_retention_changes` | `ChangeType` | TEXT | 변경 종류입니다. |
| `platform_authorization_audit_retention_changes` | `BeforeJson` | TEXT/JSON | 변경 전 정책입니다. |
| `platform_authorization_audit_retention_changes` | `AfterJson` | TEXT/JSON | 변경 후 정책입니다. |
| `platform_authorization_audit_retention_changes` | `OccurredAtUtc` | UTC | 변경 발생 시각입니다. |
| `platform_authorization_policy_changes` | `Id` | UUID/TEXT, PK | 권한 정책 변경 이력을 식별합니다. |
| `platform_authorization_policy_changes` | `TenantId` | UUID/TEXT, FK 논리 관계 | 변경 Tenant입니다. |
| `platform_authorization_policy_changes` | `PolicyId` | UUID/TEXT, FK 논리 관계 | 변경된 권한 정책입니다. |
| `platform_authorization_policy_changes` | `ActorSubjectId` | TEXT | 변경 주체의 `sub` 값입니다. |
| `platform_authorization_policy_changes` | `ChangeType` | TEXT | 변경 종류입니다. |
| `platform_authorization_policy_changes` | `BeforeJson` | TEXT/JSON | 변경 전 정책입니다. |
| `platform_authorization_policy_changes` | `AfterJson` | TEXT/JSON | 변경 후 정책입니다. |
| `platform_authorization_policy_changes` | `OccurredAtUtc` | UTC | 변경 시각입니다. |
| `platform_authorization_audit_archive_jobs` | `Id` | UUID/TEXT, PK | 감사 아카이브 작업을 식별합니다. |
| `platform_authorization_audit_archive_jobs` | `TenantId` | UUID/TEXT, FK 논리 관계 | 아카이브 Tenant입니다. |
| `platform_authorization_audit_archive_jobs` | `FromUtc` | UTC | 아카이브 대상 시작 시각입니다. |
| `platform_authorization_audit_archive_jobs` | `ToUtc` | UTC | 아카이브 대상 종료 시각입니다. |
| `platform_authorization_audit_archive_jobs` | `RequestedBy` | TEXT | 요청 주체입니다. |
| `platform_authorization_audit_archive_jobs` | `CreatedAtUtc` | UTC | 작업 생성 시각입니다. |
| `platform_authorization_audit_archive_jobs` | `Status` | TEXT, 열거값 | 작업 상태입니다. |
| `platform_authorization_audit_archive_jobs` | `ArchiveId` | TEXT, NULL 허용 | 생성된 아카이브 식별자입니다. |
| `platform_authorization_audit_archive_jobs` | `RecordCount` | INTEGER | 아카이브 레코드 수입니다. |
| `platform_authorization_audit_archive_jobs` | `Sha256` | TEXT, NULL 허용 | 아카이브 무결성 해시입니다. |
| `platform_authorization_audit_archive_jobs` | `SizeBytes` | INTEGER, NULL 허용 | 아카이브 크기입니다. |
| `platform_authorization_audit_archive_jobs` | `CompletedAtUtc` | UTC, NULL 허용 | 완료 시각입니다. |
| `platform_authorization_audit_archive_jobs` | `FailureReason` | TEXT, NULL 허용 | 실패 사유입니다. |
| `platform_authorization_audit_archive_jobs` | `AttemptCount` | INTEGER | 시도 횟수입니다. |
| `platform_authorization_audit_archive_jobs` | `Version` | INTEGER | 동시성 버전입니다. |
| `platform_authorization_audit_archive_attempts` | `Id` | UUID/TEXT, PK | 아카이브 시도를 식별합니다. |
| `platform_authorization_audit_archive_attempts` | `JobId` | UUID/TEXT, FK 논리 관계 | 대상 아카이브 작업입니다. |
| `platform_authorization_audit_archive_attempts` | `TenantId` | UUID/TEXT, FK 논리 관계 | 시도 Tenant입니다. |
| `platform_authorization_audit_archive_attempts` | `AttemptNumber` | INTEGER | 시도 순번입니다. |
| `platform_authorization_audit_archive_attempts` | `Operation` | TEXT | 수행한 작업입니다. |
| `platform_authorization_audit_archive_attempts` | `StartedAtUtc` | UTC | 시도 시작 시각입니다. |
| `platform_authorization_audit_archive_attempts` | `FinishedAtUtc` | UTC, NULL 허용 | 시도 종료 시각입니다. |
| `platform_authorization_audit_archive_attempts` | `IsSuccessful` | BOOLEAN | 시도 성공 여부입니다. |
| `platform_authorization_audit_archive_attempts` | `ResultCode` | TEXT | 결과 코드입니다. |
| `platform_authorization_audit_archive_schedules` | `Id` | UUID/TEXT, PK | 아카이브 실행 예약을 식별합니다. |
| `platform_authorization_audit_archive_schedules` | `TenantId` | UUID/TEXT, FK 논리 관계 | 예약 Tenant입니다. |
| `platform_authorization_audit_archive_schedules` | `IntervalHours` | INTEGER | 실행 간격입니다. |
| `platform_authorization_audit_archive_schedules` | `IsEnabled` | BOOLEAN | 예약 활성화 여부입니다. |
| `platform_authorization_audit_archive_schedules` | `NextRunAtUtc` | UTC, NULL 허용 | 다음 실행 시각입니다. |
| `platform_authorization_audit_archive_schedules` | `LastRunAtUtc` | UTC, NULL 허용 | 마지막 실행 시각입니다. |
| `platform_authorization_audit_archive_schedules` | `LeaseId` | TEXT, NULL 허용 | 현재 실행 Lease 식별자입니다. |
| `platform_authorization_audit_archive_schedules` | `LeaseExpiresAtUtc` | UTC, NULL 허용 | 실행 Lease 만료 시각입니다. |
| `platform_authorization_audit_archive_schedules` | `Version` | INTEGER | 동시성 버전입니다. |
| `platform_storage_quota_policies` | `Id` | UUID/TEXT, PK | 저장소 한도 정책을 식별합니다. |
| `platform_storage_quota_policies` | `TenantId` | UUID/TEXT, FK 논리 관계 | 정책 Tenant입니다. |
| `platform_storage_quota_policies` | `Scope` | TEXT, 열거값 | Tenant·조직·프로젝트·DataSet 범위입니다. |
| `platform_storage_quota_policies` | `TargetId` | UUID/TEXT, NULL 허용 | 범위 대상 식별자입니다. |
| `platform_storage_quota_policies` | `TenantLimitBytes` | INTEGER | Tenant 한도입니다. |
| `platform_storage_quota_policies` | `OrganizationLimitBytes` | INTEGER, NULL 허용 | 조직 한도입니다. |
| `platform_storage_quota_policies` | `ProjectLimitBytes` | INTEGER, NULL 허용 | 프로젝트 한도입니다. |
| `platform_storage_quota_policies` | `DataSetLimitBytes` | INTEGER, NULL 허용 | DataSet 한도입니다. |
| `platform_storage_quota_policies` | `WarningThresholdPercent` | INTEGER | 경고 임계값입니다. |
| `platform_storage_quota_policies` | `PlanCode` | TEXT | 적용 요금제 또는 계획 코드입니다. |
| `platform_storage_quota_policies` | `IsEnabled` | BOOLEAN | 정책 활성화 여부입니다. |
| `platform_storage_quota_policies` | `Version` | INTEGER | 정책 버전입니다. |
| `platform_storage_quota_policies` | `UpdatedBy` | TEXT | 변경 주체입니다. |
| `platform_storage_quota_policies` | `UpdatedAtUtc` | UTC | 변경 시각입니다. |
| `platform_storage_quota_policy_changes` | `Id` | UUID/TEXT, PK | 한도 정책 변경 이력을 식별합니다. |
| `platform_storage_quota_policy_changes` | `TenantId` | UUID/TEXT, FK 논리 관계 | 변경 Tenant입니다. |
| `platform_storage_quota_policy_changes` | `PolicyId` | UUID/TEXT, FK 논리 관계 | 변경된 한도 정책입니다. |
| `platform_storage_quota_policy_changes` | `ActorSubjectId` | TEXT | 변경 주체의 `sub` 값입니다. |
| `platform_storage_quota_policy_changes` | `ChangeType` | TEXT | 변경 종류입니다. |
| `platform_storage_quota_policy_changes` | `BeforeJson` | TEXT/JSON | 변경 전 정책입니다. |
| `platform_storage_quota_policy_changes` | `AfterJson` | TEXT/JSON | 변경 후 정책입니다. |
| `platform_storage_quota_policy_changes` | `OccurredAtUtc` | UTC | 변경 시각입니다. |

### 5.6.2 `edan-tensor-dataset.db`

| 테이블명 | 컬럼명 | 형식/제약 | 용도와 주의사항 |
|---|---|---|---|
| `datasets` | `Id` | UUID/TEXT, PK | DataSet을 고유하게 식별합니다. |
| `datasets` | `TenantId` | UUID/TEXT, FK 논리 관계 | 소유 Tenant입니다. |
| `datasets` | `OrganizationId` | UUID/TEXT, FK 논리 관계 | 소유 조직입니다. |
| `datasets` | `ProjectId` | UUID/TEXT, FK 논리 관계 | 소유 프로젝트입니다. |
| `datasets` | `Name` | TEXT | DataSet 이름입니다. |
| `datasets` | `StorageReference` | TEXT | 콘텐츠 저장 위치를 식별하는 참조값입니다. |
| `datasets` | `StorageAdapter` | TEXT, 열거값 | SQLite·Parquet·DuckDB 등 저장 어댑터입니다. |
| `datasets` | `ContentFormat` | TEXT, 열거값 | JSON·Parquet 등 콘텐츠 형식입니다. |
| `datasets` | `Status` | TEXT, 열거값 | DataSet 상태입니다. |
| `datasets` | `CreatedAtUtc` | UTC | DataSet 생성 시각입니다. |
| `datasets` | `Version` | INTEGER | DataSet 변경 버전입니다. |
| `dataset_schema_revisions` | `DataSetId` | UUID/TEXT, PK 일부, FK 논리 관계 | 대상 DataSet입니다. |
| `dataset_schema_revisions` | `Version` | INTEGER, PK 일부 | 스키마 버전입니다. |
| `dataset_schema_revisions` | `SchemaJson` | TEXT/JSON | 해당 버전의 스키마 정의입니다. |
| `dataset_schema_revisions` | `CreatedAtUtc` | UTC | 스키마 버전 생성 시각입니다. |
| `dataset_storage_usage_snapshots` | `Id` | INTEGER, PK | 저장소 사용량 스냅샷을 식별합니다. |
| `dataset_storage_usage_snapshots` | `TenantId` | UUID/TEXT, FK 논리 관계 | 집계 Tenant입니다. |
| `dataset_storage_usage_snapshots` | `PeriodStartUtc` | UTC | 집계 기간 시작 시각입니다. |
| `dataset_storage_usage_snapshots` | `CapturedAtUtc` | UTC | 스냅샷 생성 시각입니다. |
| `dataset_storage_usage_snapshots` | `UsedBytes` | INTEGER | 사용한 바이트 수입니다. |
| `dataset_storage_usage_snapshots` | `DataSetCount` | INTEGER | 집계 DataSet 수입니다. |
| `dataset_quota_threshold_events` | `Id` | INTEGER, PK | 저장소 한도 임계값 이벤트를 식별합니다. |
| `dataset_quota_threshold_events` | `TenantId` | UUID/TEXT, FK 논리 관계 | 이벤트 Tenant입니다. |
| `dataset_quota_threshold_events` | `PolicyId` | UUID/TEXT, FK 논리 관계 | 적용된 한도 정책입니다. |
| `dataset_quota_threshold_events` | `Scope` | TEXT, 열거값 | 이벤트 범위입니다. |
| `dataset_quota_threshold_events` | `TargetId` | UUID/TEXT, NULL 허용 | 범위 대상입니다. |
| `dataset_quota_threshold_events` | `EventType` | TEXT, 열거값 | 초과·경고·회복 등 이벤트 종류입니다. |
| `dataset_quota_threshold_events` | `UsedBytes` | INTEGER | 이벤트 당시 사용량입니다. |
| `dataset_quota_threshold_events` | `LimitBytes` | INTEGER | 이벤트 당시 한도입니다. |
| `dataset_quota_threshold_events` | `UtilizationPercent` | INTEGER | 이벤트 당시 사용률입니다. |
| `dataset_quota_threshold_events` | `PolicyVersion` | INTEGER | 적용 정책 버전입니다. |
| `dataset_quota_threshold_events` | `OccurredAtUtc` | UTC | 이벤트 발생 시각입니다. |
| `dataset_validation_profiles` | `Id` | UUID/TEXT, PK | 검증 프로필을 식별합니다. |
| `dataset_validation_profiles` | `TenantId` | UUID/TEXT, FK 논리 관계 | 프로필 Tenant입니다. |
| `dataset_validation_profiles` | `Name` | TEXT | 프로필 이름입니다. |
| `dataset_validation_profiles` | `CreatedAtUtc` | UTC | 프로필 생성 시각입니다. |
| `dataset_validation_profile_revisions` | `ProfileId` | UUID/TEXT, PK 일부, FK 논리 관계 | 검증 프로필입니다. |
| `dataset_validation_profile_revisions` | `TenantId` | UUID/TEXT, FK 논리 관계 | 프로필 Tenant입니다. |
| `dataset_validation_profile_revisions` | `Revision` | INTEGER, PK 일부 | 프로필 개정 번호입니다. |
| `dataset_validation_profile_revisions` | `DefinitionJson` | TEXT/JSON | 검증 규칙 정의입니다. |
| `dataset_validation_profile_revisions` | `CreatedAtUtc` | UTC | 개정 생성 시각입니다. |
| `dataset_validation_executions` | `Id` | UUID/TEXT, PK | 검증 실행을 식별합니다. |
| `dataset_validation_executions` | `TenantId` | UUID/TEXT, FK 논리 관계 | 실행 Tenant입니다. |
| `dataset_validation_executions` | `DataSetId` | UUID/TEXT, FK 논리 관계 | 검증 대상 DataSet입니다. |
| `dataset_validation_executions` | `ExpectedDataSetVersion` | INTEGER | 실행 시 기대한 DataSet 버전입니다. |
| `dataset_validation_executions` | `SchemaVersion` | INTEGER | 사용한 스키마 버전입니다. |
| `dataset_validation_executions` | `ProfileId` | UUID/TEXT, FK 논리 관계 | 사용한 검증 프로필입니다. |
| `dataset_validation_executions` | `ProfileRevision` | INTEGER | 사용한 프로필 개정입니다. |
| `dataset_validation_executions` | `Status` | TEXT, 열거값 | 검증 실행 상태입니다. |
| `dataset_validation_executions` | `DetailReference` | TEXT, NULL 허용 | 상세 결과 저장 참조입니다. |
| `dataset_validation_executions` | `ProcessedRecords` | INTEGER | 처리한 레코드 수입니다. |
| `dataset_validation_executions` | `TotalRecords` | INTEGER | 전체 레코드 수입니다. |
| `dataset_validation_executions` | `ErrorCount` | INTEGER | 오류 수입니다. |
| `dataset_validation_executions` | `WarningCount` | INTEGER | 경고 수입니다. |
| `dataset_validation_executions` | `DetailItemCount` | INTEGER | 상세 항목 수입니다. |
| `dataset_validation_executions` | `DetailTruncated` | BOOLEAN | 상세 결과가 제한되었는지 나타냅니다. |
| `dataset_validation_executions` | `RuleCountsJson` | TEXT/JSON | 규칙별 결과 집계입니다. |
| `dataset_validation_executions` | `FailureCode` | TEXT, NULL 허용 | 실패 코드입니다. |
| `dataset_validation_executions` | `FailureMessage` | TEXT, NULL 허용 | 실패 설명입니다. |
| `dataset_validation_executions` | `CreatedAtUtc` | UTC | 검증 실행 시작 시각입니다. |
| `dataset_validation_executions` | `CompletedAtUtc` | UTC, NULL 허용 | 검증 실행 완료 시각입니다. |
| `dataset_validation_executions` | `Version` | INTEGER | 동시성 버전입니다. |
| `dataset_validation_executions` | `FencingToken` | INTEGER | 오래된 실행자의 쓰기를 차단하는 펜싱 값입니다. |
| `dataset_validation_executions` | `IdempotencyKey` | TEXT, NULL 허용 | 중복 실행 방지 키입니다. |
| `dataset_validation_details` | `Id` | UUID/TEXT, PK | 검증 상세 결과를 식별합니다. |
| `dataset_validation_details` | `ExecutionId` | UUID/TEXT, FK 논리 관계 | 검증 실행입니다. |
| `dataset_validation_details` | `TenantId` | UUID/TEXT, FK 논리 관계 | 결과 Tenant입니다. |
| `dataset_validation_details` | `DetailReference` | TEXT | 상세 결과 묶음 참조입니다. |
| `dataset_validation_details` | `RowNumber` | INTEGER | 오류가 발생한 행 번호입니다. |
| `dataset_validation_details` | `FieldName` | TEXT | 오류가 발생한 필드명입니다. |
| `dataset_validation_details` | `Code` | TEXT | 검증 규칙 코드입니다. |
| `dataset_validation_details` | `Severity` | TEXT, 열거값 | 오류·경고 심각도입니다. |
| `dataset_validation_details` | `Message` | TEXT | 사용자에게 제공할 설명입니다. |
| `dataset_validation_seen_keys` | `Id` | INTEGER, PK | 중복 검사 항목을 식별합니다. |
| `dataset_validation_seen_keys` | `ExecutionId` | UUID/TEXT, FK 논리 관계 | 검증 실행입니다. |
| `dataset_validation_seen_keys` | `RuleCode` | TEXT | 중복 검사 규칙입니다. |
| `dataset_validation_seen_keys` | `KeyHash` | TEXT | 중복 비교용 키 해시이며 원문 민감값은 저장하지 않습니다. |

### 5.6.3 `edan-tensor-dataset-content.db`

| 테이블명 | 컬럼명 | 형식/제약 | 용도와 주의사항 |
|---|---|---|---|
| `DataSetContent` | `Id` | UUID/TEXT, PK | 콘텐츠 레코드를 식별합니다. 기존 Migration 호환성 이름입니다. |
| `DataSetContent` | `StorageReference` | TEXT | 콘텐츠 저장 참조입니다. |
| `DataSetContent` | `DataSetId` | UUID/TEXT, FK 논리 관계 | 콘텐츠가 속한 DataSet입니다. |
| `DataSetContent` | `TenantId` | UUID/TEXT, FK 논리 관계 | 콘텐츠 Tenant 범위입니다. |
| `DataSetContent` | `OrganizationId` | UUID/TEXT, FK 논리 관계 | 콘텐츠 조직 범위입니다. |
| `DataSetContent` | `ProjectId` | UUID/TEXT, FK 논리 관계 | 콘텐츠 프로젝트 범위입니다. |
| `DataSetContent` | `Content` | BLOB | 실제 콘텐츠 본문입니다. 개인키·비밀번호·인증 토큰은 저장하지 않습니다. |
| `__schema_comments` | `ObjectType` | TEXT | 설명 대상이 테이블인지 컬럼인지 나타냅니다. |
| `__schema_comments` | `TableName` | TEXT | 설명 대상 테이블입니다. |
| `__schema_comments` | `ColumnName` | TEXT, NULL 허용 | 컬럼 설명일 때 컬럼명입니다. |
| `__schema_comments` | `Comment` | TEXT | 한국어 설명입니다. SQLite 설명 메타데이터입니다. |

### 5.6.4 `edan-worker.db`

| 테이블명 | 컬럼명 | 형식/제약 | 용도와 주의사항 |
|---|---|---|---|
| `worker_tasks` | `Id` | UUID/TEXT, PK | 작업을 식별합니다. |
| `worker_tasks` | `TenantId` | UUID/TEXT, FK 논리 관계 | 작업 Tenant입니다. |
| `worker_tasks` | `TaskType` | TEXT, FK 논리 관계 | 처리할 작업 유형 코드입니다. |
| `worker_tasks` | `IdempotencyKey` | TEXT | 중복 enqueue를 방지하는 키입니다. |
| `worker_tasks` | `PayloadReference` | TEXT | 작업 본문 저장 참조입니다. |
| `worker_tasks` | `Status` | TEXT, 열거값 | 대기·실행·완료·실패·Dead Letter 상태입니다. |
| `worker_tasks` | `AttemptCount` | INTEGER | 현재 시도 횟수입니다. |
| `worker_tasks` | `MaxAttempts` | INTEGER | 최대 시도 횟수입니다. |
| `worker_tasks` | `LeaseId` | TEXT, NULL 허용 | 현재 작업 소유권 식별자입니다. |
| `worker_tasks` | `WorkerId` | TEXT, NULL 허용 | 작업을 점유한 Worker 식별자입니다. |
| `worker_tasks` | `FencingToken` | INTEGER | 작업 점유 세대 값이며 오래된 Worker의 쓰기를 차단합니다. |
| `worker_tasks` | `LeaseExpiresAtUtc` | UTC, NULL 허용 | Lease 만료 시각입니다. |
| `worker_tasks` | `Checkpoint` | TEXT, NULL 허용 | 작업 재개 위치입니다. |
| `worker_tasks` | `CheckpointVersion` | INTEGER | 체크포인트 버전입니다. |
| `worker_tasks` | `CreatedAtUtc` | UTC | 작업 생성 시각입니다. |
| `worker_tasks` | `CompletedAtUtc` | UTC, NULL 허용 | 작업 완료 시각입니다. |
| `worker_tasks` | `NextAttemptAtUtc` | UTC, NULL 허용 | 다음 재시도 시각입니다. |
| `worker_tasks` | `LastErrorCode` | TEXT, NULL 허용 | 마지막 오류 코드입니다. |
| `worker_tasks` | `Version` | INTEGER | 동시성 버전입니다. |
| `worker_tasks` | `ProgressPercent` | INTEGER | 작업 진행률입니다. |
| `worker_tasks` | `ProgressStage` | TEXT, NULL 허용 | 현재 처리 단계입니다. |
| `worker_tasks` | `ProgressMessage` | TEXT, NULL 허용 | 진행 설명입니다. |
| `worker_tasks` | `ResultReference` | TEXT, NULL 허용 | 결과 저장 참조입니다. |
| `worker_tasks` | `ReadyAtUtcUnixMilliseconds` | INTEGER | 큐 조회용 준비 시각의 Unix 밀리초 보조값입니다. |
| `worker_tasks` | `LeaseExpiresAtUtcUnixMilliseconds` | INTEGER, NULL 허용 | 만료 작업 조회용 Unix 밀리초 보조값입니다. |
| `worker_tasks` | `CreatedAtUtcUnixMilliseconds` | INTEGER | 정렬·페이징용 생성 시각의 Unix 밀리초 보조값입니다. 세 Unix 밀리초 컬럼은 대응하는 UTC 컬럼과 동기화합니다. |
| `worker_task_types` | `TaskType` | TEXT, PK | 작업 유형 코드입니다. |
| `worker_task_types` | `Description` | TEXT | 작업 유형 설명입니다. |
| `worker_task_types` | `IsEnabled` | BOOLEAN | 작업 유형 활성화 여부입니다. |
| `worker_task_types` | `UpdatedAtUtc` | UTC | 변경 시각입니다. |
| `worker_task_types` | `Version` | INTEGER | 동시성 버전입니다. |
| `worker_task_replays` | `Id` | UUID/TEXT, PK | 재실행 이력을 식별합니다. |
| `worker_task_replays` | `TenantId` | UUID/TEXT, FK 논리 관계 | 재실행 Tenant입니다. |
| `worker_task_replays` | `OriginalTaskId` | UUID/TEXT, FK 논리 관계 | 원본 작업입니다. |
| `worker_task_replays` | `ReplayTaskId` | UUID/TEXT, FK 논리 관계 | 새로 생성한 재실행 작업입니다. |
| `worker_task_replays` | `RequestedBy` | TEXT | 재실행 요청 주체입니다. |
| `worker_task_replays` | `CreatedAtUtc` | UTC | 재실행 요청 시각입니다. |
| `worker_task_replays` | `ReplayRequestId` | UUID/TEXT, UNIQUE | 같은 재실행 요청의 중복 방지 키입니다. |
| `external_deliveries` | `Id` | UUID/TEXT, PK | 외부 전달을 식별합니다. |
| `external_deliveries` | `TenantId` | UUID/TEXT, FK 논리 관계 | 전달 Tenant입니다. |
| `external_deliveries` | `TargetUri` | TEXT/URI | 외부 수신 시스템 주소입니다. |
| `external_deliveries` | `Payload` | TEXT/JSON | 전송 본문이며 비밀값을 포함하지 않아야 합니다. |
| `external_deliveries` | `IdempotencyKey` | TEXT | 외부 전달 중복 방지 키입니다. |
| `external_deliveries` | `Status` | TEXT, 열거값 | 대기·전송·성공·실패 상태입니다. |
| `external_deliveries` | `AttemptCount` | INTEGER | 현재 전송 시도 횟수입니다. |
| `external_deliveries` | `MaxAttempts` | INTEGER | 최대 전송 시도 횟수입니다. |
| `external_deliveries` | `LastErrorCode` | TEXT, NULL 허용 | 마지막 전송 오류 코드입니다. |
| `external_deliveries` | `CreatedAtUtc` | UTC | 전달 생성 시각입니다. |
| `external_deliveries` | `CompletedAtUtc` | UTC, NULL 허용 | 전달 완료 시각입니다. |
| `external_deliveries` | `Version` | INTEGER | 동시성 버전입니다. |

### 5.6.5 프레임워크 테이블

| 데이터베이스 | 테이블명 | 컬럼명 | 형식/제약 | 용도와 주의사항 |
|---|---|---|---|---|
| 각 EF Core DB | `__EFMigrationsHistory` | `MigrationId` | TEXT, 프레임워크 관리 | 적용된 EF Core Migration 식별자입니다. |
| 각 EF Core DB | `__EFMigrationsHistory` | `ProductVersion` | TEXT, 프레임워크 관리 | Migration을 적용한 EF Core 버전입니다. |
| 각 EF Core DB | `__EFMigrationsLock` | 프레임워크 정의 컬럼 | 프레임워크 관리 | Migration 동시 실행을 막는 잠금 정보입니다. 실제 컬럼은 EF Core 버전에 따라 달라질 수 있으므로 제품 컬럼으로 해석하지 않습니다. |

## 6. 컬럼 구성과 보안 경계

- 모든 Tenant 범위 테이블은 가능한 경우 `tenant_id`를 포함해야 하며, 조회·수정·삭제 조건에서 Tenant 범위를 먼저 검사해야 합니다.
- 조직 또는 프로젝트 범위 데이터는 `organization_id`, `project_id`를 함께 사용하고, 프로젝트가 조직에 속하는지 검증해야 합니다.
- 외부 인증 주체는 사용자 이름이 아니라 안정적인 `subject_id`를 기준으로 연결합니다. 이메일은 식별자로 사용하지 않습니다.
- 상태 컬럼은 `Active`, `Inactive`, `Pending`, `Completed`, `Failed`, `Revoked`처럼 도메인 계약에 정의된 값만 저장합니다.
- Worker의 `lease_id`, `lease_expires_at_utc`, `fencing_token`, `version`은 Lease와 오래된 Worker의 쓰기 차단을 위해 함께 사용합니다.
- `idempotency_key`는 Tenant 범위에서 유일하게 관리하여 같은 요청의 중복 처리를 막습니다.
- `payload`, `content`, `*_json`은 민감정보와 개인키를 저장하지 않도록 입력 경계를 검사해야 합니다.
- 인증 토큰, 개인키, 비밀번호는 어느 테이블의 설명이나 로그에도 기록하지 않습니다.

## 7. 변경 정책

이번 작업에서는 기존 네 `.db` 파일에 설명 메타데이터만 추가했습니다. 기존 테이블·컬럼 이름을 바꾸지 않은 이유는 다음과 같습니다.

1. EF Core Migration의 기존 테이블 이름과 실제 파일의 이름이 연결되어 있습니다.
2. API와 Worker가 같은 물리 이름을 사용하므로 한 파일만 바꾸면 런타임 조회가 실패합니다.
3. 이름 변경은 데이터 복사, 외래 키, 인덱스, 백업·복구, 롤백, 회귀 테스트를 포함하는 별도 Migration입니다.
4. 이름 변경 Migration이 승인되기 전에는 현재 이름을 호환성 이름으로 보존하는 것이 안전합니다.

향후 이름을 실제로 변경할 때는 다음 순서를 지킵니다.

1. 전자사전에서 새 이름을 승인합니다.
2. EF Core 모델과 Migration을 함께 변경합니다.
3. 기존 데이터를 새 테이블·컬럼으로 복사하고 PK·인덱스를 `{table_name}_PK`, `{table_name}_N01` 형식으로 생성합니다.
4. 구버전 데이터와 새 데이터의 건수·해시·Tenant 격리를 비교합니다.
5. API, Worker, Client, 백업·복구, SQLite·PostgreSQL·SQL Server·Oracle 경계 테스트를 수행합니다.
6. 롤백 절차를 확인한 뒤에만 운영 후보 DB에 적용합니다.

## 8. SQLite 설명 확인

DBeaver에서 다음 조회를 실행하면 현재 파일에 저장된 테이블·컬럼 설명을 확인할 수 있습니다.

```sql
SELECT
    "ObjectType" AS "설명 대상",
    "TableName" AS "테이블명",
    NULLIF("ColumnName", '') AS "컬럼명",
    "Comment" AS "설명"
FROM "__schema_comments"
ORDER BY
    "TableName",
    CASE WHEN "ObjectType" = 'table' THEN 0 ELSE 1 END,
    "ColumnName";
```
