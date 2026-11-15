# EDAN.Tensor 화면 API·DTO 정합성 및 구현 격차 보고서

- 조사·구현 기준일: 2026-11-15
- 작업 브랜치: `main-auto`
- 범위: 기존 Registry의 15개 화면 전체
- 원칙: 실제 소스와 실행한 검증만 기록한다. 미구현과 미확인을 구분하며, 실행하지 않은 테스트는 통과로 표시하지 않는다.
- 원자료: [screen-api-mapping.csv](screen-api-mapping.csv), [screen-api-validation.csv](screen-api-validation.csv)
- 화면 구현: `src/Client/EDAN.Tensor.Desktop`의 .NET 10 WPF Desktop shell과 15개 ViewModel이 등록되었고, 모든 화면이 `IDesktopScreenService` → `DesktopApiSession` → `TensorApiClient` 경로를 사용한다.

## 1. 현재 상태 요약

| ScreenId | 실제 화면 연결 | 실제 API 조회 | 남은 격차 | 우선순위 |
|---|---|---|---|---|
| PLT-TNT-001 | InDevelopment | `GET /api/platform/tenants/mine` | JSON 명령 영역은 create/activate를 지원하지만 구조화된 입력·전체 directory 전환은 남음 | P1 |
| PLT-ORG-001 | InDevelopment | `GET /api/platform/tenants/{tenantId}/organizations` | JSON 명령 영역은 create를 지원; 구조화된 수정/status action은 남음 | P1 |
| PLT-PJT-001 | InDevelopment | `GET /api/platform/tenants/{tenantId}/projects` | JSON 명령 영역은 organization 대상 create를 지원; 조직 선택·수정 UI는 남음 | P1 |
| PLT-USR-001 | InDevelopment | `GET /api/platform/tenants/{tenantId}/users` | JSON 명령 영역은 초대 create를 지원; 구조화된 상태 UI는 남음 | P1 |
| PLT-MBR-001 | InDevelopment | `GET /api/platform/tenants/{tenantId}/memberships` | JSON 명령 영역은 Tenant grant를 지원; 조직·프로젝트 grant/revoke UI는 남음 | P1 |
| PLT-AUTH-001 | InDevelopment | `GET /api/platform/tenants/{tenantId}/authorization-policies` | JSON 명령 영역은 create/update를 지원; enable action·평가 결과 전용 API는 남음 | P1 |
| PLT-AUD-001 | InDevelopment | 감사 페이지 `GET .../authorization-audit` | JSON 명령 영역은 integrity verify를 지원; archive/retention action과 archive list API는 남음 | P2 |
| PLT-QTA-001 | InDevelopment | `GET .../storage-quota-policies` | JSON 명령 영역은 create를 지원; update/enable/history 구조화 UI와 page 외 필터는 남음 | P1 |
| DSET-DSM-001 | InDevelopment | `GET .../datasets/{dataSetId}` | JSON 명령 영역은 create/activate를 지원; collection list와 content UI는 남음 | P1 |
| DSET-SCHM-001 | InDevelopment | DataSet detail의 `SchemaRevisions` | JSON 명령 영역은 schema 등록을 지원; 전용 revision endpoint 없음 | P2 |
| DSET-STO-001 | InDevelopment | tenant/dataSet usage와 history | content 입출력 UI와 기간 필터는 남음 | P2 |
| DSET-VLD-001 | InDevelopment | execution/detail `GET` | JSON 명령 영역은 validation create를 지원; profile/execution list·delete UI는 남음 | P1 |
| WRK-TSK-001 | InDevelopment | `GET .../worker/tasks/{taskId}` | JSON 명령 영역은 enqueue를 지원; task list/filter·운영 제어 API는 남음 | P1 |
| WRK-DLQ-001 | InDevelopment | DLQ page `GET` | JSON 명령 영역은 replay를 지원; type/error/date 필터와 history API는 남음 | P2 |
| WRK-XDL-001 | InDevelopment | delivery detail `GET` | JSON 명령 영역은 delivery create를 지원; delivery list·수동 retry API는 남음 | P1 |

`InDevelopment`는 View·ViewModel·typed Client 조회 경로가 소스에 존재한다는 뜻이며, 해당 화면의 모든 mutation workflow가 완성됐다는 뜻이 아니다. 실제 API가 제공하지 않는 collection/filter/control을 화면에서 추정 구현하지 않았다.

## 2. 공통 실제 계약

- 화면 진입은 `MainWindowViewModel`의 15개 navigation item과 `DataTemplate` 매핑으로 고정된다. 기존 Screen ID와 MODULE·AREA 코드는 변경하지 않았다.
- `RegistryScreenView.xaml`은 실제 WPF `UserControl`이며 화면 ID, 제목, 설명, API 상태, 검색어, 페이지, 페이지 크기, 대상 ID, action, 결과 필드를 표시한다.
- ViewModel은 `IDesktopScreenService`만 의존한다. `DesktopApiSession`은 창 수명 동안 한 개가 생성되고, 연결 시 `TensorApiClient`에 `DefaultTenantId`와 `AccessTokenProvider`를 설정한다. 공통 명령 영역은 실제 Request DTO JSON과 멱등성/Replay 키를 그대로 Client 계약에 전달한다.
- API Client는 `X-Tenant-Id`, Bearer 토큰, `Idempotency-Key`, `Replay-Request-Id`를 요청 경계에서 구성한다. View나 ViewModel이 저장소·DbContext·Application Service를 직접 호출하지 않는다.
- Tenant-scoped endpoint는 route tenant, `X-Tenant-Id`, active membership를 API Host filter에서 검증하며, 관리자 변경 API는 `TenantAdministrator`를 요구한다. Desktop의 조회 성공은 권한 우회를 의미하지 않는다.

## 3. 화면별 구현 근거와 미구현 구분

### Platform

- `PLT-TNT-001`: `TenantManagementViewModel`이 `GetMyMembershipsAsync`를 호출한다. 공통 JSON 명령 영역에서 실제 `CreateTenantRequestContract` 생성과 Tenant 활성화를 호출하며, 구조화된 입력·전체 directory 전환은 남았다.
- `PLT-ORG-001`: `OrganizationManagementViewModel`이 검색·페이지·페이지 크기를 `GetOrganizationsAsync`로 전달한다. 공통 JSON 명령 영역에서 실제 `CreateOrganizationRequestContract` 생성을 호출하며, 구조화된 생성·수정/status action은 남았다.
- `PLT-PJT-001`: `ProjectManagementViewModel`이 `GetProjectsAsync`를 호출한다. 공통 JSON 명령 영역에서 실제 `CreateProjectRequestContract` 생성을 호출하며, `organizationId` 선택과 구조화된 수정 UI는 남았다.
- `PLT-USR-001`: `UserInvitationManagementViewModel`이 `GetUsersAsync`를 호출한다. 공통 JSON 명령 영역에서 실제 `InviteUserRequestContract` 초대를 호출하며, 구조화된 초대·상태 UI는 남았다.
- `PLT-MBR-001`: `MembershipManagementViewModel`이 Tenant membership list를 조회한다. 공통 JSON 명령 영역에서 실제 Tenant membership grant를 호출하며, 조직/프로젝트 대상 선택과 grant/revoke 구조화 UI는 남았다.
- `PLT-AUTH-001`: 정책 목록 DTO를 실제 API 응답 필드로 표시한다. 공통 JSON 명령 영역에서 실제 정책 create/update를 호출하며, enable action과 평가 결과 전용 endpoint는 없다.
- `PLT-AUD-001`: `subjectId` 검색, page/pageSize를 감사 조회 API에 전달하고 실제 audit item 필드를 표시한다. 무결성·archive·retention route는 API에 있으나 화면 action은 남았다.
- `PLT-QTA-001`: quota page DTO와 Tenant 관리자 경계를 사용한다. 공통 JSON 명령 영역에서 실제 quota create를 호출하며, update/enable/history 구조화 UI와 page 외 필터는 남았다.

### DataSet

- `DSET-DSM-001`: 대상 DataSet ID로 detail DTO와 `SchemaRevisions`를 조회한다. DataSet collection list API는 실제로 없으므로 목록을 추정하지 않았다.
- `DSET-SCHM-001`: 같은 detail 응답의 실제 revision 필드(`Version`, `SchemaJson`, `CreatedAtUtc`)를 표시한다. revision 등록 UI와 전용 revision list endpoint는 남았다.
- `DSET-STO-001`: Tenant usage, 선택 DataSet usage, 최근 snapshot을 typed DTO로 조회한다. content upload/download action과 기간/scope filter는 남았다.
- `DSET-VLD-001`: execution ID로 status와 detail paging을 조회한다. 공통 JSON 명령 영역에서 실제 validation create를 호출하며, profile/execution collection API와 profile/revision/delete UI는 남았다.

### Worker

- `WRK-TSK-001`: task ID로 Lease/Fencing/checkpoint/progress 필드를 실제 DTO에서 표시한다. task list·cancel·retry·result API는 실제로 확인되지 않아 구현하지 않았다.
- `WRK-DLQ-001`: page/pageSize를 DLQ API에 전달하고 실제 항목을 표시한다. 공통 대상 ID·Replay 키 입력으로 실제 replay endpoint를 호출하며, type/error/date 필터와 history API는 남았다.
- `WRK-XDL-001`: delivery ID로 Payload를 노출하지 않는 실제 status projection을 표시한다. delivery list와 수동 retry endpoint는 실제로 확인되지 않았다.

## 4. 우선순위별 개선 사항

### P0

현재 구현과 기존 통합 테스트에서 Tenant route/header/membership를 우회하는 확정 증거는 없다. 새 명령 UI를 추가할 때도 동일한 API Host 보안 경계를 유지해야 한다.

### P1

1. JSON 명령 영역을 화면별 구조화 입력과 validation message로 교체하고, 성공 후 대상 ID 자동 연결을 추가한다.
2. DataSet/Validation/Worker/External Delivery의 대상 ID 입력을 목록 선택 또는 명령 결과 자동 연결로 확장한다.
3. 실제 endpoint가 없는 collection/list/filter는 API를 먼저 확정한 뒤에만 화면 기능으로 승격한다.

### P2

1. 감사 archive 목록, quota scope/status filter, storage 기간 filter, DLQ type/error/date filter를 API 계약으로 확정한다.
2. DataSet validation profile/execution collection과 schema revision 전용 조회 계약을 확정한다.
3. 외부 전달 수동 retry와 Worker 운영 제어의 보안·멱등성 계약을 확정한다.

## 5. 현재 검증 결과

- `dotnet build EDAN.Tensor.sln --configuration Release --no-restore -m:1`: 0 warnings, 0 errors. API, Client, Desktop, 기존 테스트 프로젝트를 모두 빌드했다.
- 새 Platform directory integration test는 `dotnet test ... --filter FullyQualifiedName~PlatformDirectoryRoutesReturnTenantScopedPagedContracts`로 실제 실행을 시도했으나, `VSTEST_CONNECTION_TIMEOUT=30` 후 testhost 연결 실패로 중단됐다. 통과로 집계하지 않았다.
- 새 Client 계약 테스트도 동일하게 필터 실행을 시도했으나 testhost 연결 실패로 중단됐다. 통과로 집계하지 않았다.
- 변경 전 기준선 전체 테스트는 13개 프로젝트 273 passed, 0 failed, 0 skipped였으나 이번 변경 후 전체 통과를 의미하지 않는다.
- 따라서 이번 변경에서 테스트 상태를 `Passed`로 표시하지 않으며, 실행 가능한 환경에서 전체 테스트를 재실행해야 한다.

## 6. UI 아키텍처 단계 판단

현재는 API·DTO 정합성 조사와 실제 조회·명령 화면 연결이 진행된 상태다. 공통 JSON 명령 영역으로 실제 계약을 호출할 수 있지만, 구조화된 입력, 대상 선택, UI smoke/test 증거가 아직 부족하므로 UI 아키텍처 설계를 최종 완료로 판정하지 않는다. 다음 단계는 JSON 입력을 화면별 구조화 입력으로 교체하고, 실행 가능한 UI smoke/test 증거를 확보한 뒤에만 각 화면을 `Active`로 전환하는 것이다.
