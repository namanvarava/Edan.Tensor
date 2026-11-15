# EDAN.Tensor 화면 API·DTO 정합성 및 구현 격차 보고서

- 조사·구현 기준일: 2026-11-15
- 작업 브랜치: `main-auto`
- 범위: 기존 Registry의 15개 화면 전체
- 원칙: 실제 소스와 실행한 검증만 기록한다. 미구현과 미확인을 구분하며, 실행하지 않은 테스트는 통과로 표시하지 않는다.
- 원자료: [screen-api-mapping.csv](screen-api-mapping.csv), [screen-api-validation.csv](screen-api-validation.csv)
- 화면 구현: `src/Client/EDAN.Tensor.Desktop`의 .NET 10 WPF Desktop shell과 15개 ViewModel이 등록되었고, 모든 화면이 `IDesktopScreenService` → `DesktopApiSession` → `TensorApiClient` 경로를 사용한다.

## 1. 현재 상태 요약

| ScreenId | 실제 화면 연결 | 실제 API 조회 | 확인된 구현 범위 | 남은 격차 | 우선순위 |
|---|---|---|---|---|---|
| PLT-TNT-001 | InDevelopment | 내 소속 조회 + Tenant directory | 구조화 Tenant 생성·활성화, directory 대상 선택, Tenant 수정 | directory API는 PlatformAdministrator 경계이며 UI smoke 미확인 | P1 |
| PLT-ORG-001 | InDevelopment | 조직 검색·페이징 | 구조화 생성, 대상 선택, 조직 수정 | 상태 변경·멤버십 목록 API는 별도 계약 없음 | P1 |
| PLT-PJT-001 | InDevelopment | 프로젝트 검색·페이징 | organizationId 대상 선택, 구조화 생성, 프로젝트 수정 | 상태 변경·프로젝트 목록 외 추가 필터 API 없음 | P1 |
| PLT-USR-001 | InDevelopment | 사용자 검색·페이징 | 구조화 초대, 대상 선택, 사용자 수정 | 상태 전용 API는 확인되지 않음 | P1 |
| PLT-MBR-001 | InDevelopment | Tenant 멤버십 목록 | Tenant·조직·프로젝트 멤버십 부여 입력 연결 | revoke 및 조직/프로젝트 membership collection API는 확인되지 않음 | P1 |
| PLT-AUTH-001 | InDevelopment | 정책 목록 | 구조화 생성·수정, 대상 선택, enabled 변경 | 평가 결과 전용 API는 확인되지 않음 | P1 |
| PLT-AUD-001 | InDevelopment | 감사 검색·필터·페이징 | integrity, archive, archive job, schedule, retention의 실제 Client 명령 연결 | archive collection/list API는 확인되지 않음 | P2 |
| PLT-QTA-001 | InDevelopment | quota 정책 페이징 | 구조화 생성, 대상 선택, 수정·enabled·history 연결 | scope/status 전용 query filter API는 확인되지 않음 | P1 |
| DSET-DSM-001 | InDevelopment | DataSet 상세 | 구조화 생성·활성화, SchemaRevisions 실제 DTO 표시 | DataSet collection/list API가 소스에 없음 | P1 |
| DSET-SCHM-001 | InDevelopment | DataSet 상세의 SchemaRevisions | 구조화 schema 등록, 실제 revision 필드 표시 | 전용 schema revision collection API가 소스에 없음 | P2 |
| DSET-STO-001 | InDevelopment | tenant·DataSet usage와 history | 파일 업로드·다운로드, quota event 조회, history take 연결 | 기간·scope filter query API가 소스에 없음 | P2 |
| DSET-VLD-001 | InDevelopment | validation execution/detail | 구조화 validation 실행, profile/revision 생성, execution 삭제 | profile/execution collection API가 소스에 없음 | P1 |
| WRK-TSK-001 | InDevelopment | Worker task 상세 | 구조화 enqueue, 실제 task ID 결과 연결 | task collection/list, cancel/retry/result API가 소스에 없음 | P1 |
| WRK-DLQ-001 | InDevelopment | DLQ 페이징 | 대상 선택, replay 및 Replay 키 연결 | type/error/date/history filter API가 소스에 없음 | P2 |
| WRK-XDL-001 | InDevelopment | 외부 전달 상세 | 구조화 delivery 생성 및 결과 ID 연결 | delivery collection/list·수동 retry API가 소스에 없음 | P1 |

`InDevelopment`는 소스 기준 View·ViewModel·typed Client 연결과 실제 계약 기반 입력이 존재한다는 뜻이다. 런타임 smoke와 현재 변경 이후 테스트host 실행 증거가 확보되기 전에는 `Active`로 표시하지 않는다.

## 2. 공통 실제 계약

- 화면 진입은 `MainWindowViewModel`의 15개 navigation item과 `DataTemplate` 매핑으로 고정된다. 기존 Screen ID와 MODULE·AREA 코드는 변경하지 않았다.
- `RegistryScreenView.xaml`은 실제 WPF `UserControl`이며 화면 ID, 제목, 설명, API 상태, 검색어, 페이지, 페이지 크기, 대상 ID, 구조화 Request DTO 입력, 고급 JSON, 결과 ID를 표시한다.
- 구조화 입력은 `ScreenInputModel`의 실제 DTO 필드명·형식(Guid, integer, long, Boolean, DateTimeOffset, JSON)을 사용한다. 고급 JSON은 실제 Request DTO를 직접 호출해야 하는 수정·보존·스케줄 명령에만 남겨 두었다.
- 조회 결과에서 Tenant·조직·프로젝트·사용자·정책·quota·DLQ 항목의 실제 ID를 대상 선택 버튼으로 연결한다. 생성 결과 ID는 `LastResultIdText`에 자동 표시한다.
- ViewModel은 `IDesktopScreenService`만 의존한다. `DesktopApiSession`은 창 수명 동안 한 개가 생성되고, 연결 시 `TensorApiClient`에 `DefaultTenantId`와 `AccessTokenProvider`를 설정한다. View나 ViewModel이 저장소·DbContext·Application Service를 직접 호출하지 않는다.
- API Client는 `X-Tenant-Id`, Bearer 토큰, `Idempotency-Key`, `Replay-Request-Id`를 요청 경계에서 구성한다. Tenant-scoped endpoint는 route tenant, header, active membership를 API Host filter에서 검증하며 관리자 변경 API는 확인된 관리자 정책을 요구한다.

## 3. 화면별 구현 근거와 미구현·미확인 구분

### Platform

- `PLT-TNT-001`: `GetMyMembershipsAsync`와 Platform 관리자용 `GetTenantsAsync`가 모두 연결됐다. `CreateTenantRequestContract`, activate, `UpdateDirectoryEntryRequestContract`를 구조화/고급 입력으로 호출한다.
- `PLT-ORG-001`: `GetOrganizationsAsync`에 search/page/pageSize를 전달하고 실제 directory ID를 선택한다. `CreateOrganizationRequestContract`와 조직 update 계약을 호출한다.
- `PLT-PJT-001`: `GetProjectsAsync`에 search/page/pageSize를 전달하고 응답의 `OrganizationId`를 확인할 수 있다. organizationId 대상의 생성과 organizationId + projectId 대상의 update 계약을 호출한다.
- `PLT-USR-001`: `GetUsersAsync`에 search/page/pageSize를 전달하고 실제 사용자 ID를 선택한다. `InviteUserRequestContract`와 사용자 update 계약을 호출한다.
- `PLT-MBR-001`: Tenant membership 목록을 실제 DTO로 표시하고 Tenant grant를 구조화 입력으로 호출한다. 조직·프로젝트 grant는 실제 route와 DTO를 사용하며, revoke/list 계약은 소스에서 확인되지 않았다.
- `PLT-AUTH-001`: 정책 목록 DTO의 organization/project/subject/action/resource/effect/priority/enabled 필드를 표시한다. 정책 create/update와 enabled patch를 연결했다. 평가 전용 endpoint는 확인되지 않았다.
- `PLT-AUD-001`: subjectId/action/isAllowed/fromUtc/toUtc query를 실제 감사 조회 API에 전달한다. integrity verify, archive 생성/검증, archive job 생성·상세·재시도·검증, schedule 조회·수정, retention 조회·수정을 연결했다. archive collection/list route는 확인되지 않았다.
- `PLT-QTA-001`: quota 정책의 scope/target/limit/version/enabled DTO를 표시한다. create/update/enabled/history를 연결했으며, API에 없는 scope/status query filter는 추가하지 않았다.

### DataSet

- `DSET-DSM-001`: 대상 DataSet ID로 detail DTO와 `SchemaRevisions`를 조회하고 create/activate를 연결했다. collection/list API는 실제로 없어 목록을 추정하지 않았다.
- `DSET-SCHM-001`: detail 응답의 `Version`, `SchemaJson`, `CreatedAtUtc`를 사용하고 `AddDataSetSchemaRequestContract`를 연결했다. 전용 revision list API는 소스에 없다.
- `DSET-STO-001`: Tenant usage, 선택 DataSet usage, storage history의 실제 DTO를 조회한다. `ContentPath`를 통한 파일 upload/download와 quota event history를 연결했다. 기간/scope query API는 없다.
- `DSET-VLD-001`: execution ID로 status/detail paging을 조회하고 `CreateDataSetValidationRequestContract`를 구조화 입력으로 호출한다. profile 생성/revision 추가와 execution 삭제를 연결했다. profile/execution collection API는 없다.

### Worker

- `WRK-TSK-001`: task ID로 Lease/Fencing/checkpoint/progress 필드를 실제 DTO에서 표시하고 `EnqueueWorkerTaskRequestContract`를 구조화 입력으로 호출한다. task list 및 운영 제어 API는 확인되지 않았다.
- `WRK-DLQ-001`: page/pageSize를 DLQ API에 전달하고 실제 항목 ID를 대상 선택한다. replay endpoint에 Replay 키를 연결했다. type/error/date/history filter API는 확인되지 않았다.
- `WRK-XDL-001`: delivery detail의 상태·시도·오류 projection을 표시하고 `CreateExternalDeliveryRequestContract`를 구조화 입력으로 호출한다. delivery list와 수동 retry API는 확인되지 않았다.

## 4. 우선순위별 처리 결과

### P0 — 보안 경계 유지

Tenant route/header/membership 검증과 관리자 endpoint filter를 기존 API Host 소스 기준으로 유지했다. 새 화면 명령은 모두 typed Client를 통하며 저장소 직접 접근을 추가하지 않았다. 현재 변경 후 런타임 보안 테스트는 testhost 연결 실패로 실행 증거를 확보하지 못했으므로 통과로 표시하지 않는다.

### P1 — 실제 계약 구현 완료, 런타임 확인 대기

1. 15개 화면의 구조화 Request DTO 입력, 고급 JSON fallback, 대상 선택, 생성 결과 ID 표시를 구현했다.
2. 소스에 이미 존재한 directory update, membership grant, policy enabled, quota update/history/enabled, validation profile/revision/delete 계약을 화면 명령에 연결했다.
3. DataSet content, quota event, audit archive/job/schedule/retention 계약을 화면 명령에 연결했다.
4. API가 실제로 제공하지 않는 DataSet/Validation/Worker/External Delivery collection 및 운영 제어는 구현하지 않고 API 부재로 기록했다.

남은 P1 항목은 제품 코드 추정 구현이 아니라, 실제로 없는 API 계약 확정과 실행 가능한 UI smoke/test 환경 확보이다.

### P2 — 소스 확인 완료 및 API 부재 종결

1. audit archive 목록, quota scope/status filter, storage 기간/scope filter, DLQ type/error/date/history filter는 route/repository가 확인되지 않아 미구현으로 종결했다.
2. DataSet validation profile/execution collection과 schema revision 전용 collection은 route/repository가 확인되지 않아 미구현으로 종결했다.
3. Worker 운영 제어와 External Delivery 수동 retry/list는 route/repository가 확인되지 않아 미구현으로 종결했다.

## 5. 현재 검증 결과

- 현재 변경분을 반영한 `dotnet build EDAN.Tensor.sln --configuration Release --no-restore -m:1`이 경고 0개·오류 0개로 완료됐다. Desktop 최종 빌드도 경고 0개·오류 0개다.
- CSV 검증을 실행해 `screens.csv` 15행, `screen-api-mapping.csv` 69행, `screen-api-validation.csv` 75행, `screen-service-bindings.csv` 15행, `screen-functional-specifications.csv` 15행의 행·열·중복·Screen ID 참조·필수 근거를 확인했고 `CSV_VALIDATION=PASS`를 얻었다.
- 현재 변경 이후 Client 단위 테스트를 `AuthorizationAuditOperationalContractsUseTenantScopedRoutes`와 기존 외부 전달 테스트 필터로 실행했으나, VSTest가 30초 후 testhost에 연결하지 못해 중단됐다(exit 1). 따라서 이번 변경의 Client 테스트는 통과 집계하지 않고 관련 UI 검증 행을 `NotRun`으로 유지한다.
- 변경 전 기준선 전체 테스트는 13개 프로젝트 273 passed였으나 이번 변경 후 전체 통과를 의미하지 않는다.

## 6. UI 아키텍처 단계 판단

15개 화면의 API·DTO 조사와 실제 소스에 존재하는 조회·검색·페이징·mutation 계약 연결은 완료 범위로 판단한다. API가 없는 목록·필터·운영 제어를 임의로 만들지 않았고, 그 근거도 기록했다. 다만 현재 화면 상태를 `Active`로 승격하거나 UI 아키텍처 설계를 최종 확정하려면, 실행 가능한 환경에서 현재 변경분의 WPF smoke와 테스트host 재실행 증거가 추가로 필요하다.
