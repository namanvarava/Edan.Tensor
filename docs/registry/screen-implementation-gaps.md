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
| PLT-ORG-001 | InDevelopment | 조직 검색·페이징 | 구조화 생성, 대상 선택, 조직 이름·상태 수정 | UI smoke 미확인; ID/date filter는 실제 source contract에 없음 | P1 |
| PLT-PJT-001 | InDevelopment | 프로젝트 검색·페이징 | organizationId 대상 선택, 구조화 생성, 프로젝트 이름·상태 수정 | UI smoke 미확인; ID/date filter는 실제 source contract에 없음 | P1 |
| PLT-USR-001 | InDevelopment | 사용자·초대 검색·페이징 | 구조화 초대, 초대 상태 조회·재전송·철회·수락, 대상 선택, 사용자 수정, 상태 전용 명령 | CI WPF Registry smoke 통과; action-level UI 미검증 | P1 |
| PLT-MBR-001 | InDevelopment | Tenant·조직·프로젝트 멤버십 목록 | query-enabled scope list, 구조화 grant, 역할 변경·회수 명령 연결 | UI smoke 미확인; own-membership은 기존 unpaged 배열 | P1 |
| PLT-AUTH-001 | InDevelopment | 정책 목록·필터·평가 | 구조화 생성·수정, 대상 선택, enabled 변경, 평가 결과 API 연결 | UI smoke 미확인 | P1 |
| PLT-AUD-001 | InDevelopment | 감사 검색·필터·페이징 | integrity, archive 생성·검증·다운로드, archive job 목록·상세·재시도·검증, schedule, retention 연결 | UI smoke 미확인 | P2 |
| PLT-QTA-001 | InDevelopment | quota 정책 페이징·필터 | 구조화 생성, scope/target/enabled filter, 수정·enabled·history 연결 | UI smoke 미확인; plan은 실제 DTO의 응답 필드만 사용 | P1 |
| DSET-DSM-001 | InDevelopment | DataSet 목록·상세 | 조직·프로젝트·이름·상태 filter, 구조화 생성·활성화·이름 변경·보관·복원·삭제, SchemaRevisions 표시 | UI smoke 미확인 | P1 |
| DSET-SCHM-001 | InDevelopment | DataSet schema revision 목록 | 구조화 schema 등록, 전용 revision API, diff, immutable rollback, 실제 revision 필드 표시 | UI smoke 미확인 | P2 |
| DSET-STO-001 | InDevelopment | tenant·DataSet usage와 history | 파일 업로드·다운로드, 기간·scope·eventType filter, quota event, storage reset 연결 | UI smoke 미확인 | P2 |
| DSET-VLD-001 | InDevelopment | validation profile/execution 목록·detail | profile/execution collection, 구조화 validation 실행, profile/revision 생성·이름 변경·삭제, execution 삭제, date/failure filter 연결 | UI smoke 미확인 | P1 |
| WRK-TSK-001 | InDevelopment | Worker task 목록·상세 | taskType/status/time filter, 구조화 enqueue, 취소·강제 재시도, 결과 metadata 조회와 Tenant/Task 격리 raw result 저장·스트리밍 | CI WPF Registry smoke 통과; action-level UI 미검증 | P1 |
| WRK-DLQ-001 | InDevelopment | DLQ 페이징·필터·replay history | type/error/date filter, 대상 선택, replay 및 Replay 키, history 연결 | UI smoke 미확인; taskId/status는 item/detail 계약 | P2 |
| WRK-XDL-001 | InDevelopment | 외부 전달 목록·상세·수동 retry | status/host/error/date filter, 구조화 delivery 생성, 수동 retry와 결과 ID 연결 | UI smoke 미확인; payload는 계속 비노출 | P1 |

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
- `PLT-USR-001`: `GetUsersAsync`에 search/page/pageSize를 전달하고 실제 사용자 ID를 선택한다. `InviteUserRequestContract`, 사용자 update, status-only PATCH, 초대 목록·재전송·철회·token 수락 계약을 호출한다. 평문 token은 API 응답에 포함하지 않고 Development delivery adapter에서만 확인한다.
- `PLT-MBR-001`: Tenant·조직·프로젝트 membership 목록의 query-enabled page와 실제 DTO를 표시하고, 세 범위 grant 및 Tenant 역할·회수 PATCH를 연결했다. 기존 no-query 배열 응답과 mine 응답은 보존했다.
- `PLT-AUTH-001`: 정책 목록 DTO의 organization/project/subject/action/resource/effect/priority/enabled 필드를 표시한다. 정책 create/update/enabled patch, 실제 평가 결과 page contract를 연결했다.
- `PLT-AUD-001`: subjectId/action/isAllowed/fromUtc/toUtc query를 실제 감사 조회 API에 전달한다. integrity verify, archive 생성/검증/다운로드, archive job 목록·생성·상세·재시도·검증, schedule 조회·수정, retention 조회·수정을 연결했다.
- `PLT-QTA-001`: quota 정책의 scope/target/limit/version/enabled/plan DTO를 표시하고 scope/target/enabled filter와 create/update/enabled/history를 연결했다.

### DataSet

- `DSET-DSM-001`: DataSet collection을 조직·프로젝트·이름·상태로 조회하고 대상 detail DTO와 `SchemaRevisions`를 표시하며 create/activate/rename/archive/restore/delete를 연결했다.
- `DSET-SCHM-001`: detail 응답과 전용 `/schemas` page에서 `Version`, `SchemaJson`, `CreatedAtUtc`를 사용하고 schema 등록·diff·immutable rollback을 연결했다.
- `DSET-STO-001`: Tenant usage, 선택 DataSet usage, 기간 filter가 있는 history와 scope/eventType filter가 있는 quota event를 조회하며 `ContentPath` upload/download와 storage reset을 연결했다.
- `DSET-VLD-001`: profile page와 execution page를 조회하고 execution ID로 status/detail paging을 조회한다. profile 생성·revision·이름 변경·삭제, validation 실행, execution 삭제, date/failure filter를 연결했다.

### Worker

- `WRK-TSK-001`: task collection의 taskType/status/time filter와 task detail의 Lease/Fencing/checkpoint/progress 필드를 실제 DTO에서 표시하고 `EnqueueWorkerTaskRequestContract`를 구조화 입력으로 호출한다. cancel/retry, 결과 metadata, 64 MiB 제한 raw upload/download를 연결했다. `IWorkerResultStore`와 개발용 Tenant/Task 격리 파일 adapter가 결과 바이트 계약을 구현한다.
- `WRK-DLQ-001`: page/pageSize와 type/error/date filter를 DLQ API에 전달하고 실제 항목 ID를 대상 선택한다. replay endpoint와 replay history에 Replay 키를 연결했다.
- `WRK-XDL-001`: delivery collection filter와 detail의 상태·시도·오류 projection을 표시하고 `CreateExternalDeliveryRequestContract`를 구조화 입력으로 호출한다. admin 수동 retry가 새 worker task를 생성하며 payload는 비노출이다.

## 4. 우선순위별 처리 결과

### P0 — 보안 경계 유지

Tenant route/header/membership 검증과 관리자 endpoint filter를 기존 API Host 소스 기준으로 유지했다. 새 화면 명령은 모두 typed Client를 통하며 저장소 직접 접근을 추가하지 않았다. 원격 CI run `43736265722`가 전체 API 통합 테스트와 신규 Tenant 경계 smoke를 통과했고, 로컬 testhost만 별도로 미확인이다.

### P1 — 실제 계약 구현 완료, 런타임 확인 대기

1. 15개 화면의 구조화 Request DTO 입력, 고급 JSON fallback, 대상 선택, 생성 결과 ID 표시를 구현했다.
2. directory update/status, membership grant/list/role/revoke, policy filter/evaluation, quota scope/target/enabled filter/history, validation collection/lifecycle/date/failure filter, DataSet lifecycle/schema/storage, Worker collection/control/result-reference, External Delivery collection/retry 계약을 실제 소스에 추가하고 화면 Client에 연결했다.
3. DataSet content/history/quota event, audit archive/job/schedule/retention/raw archive export 계약을 화면 명령에 연결했다.
4. 확정된 `IWorkerResultStore` 계약과 개발용 파일 adapter를 구현했다. Worker 도메인은 계속 `ResultReference`만 보유하고, raw bytes와 metadata는 Tenant·Task 검증을 거쳐 별도 저장소가 관리한다.

남은 항목은 기능 구현이 아니라 로컬 환경에서 수행하지 못한 testhost 실행과 action-level UI 조작 증거다. 원격 Windows CI run `43736265722`에서는 전체 테스트, HTTP smoke, WPF Registry smoke가 통과했다.

### P2 — 소스 확인 완료 및 권장 구현 반영

1. membership 역할 변경·회수와 범위별 query-enabled 목록은 구현했다. own-membership은 보안상 활성 배열 계약을 유지한다.
2. DataSet rename/delete/archive/restore, schema diff/rollback, validation profile lifecycle/date/failure filter, storage reset을 실제 domain/application/HTTP/client/WPF 경로로 구현했다.
3. Worker cancel/force-retry/result metadata와 raw result upload/download, audit raw archive export를 Tenant 관리자 경계와 함께 구현했다. Worker raw bytes는 `IWorkerResultStore`에 저장하고 도메인에는 검증된 reference만 연결한다.

## 5. 현재 검증 결과

- 현재 변경분을 반영한 전체 Release 빌드는 오류 0건으로 완료됐다. API Host·Desktop·신규 smoke 프로젝트도 모두 빌드됐다. 패키지 취약성 조회 경고 3건은 외부 NuGet advisory/network 상태로 발생했으며 컴파일 오류는 아니다.
- CSV 검증을 실행했다. `screens.csv` 15행, `screen-api-mapping.csv` 110행, `screen-api-validation.csv` 75행, 기능 명세 15행, 서비스 바인딩 15행이며 Screen ID 15개, API ID 중복 0건, Validation ID 중복 0건, mapping·validation·functional·binding 참조 누락 0건이다.
- `dotnet build EDAN.Tensor.sln --configuration Release --no-restore -m:1`은 snapshot 테스트 접근성 오류를 수정한 뒤 오류 0건으로 재검증됐다.
- 로컬 전체 `dotnet test EDAN.Tensor.sln --configuration Release --no-build --no-restore -m:1`과 로컬 WPF smoke는 VSTest testhost 연결 제한으로 완료되지 않았다. 반면 원격 GitHub Actions run `43736265722`가 현재 커밋을 대상으로 전체 Build/Test, HTTP smoke 2건, WPF UI smoke 1건을 모두 성공시켰다. API/Worker 신규 계약 검증 행은 실제 원격 실행 증거에 따라 `Passed`로 갱신했고, action-level UI 행은 Registry smoke 범위를 넘지 않으므로 `NotRun`을 유지한다.
- 변경 전 기준선 전체 테스트는 13개 프로젝트 273 passed였고, 현재 커밋의 원격 전체 suite도 모든 프로젝트 통과를 확인했다.

## 6. UI 아키텍처 단계 판단

15개 화면의 API·DTO 조사와 실제 소스에 존재하는 조회·검색·페이징·mutation·lifecycle·control 계약 연결, 원격 Windows CI의 전체 Build/Test·HTTP smoke·WPF Registry smoke는 완료 범위로 판단한다. 화면 상태는 action-level UI 조작과 운영 IdP/DB를 포함하지 않는 현재 조사 단계의 `InDevelopment` 의미를 유지하며, 이는 이번 계약 구현의 미완료를 뜻하지 않는다.
