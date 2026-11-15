# EDAN.Tensor 화면 API·DTO 정합성 및 구현 격차 보고서

- 조사 기준일: 2026-11-15
- 조사 브랜치: `main-auto`
- 조사 기준 커밋: `c8a2d87`
- 조사 범위: 기존 Screen Registry 15개 전체
- 조사 원칙: 실제 Endpoint, DTO/응답 투영, Application Service, DI, 인증·인가, 테넌트 경계, 테스트 소스만 근거로 기록
- 상세 원자료: [screen-api-mapping.csv](screen-api-mapping.csv), [screen-api-validation.csv](screen-api-validation.csv)
- 화면 상태: 조사 대상 15개 모두 기존 Registry의 `Planned`를 유지한다. API 일부 구현은 UI 구현 완료를 의미하지 않는다.

## 1. 조사 결과 요약

15개 화면 모두에 연결되는 API 코드 또는 관련 업무 코드가 일부 확인되었다. 다만 API가 있다는 사실만으로 화면 요구사항이 충족된 것은 아니다. 특히 관리 목록·상세·검색·필터·수정·비활성화와 같은 화면 기능은 화면별로 별도 확인해야 하며, API가 없는 기능은 `NotImplemented`, 소스 범위를 확정할 수 없는 기능은 `NotVerified`로 구분했다.

| ScreenId | 실제 API 요약 | API 범위 판단 | 핵심 격차 | 우선순위 |
|---|---|---|---|---|
| PLT-TNT-001 | 테넌트 생성·활성화·내 멤버십 조회 | PartiallyImplemented | 전체 테넌트 목록·상세·검색 없음 | P1 |
| PLT-ORG-001 | 조직 생성 | PartiallyImplemented | 조직 목록·상세·수정·상태 API 없음 | P1 |
| PLT-PJT-001 | 프로젝트 생성 | PartiallyImplemented | 프로젝트 목록·상세·수정·상태 API 없음 | P1 |
| PLT-USR-001 | 사용자 초대 생성 | PartiallyImplemented | 초대 목록·상세·재발송·취소·만료 API 없음 | P1 |
| PLT-MBR-001 | 테넌트·조직·프로젝트 멤버십 부여 및 내 멤버십 조회 | PartiallyImplemented | 멤버십 목록·상세·역할 변경·회수 API 없음 | P1 |
| PLT-AUTH-001 | 정책 생성·목록·수정·활성화 | PartiallyImplemented | 정책 검색·평가 결과 API 없음 | P1 |
| PLT-AUD-001 | 감사 조회·무결성·아카이브·스케줄·작업·보존 | Implemented | 아카이브 목록/검색 API는 확인되지 않음 | P2 |
| PLT-QTA-001 | 정책 CRUD 일부·활성화·변경 이력 | PartiallyImplemented | 목록 query가 page/pageSize만 지원하고 도메인 필터 없음 | P2 |
| DSET-DSM-001 | 데이터셋 생성·상세·콘텐츠·활성화 | PartiallyImplemented | 목록·삭제/보관·수정 API 없음, 생성 계약의 선택성 확인 필요 | P1 |
| DSET-SCHM-001 | 스키마 추가·상세 내 revision | PartiallyImplemented | revision 목록/조회 전용 API와 schema 검색 없음 | P2 |
| DSET-STO-001 | 콘텐츠·사용량·테넌트 사용량·이력·quota event | PartiallyImplemented | 저장소 lifecycle 및 사용량 필터/페이지 API 없음 | P2 |
| DSET-VLD-001 | profile/revision, 실행 생성·상세·details·삭제 | PartiallyImplemented | profile/execution 목록 및 결과 요약용 전용 계약 없음 | P1 |
| WRK-TSK-001 | 작업 생성·상세 | PartiallyImplemented | 작업 목록·상태 필터·운영 제어 API 없음 | P1 |
| WRK-DLQ-001 | DLQ 목록·replay | PartiallyImplemented | task type/error/date 필터 및 이력 API 없음 | P2 |
| WRK-XDL-001 | 전달 생성·상세 | PartiallyImplemented | 전달 목록·retry API 없음 | P1 |

`PLT-AUD-001`은 현재 조사 범위에서 화면의 주요 감사 운영 동작을 모두 호출할 수 있는 API가 확인되어 `Implemented`로 분류했지만, 아카이브 목록이나 검색이 화면 요구에 필요하다면 별도 API 격차로 남는다. 이 분류는 UI가 구현되었다는 뜻이 아니다.

## 2. 화면별 실제 계약과 격차

### 2.1 Platform 관리

#### PLT-TNT-001 — Tenant Management

- 실제 Endpoint는 `POST /api/platform/tenants`, `POST /api/platform/tenants/{tenantId}/activate`, `GET /api/platform/tenants/mine`이다.
- 생성 Request는 `CreateTenantRequest(Code, Name)`이고 생성 응답은 anonymous `{ Id }`와 HTTP 201이다. 활성화는 body 없이 204를 반환한다. `mine` 응답은 `TenantId`, `Role` 배열이며 전체 테넌트 이름·상태·생성일을 반환하는 DTO는 없다.
- `PlatformProvisioningService`와 `TenantMembershipService`가 scoped로 등록된다. 생성·활성화는 `PlatformAdministrator`, 본인 멤버십 조회는 `TenantMember` 정책이다.
- `G-PLT-TNT-001-LIST`: 전체 테넌트 목록·상세·코드/명 검색 API 없음. 기존 명세의 검색/출력 필드는 설계 요구로 남긴다.

#### PLT-ORG-001 — Organization Management

- 실제 Endpoint는 `POST /api/platform/tenants/{tenantId}/organizations`이며 Request는 `CreateOrganizationRequest(Code, Name)`이다. 응답은 생성 ID와 HTTP 201이다.
- `PlatformProvisioningService`가 scoped로 주입된다. TenantMember, route tenant, active membership, TenantAdministrator 필터가 적용된다.
- `G-PLT-ORG-001-READ`: 조직 목록·상세·수정·상태 변경·검색 API가 없다. `OrgManager`는 현재 인증 정책으로 확인되지 않았다.

#### PLT-PJT-001 — Project Management

- 실제 Endpoint는 `POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/projects`이며 Request는 `CreateProjectRequest(Code, Name)`이다. 응답은 생성 ID와 HTTP 201이다.
- `PlatformProvisioningService`가 scoped로 주입되고 조직이 선택된 테넌트에 속하며 active인지 검증한다. TenantAdministrator 경계가 적용된다.
- `G-PLT-PJT-001-READ`: 프로젝트 목록·상세·수정·상태 변경·검색 API가 없다.

#### PLT-USR-001 — User Invitation Management

- 실제 Endpoint는 `POST /api/platform/tenants/{tenantId}/users/invitations`이며 Request는 `InviteUserRequest(Email, DisplayName)`이다. 응답은 생성 ID와 HTTP 201이다.
- `PlatformProvisioningService`가 scoped로 주입되고 TenantAdministrator 및 선택된 테넌트 경계가 적용된다. 서비스는 tenant 내 email uniqueness를 검증한다.
- `G-PLT-USR-001-LIFECYCLE`: 초대 목록·상세·상태·만료·재발송·취소 API가 없다.

#### PLT-MBR-001 — Membership Management

- 실제 Endpoint는 `POST /api/platform/tenants/{tenantId}/memberships`, `POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/memberships`, `POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/projects/{projectId}/memberships`, `GET /api/platform/tenants/mine`이다.
- Request는 각각 `GrantTenantMembershipRequest(SubjectId, Role)`, `GrantOrganizationMembershipRequest(SubjectId, Role)`, `GrantProjectMembershipRequest(SubjectId, Role)`이다. 부여 응답은 ID와 HTTP 201이며, `mine`은 `TenantId`, `Role`만 반환한다.
- `TenantMembershipService`와 `ScopedMembershipService`가 scoped로 등록된다. 대상 subject의 active tenant membership과 조직/프로젝트 parent scope를 확인한다.
- `G-PLT-MBR-001-READ`: 멤버십 목록·상세·역할 변경·회수·상태 API가 없다.

#### PLT-AUTH-001 — Authorization Policy Management

- 정책 생성/조회는 `POST|GET /api/platform/tenants/{tenantId}/authorization-policies`이다. 생성 Request는 `CreateAuthorizationPolicyRequest(OrganizationId?, ProjectId?, SubjectId, Action, ResourceType, Effect, Priority)`이고 목록 응답은 `Id, OrganizationId, ProjectId, SubjectId, Action, ResourceType, Effect, Priority, IsEnabled, CreatedAtUtc`이다.
- 수정은 `PUT /authorization-policies/{policyId}` (`Effect, Priority`, 204), 활성화 변경은 `PATCH /authorization-policies/{policyId}/enabled` (`IsEnabled`, 204)이다.
- `AuthorizationPolicyService`와 `PlatformAuthorizationService`가 scoped로 등록된다. 정책 scope가 선택된 tenant/org/project에 속하는지 검증하고 변경 감사 기록을 남긴다.
- `G-PLT-AUTH-001-EVALUATE`: 정책 평가 결과를 직접 조회하는 API가 없다. `PlatformAuthorizationService`는 내부 리소스 인가 평가에 사용된다.
- `G-PLT-AUTH-001-QUERY`: 정책 목록은 query filter/page 계약 없이 전체 tenant 목록을 반환한다.

#### PLT-AUD-001 — Authorization Audit Management

- 조회는 `GET /api/platform/tenants/{tenantId}/authorization-audit`이며 `subjectId, action, isAllowed, fromUtc, toUtc, page, pageSize`를 지원한다. 응답은 `TotalCount, Page, PageSize, Items[Id, SubjectId, Action, ResourceType, Scope, IsAllowed, ReasonCode, MatchedPolicyId, OccurredAtUtc]`이다.
- 무결성은 `GET /authorization-audit/integrity`, `POST /authorization-audit/integrity/verify`이며 archive는 생성·검증·schedule·job 생성/조회/retry/verify·retention 조회/수정 API까지 확인된다.
- `AuthorizationAuditQueryService`, `AuthorizationAuditIntegrityService`, `AuthorizationAuditArchiveService`, `AuthorizationAuditArchiveJobService`, `AuthorizationAuditArchiveScheduleService`, `AuthorizationAuditRetentionService`는 scoped로 등록된다. 모든 tenant 경로는 TenantAdministrator 및 route/membership 경계를 거친다.
- `G-PLT-AUD-001-ARCHIVE-LIST`: 아카이브 목록/검색 endpoint는 확인되지 않았다. 생성된 archive는 ID를 통한 검증으로 접근한다.

#### PLT-QTA-001 — Storage Quota Management

- 정책 목록은 `GET /api/platform/tenants/{tenantId}/storage-quota-policies?page&pageSize`, 변경 이력은 `GET /storage-quota-policies/{policyId}/history?page&pageSize`이다. 정책 생성은 `POST`, 수정은 `PUT`, 활성화는 `PATCH /storage-quota-policies/{policyId}/enabled`이다.
- 생성 Request는 `Scope, TargetId, TenantLimitBytes?, OrganizationLimitBytes?, ProjectLimitBytes?, DataSetLimitBytes?, WarningThresholdPercent=80, PlanCode?='Community'`이다. 수정은 `ExpectedVersion, ... limits?, WarningThresholdPercent, PlanCode`, enabled 변경은 `IsEnabled, ExpectedVersion`이다.
- 목록 응답은 `TotalCount, Page, PageSize, Items`이고 history item은 `Id, TenantId, PolicyId, ActorSubjectId, ChangeType, BeforeJson, AfterJson, OccurredAtUtc`이다. optimistic concurrency conflict가 실제 구현되어 있다.
- `StorageQuotaPolicyManagementService`는 ApiHost에서 scoped로 등록된다. TenantAdministrator 및 selected tenant 경계가 적용된다.
- `G-PLT-QTA-001-FILTER`: page/pageSize 외 코드·scope·target·enabled·plan/date 필터는 없다.

### 2.2 DataSet 관리

#### DSET-DSM-001 — DataSet Management

- 그룹은 `/api/platform/tenants/{tenantId}/datasets`이며 생성 `POST`의 Request는 `CreateDataSetRequest(OrganizationId, ProjectId, Name, StorageAdapter='sqlite', ContentFormat='json-array')`이다.
- 생성 응답은 `Id, TenantId, OrganizationId, ProjectId, Name, StorageAdapter, ContentFormat, Status=Draft`를 반환한다. 상세 `GET /{dataSetId}`는 `Version, CreatedAtUtc, CurrentSchemaVersion, SchemaRevisions[Version, SchemaJson, CreatedAtUtc]`를 포함한다.
- 콘텐츠 쓰기 `POST /{dataSetId}/content`는 raw body, 읽기 `GET /{dataSetId}/content`는 octet-stream이며 활성화는 `POST /{dataSetId}/activate`이다. `DataSetManagementService`는 scoped다.
- `G-DSET-DSM-001-LIST-LIFECYCLE`: dataset collection list, 수정, 삭제/보관 API가 없다.
- `G-DSET-DSM-001-DTO`: 생성 DTO의 organization/project 필수성 및 response가 anonymous projection이라는 점을 화면 계약으로 고정하기 전 별도 계약화가 필요하다.

#### DSET-SCHM-001 — DataSet Schema Management

- `POST /api/platform/tenants/{tenantId}/datasets/{dataSetId}/schemas`가 `AddDataSetSchemaRequest(SchemaJson)`을 받아 `DataSetId, SchemaVersion`을 201로 반환한다.
- dataset 상세 응답의 `SchemaRevisions`에 `Version, SchemaJson, CreatedAtUtc`가 포함된다. 별도 schema revision collection endpoint는 없다.
- dataset tenant/membership 경계와 TenantAdministrator가 적용된다.
- `G-DSET-SCHM-001-QUERY`: schema revision 목록/상세·버전 필터·수정/삭제 API가 없다.

#### DSET-STO-001 — DataSet Storage Management

- dataset별 `GET /{dataSetId}/content`, `GET /{dataSetId}/usage`와 tenant별 `GET /storage-usage`, `GET /storage-usage/history?take`, `GET /storage-usage/quota-events?take`가 있다.
- dataset usage는 bytes/counts, limits, utilization, warning, measured time을 반환한다. tenant history item은 `PeriodStartUtc, CapturedAtUtc, UsedBytes, DataSetCount`; quota event는 `PolicyId, Scope, TargetId, EventType, UsedBytes, LimitBytes, UtilizationPercent, PolicyVersion, OccurredAtUtc`이다.
- storage adapter/usage reader는 infrastructure에서 singleton, usage snapshot service는 ApiHost에서 scoped로 등록된다. 데이터 read는 TenantMember, quota event는 TenantAdministrator 경계다.
- `G-DSET-STO-001-LIFECYCLE`: storage adapter allocation/release를 관리하는 별도 API는 없다.
- `G-DSET-STO-001-FILTER`: history/quota-events는 `take`만 지원하며 기간·scope·event type 필터와 page 응답이 없다.

#### DSET-VLD-001 — DataSet Validation Management

- profile 생성은 `POST /validation-profiles` (`Name, DefinitionJson`), revision 추가는 `POST /validation-profiles/{profileId}/revisions` (`DefinitionJson`)이다.
- 실행 생성은 `POST /{dataSetId}/validations` (`ExpectedDataSetVersion, SchemaVersion, ProfileId, ProfileRevision`)이며 required `Idempotency-Key`를 받고 202 `{Execution, WorkerTaskId, Created}`를 반환한다. 삭제는 `DELETE /validations/{executionId}`이다.
- 실행 상세는 `GET /validations/{executionId}`이며 `Id, TenantId, DataSetId, ExpectedDataSetVersion, SchemaVersion, ProfileId, ProfileRevision, FencingToken, Status, DetailReference, ProcessedRecords, TotalRecords, ErrorCount, WarningCount, DetailItemCount, DetailTruncated, RuleCountsJson, FailureCode?, FailureMessage?, CreatedAtUtc, CompletedAtUtc?`를 제공한다. details는 `skip,take`와 `RowNumber, FieldName?, Code, Severity, Message`를 제공한다.
- `DataSetValidationService`와 worker enqueue service는 scoped다. profile/revision/execution 생성/삭제는 TenantAdministrator, 실행 조회/details는 TenantMember이다.
- `G-DSET-VLD-001-LIST`: profile 목록·revision 목록·execution 목록 endpoint가 없다.
- `G-DSET-VLD-001-DTO`: profile snapshot 및 실행 응답이 내부 snapshot/anonymous projection을 조합하므로 화면용 안정 DTO로 고정하기 전 계약 검토가 필요하다.

### 2.3 Worker 관리

#### WRK-TSK-001 — Worker Task Monitoring

- 작업 생성은 `POST /api/platform/tenants/{tenantId}/worker/tasks`, Request는 `EnqueueWorkerTaskRequest(TaskType, PayloadReference, MaxAttempts=5)`와 required `Idempotency-Key`이다. 응답은 `Id, TaskType, Status, CreatedAtUtc`와 `Created`이다.
- 상세 `GET /worker/tasks/{taskId}`는 `Id, TaskType, Status, AttemptCount, MaxAttempts, FencingToken, CheckpointVersion, CreatedAtUtc, NextAttemptAtUtc, CompletedAtUtc, LastErrorCode, LeaseExpiresAtUtc, ProgressPercent, ProgressStage, ProgressMessage, HasResult`이다.
- `WorkerTaskEnqueueService`와 `WorkerTaskQueryService`가 scoped, repository도 scoped, lease renewer와 handler registry는 singleton이다. route/membership/TenantAdministrator 경계와 tenant idempotency가 적용된다.
- `G-WRK-TSK-001-LIST-CONTROL`: 작업 목록, 상태/유형/기간 필터, cancel/retry/pause 같은 운영 제어 endpoint가 없다.

#### WRK-DLQ-001 — Dead Letter Queue Management

- `GET /api/platform/tenants/{tenantId}/worker/dead-letters?page&pageSize`는 `Page, PageSize, Items[Id, TaskType, Status, AttemptCount, MaxAttempts, LastErrorCode, CreatedAtUtc, NextAttemptAtUtc]`를 반환한다.
- `POST /dead-letters/{taskId}/replay`는 required `Replay-Request-Id` GUID와 subject를 받아 `OriginalTaskId, ReplayTaskId`를 201로 반환한다.
- `WorkerTaskDeadLetterService`는 ApiHost scoped, repository는 scoped다. tenant administrator 및 dead-letter 상태/재처리 idempotency 경계가 적용된다.
- `G-WRK-DLQ-001-FILTER-HISTORY`: page/pageSize 외 task type·error·date 필터와 replay 이력 endpoint가 없다.

#### WRK-XDL-001 — External Delivery Monitoring

- 생성 `POST /api/platform/tenants/{tenantId}/worker/deliveries`는 `CreateExternalDeliveryRequest(TargetUri, Payload, MaxAttempts=5)`와 required `Idempotency-Key`를 받고 202 `{Delivery, WorkerTaskId, Created}`를 반환한다.
- 상세 `GET /worker/deliveries/{deliveryId}`의 Delivery projection은 `Id, TenantId, TargetUri, IdempotencyKey, Status, AttemptCount, MaxAttempts, LastErrorCode, CreatedAtUtc, CompletedAtUtc`이며 Payload는 반환하지 않는다.
- 생성은 TenantAdministrator, 상세는 TenantMember이다. `ExternalDeliveryService`는 typed HttpClient로 등록되고 repository는 scoped다. Target URI allowlist/loopback 정책과 동일 key conflict가 구현되어 있다.
- `G-WRK-XDL-001-LIST-RETRY`: 전달 목록·status/error/date 검색과 수동 retry endpoint가 없다. 자동 retry는 Worker 동작으로만 확인된다.

## 3. 공통 인증·인가·테넌트 경계

- 외부 JWT는 issuer, audience, lifetime, signing key를 검증한다. Development에서만 명시적 bypass가 가능하며 운영 정책으로 간주하지 않는다.
- `TenantMember`는 authenticated user와 `sub`를 요구하고, `PlatformAdministrator`는 여기에 `platform_admin` 역할을 추가 요구한다.
- tenant scoped route는 `X-Tenant-Id`와 route `tenantId`를 비교하는 `TenantRouteAuthorizationFilter`를 거치며, `TenantMembershipAuthorizationFilter`가 선택된 tenant의 active membership을 확인한다.
- 관리자 변경 API는 `TenantAdministratorAuthorizationFilter`로 active tenant administrator인지 확인한다. 정책 평가가 필요한 resource endpoint는 `ScopedResourceAuthorizationFilter`와 `PlatformAuthorizationService`를 사용한다.
- 조사한 통합 테스트에는 tenant header 누락, 다른 tenant membership 접근, quota concurrency, dataset scope, worker DLQ, external delivery tenant/idempotency 경계가 포함된다. 확인된 코드와 테스트 범위 밖의 모든 보안 속성을 추정하지 않는다.

## 4. DI 및 Application Service 요약

| 영역 | 계약/구현 확인 | DI 확인 |
|---|---|---|
| Platform provisioning/membership/policy/audit | endpoint가 concrete Application Service를 직접 사용; 화면 ViewModel 계약은 없음 | Platform Application 서비스 scoped; repository scoped; archive store singleton |
| Quota/usage/worker API orchestration | `StorageQuotaPolicyManagementService`, `DataSetStorageUsageSnapshotService`, `WorkerTask*Service` | ApiHost scoped; storage adapter/reader 일부 singleton |
| DataSet/validation | `DataSetManagementService`, `DataSetValidationService` | DataSet Application scoped; repository/DbContext scoped |
| External delivery | `ExternalDeliveryService` | typed HttpClient; repository scoped; options singleton |
| Screen-Service binding | 실제 View/ViewModel 파일과 연결되는 contract 없음 | `screen-service-bindings.csv`는 기존 표준에 따라 header-only 유지 |

API Application Service의 DI 등록을 UI Service binding으로 전환하지 않았다. 화면 구현이 시작되면 ViewModel이 실제로 호출하는 별도 계약을 확인한 뒤 `screen-service-bindings.csv`를 갱신해야 한다.

## 5. 테스트 결과

실제 실행한 명령:

```text
dotnet test EDAN.Tensor.sln --no-restore --configuration Release --logger "console;verbosity=minimal"
```

결과: 전체 13개 테스트 프로젝트에서 **273 passed, 0 failed, 0 skipped**. 테스트 실행이 확인된 범위는 Platform/DataSet/Worker Domain, Application, Infrastructure, Client, Commercial, Licensing, ApiHost Integration이다. 주요 화면 관련 통합 테스트는 tenant provisioning, dataset management/validation, external delivery, worker DLQ, quota paging/concurrency/history, tenant scope/header 경계를 포함한다.

샌드박스 제한으로 처음 실행된 별도 시도는 `MSB4276` Workload SDK resolver 오류로 시작되지 않았으며 통과로 집계하지 않았다. 이후 실제 Windows 실행에서 위 전체 결과를 확인했다.

## 6. 우선순위별 개선 사항

### P0 — 즉시 차단

현재 소스와 실행한 테스트에서 tenant route/membership를 우회하는 확정 증거는 확인하지 못했다. 따라서 이번 조사 결과의 P0 구현 격차는 없다. 향후 목록·상세 API를 추가할 때에도 동일한 route/header/membership/administrator 경계를 유지해야 한다.

### P1 — 화면 착수 전 API 계약 확정이 필요한 기능

- `PLT-TNT-001`, `PLT-ORG-001`, `PLT-PJT-001`, `PLT-USR-001`, `PLT-MBR-001`: 관리 목록·상세·검색·lifecycle 및 역할 변경/회수 계약 확정.
- `DSET-DSM-001`: dataset collection 조회와 lifecycle 범위(수정, 삭제/보관) 확정.
- `DSET-VLD-001`: profile/revision/execution 목록 및 화면용 안정 응답 계약 확정.
- `WRK-TSK-001`: task 목록·필터·운영 제어 범위 확정.
- `WRK-XDL-001`: delivery 목록·retry 범위 확정.
- `PLT-AUTH-001`: 정책 검색과 평가 결과 노출 여부 확정.

### P2 — 운영 화면 완성도 개선

- quota, dataset storage, DLQ의 query filter/page 계약 확장.
- authorization archive 목록/검색·export 필요 여부 결정.
- schema revision 전용 조회와 dataset validation 결과/프로파일 collection 계약 결정.
- anonymous projection을 화면 소비용 named response DTO로 안정화할지 결정하고, 결정 전에는 현재 필드를 추가로 추정하지 않음.

### P3 — 문서 및 연결성 개선

- UI ViewModel이 실제 API를 소비할 때만 화면-Service binding을 등록.
- API DTO 변경 시 mapping/validation/functional spec MD·CSV와 테스트 증거를 같은 변경 단위로 갱신.
- 각 신규 목록 API에 tenant 경계, paging 상한, empty result, concurrency/idempotency 테스트를 추가.

## 7. UI 아키텍처 단계 진입 판단

이번 조사로 API·DTO의 현재 경계와 미구현 기능이 문서화되었으므로 **UI 아키텍처 설계(화면 상태·navigation·ViewModel 경계)를 시작하는 것은 가능**하다. 다만 P1 API 격차를 구현 완료로 간주하거나 화면을 API가 제공하지 않는 필드에 맞춰 구현해서는 안 된다.

권장 진입 조건은 다음과 같다.

1. 각 화면의 P1 격차에 대해 API 확장, 화면 범위 축소, 또는 설계 보류 중 하나를 승인한다.
2. 승인된 API 계약의 named Request/Response DTO와 오류/페이징 계약을 확정한다.
3. 그 다음에만 ViewModel-Service 계약을 만들고 `screen-service-bindings.csv`를 실제 코드와 함께 등록한다.

따라서 현재 결론은 `UI 아키텍처 설계: 진행 가능`, `UI 기능 구현: P1 계약 결정 후 진행`이다.

## 8. 산출물 검증 체크리스트

- [x] Registry의 15개 ScreenId가 mapping/validation에 각각 존재
- [x] mapping의 API·route·DTO·Service·DI·auth·test 필드를 실제 소스 기준으로 기록
- [x] validation에서 구현됨/부분 구현/미구현/미확인을 구분
- [x] 미실행 테스트를 통과로 표시하지 않음
- [x] `screen-service-bindings.csv`에 API Application Service를 UI 바인딩으로 오기록하지 않음
- [x] CSV parsing, 중복·누락·reference·actual source path·UTF-8 형식 검증 수행
- [x] main-auto에만 변경 및 커밋
