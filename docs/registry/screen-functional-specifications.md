# EDAN.Tensor 화면별 상세 기능 명세

- 문서 상태: 화면 설계 기준안 + 2026-11-15 API·DTO 정합성 조사 반영
- 적용 브랜치: `main-auto`
- 기준 Registry: [screens.csv](screens.csv)
- 화면 수: 15
- 연계 CSV: [screen-functional-specifications.csv](screen-functional-specifications.csv)
- API 매핑: [screen-api-mapping.csv](screen-api-mapping.csv)
- API 검증: [screen-api-validation.csv](screen-api-validation.csv)
- 구현 격차: [screen-implementation-gaps.md](screen-implementation-gaps.md)
- 작성 원칙: 확정된 화면 ID·MODULE·AREA는 기존 Screen Registry를 참조하고, 상세 기능은 본 문서에서 단일 관리한다.
- 구현 상태: `src/Client/EDAN.Tensor.Desktop`에 공통 WPF View, 15개 ViewModel, typed Client 조회·구조화 Request DTO·대상 선택·확인된 mutation binding이 구현되어 `InDevelopment`로 관리한다. API가 실제로 제공하지 않는 collection/filter/control은 미구현으로 구분하며, 실행 가능한 UI smoke/test가 확인되기 전에는 `Active`로 판정하지 않는다.

## 1. 문서 목적 및 관리 경계

본 문서는 등록된 15개 화면의 목적, 주요 기능, 검색 조건, 입력 항목, 출력 항목, 사용자 권한 및 API 구현 근거를 정의한다.

2026-11-15 전수 조사 이후에는 화면 설계 요구와 실제 API 계약을 분리한다. 기존 화면별 절의 검색·입력·출력은 설계 기준이며, 실제 Request/Response 필드·Endpoint·DI·인가·테스트 결과는 API 매핑/검증 CSV와 이 문서의 API 정합성 부록을 기준으로 한다. 소스에 없는 필드는 실제 계약으로 승격하지 않는다.

| 정보 | 기준 문서 |
|---|---|
| ScreenId, 모듈·영역, 화면명, 상태, View·ViewModel 경로 | [screens.csv](screens.csv) |
| 화면별 목적·기능·검색·입력·출력·권한·구현 격차 | 본 문서 및 [screen-functional-specifications.csv](screen-functional-specifications.csv) |
| 화면-Service 계약 관계 | [screen-service-bindings.csv](screen-service-bindings.csv) |
| MODULE·AREA 코드 | [screen-module-area-codes.csv](screen-module-area-codes.csv) |
| ID·상태·소스 연결 관리 규칙 | [Screen Registry 표준](../standards/EDAN-SCREEN-REGISTRY-STANDARD.md) |

중복 관리를 피하기 위해 본 문서는 화면별 상세 명세의 원본이며 CSV는 엑셀 필터·검토·추적을 위한 동일 데이터의 표 형식이다. ScreenId와 화면 상태를 별도로 재발급하거나 변경하지 않는다.

## 2. 확정 기준과 설계 제안의 구분

### 2.1 기존 Registry에서 확정된 항목과 현재 구현 반영
- 15개 ScreenId, MODULE·AREA 코드, 한국어·영문 화면명
- 모든 화면의 현재 상태는 `InDevelopment`
- ViewTechnology, ViewType, ViewPath, ViewCodeBehindPath, ViewModelType, ViewModelPath는 `screens.csv`에 실제 소스 경로로 기록
- ViewModel은 `IDesktopScreenService` 계약을 사용하고, 구현체 `DesktopApiSession`은 `MainWindowViewModel`에서 창 수명으로 생성
- 공통 View는 `Views/RegistryScreenView.xaml`, 화면별 concrete View와 ViewModel은 실제 WPF 프로젝트 소스에 존재

### 2.2 본 명세의 설계 기준과 실제 제한
- 목적, 기능, 검색 조건, 입력·출력 항목은 화면 설계 기준으로 정의
- 검색 조건은 UI 설계 조건이며, API가 해당 필터를 모두 지원한다는 뜻이 아님
- 입력 항목은 화면 요구사항이며, 실제 Request DTO와 일치 여부를 구현 전에 검증
- 출력 항목은 화면에 필요한 정보이며, 실제 Response 계약에서 제공하지 않는 값은 API 보완 또는 표시 제외
- 사용자 권한의 제안 역할은 현재 구현된 인증 정책과 구분
- 현재 구현은 실제 API가 제공하는 조회·검색·페이징·상세 계약과 화면별 구조화 Request DTO 입력을 호출한다. 수정·보존·스케줄처럼 DTO JSON이 필요한 계약은 고급 JSON 입력으로 연결하고, 대상 선택·생성 결과 ID·Replay/Idempotency 키를 공통 UI에 표시한다. API가 없는 기능과 UI smoke/test 미확인은 [구현 격차 보고서](screen-implementation-gaps.md)에 별도로 기록한다.

### 2.3 권한 표현 기준

| 유형 | 의미 |
|---|---|
| 구현 확인 | API 코드에서 인증 정책 또는 Endpoint Filter로 확인된 권한 |
| 설계 제안 | 화면 권한 설계를 위한 역할명. 실제 역할·정책 구현 전까지 미확정 |
| 추가 확인 | 실제 API 인가 필터·요청 범위·정책 연결을 다시 확인해야 함 |

현재 코드에서 확인된 인증 정책 이름은 `PlatformAdministrator`, `TenantMember`, `TenantAdministrator`다. `OrgManager`, `ProjectManager`, `Auditor`, `WorkerOperator`, `ReadOnly`는 본 명세의 제안 역할이며 실제 구현된 권한 코드로 간주하지 않는다. 화면 버튼 노출과 무관하게 서버가 최종 권한을 검증해야 한다.

## 3. 공통 화면 기준

- 테넌트 선택 및 소속 확인을 먼저 수행하고, 모든 업무 검색은 서버가 승인한 Tenant 범위로 제한한다.
- 목록 화면은 기본 페이지 크기 50건을 제안한다. 실제 API가 페이지 파라미터를 제공하는 화면에서만 서버 페이징으로 연결한다.
- 기간 검색은 UTC 기준 API 값과 사용자 로컬 시간 표시를 분리한다.
- 검색 필드가 API에서 지원되지 않으면 화면 로컬 필터로 오인 구현하지 말고 API 추가 필요로 기록한다.
- 입력 검증은 필수값, 형식, 길이, 범위, 멱등성 키, 동시성 버전 및 상위 소속 관계를 포함한다.
- 민감정보, 인증 토큰, 원본 Payload, 내부 저장 경로는 최소 노출 원칙을 적용한다.
- 모든 권한 변경·재처리·정책 변경은 행위자와 실행 시각을 추적할 수 있도록 감사 정책을 연계한다.

## 4. 화면별 상세 명세

### PLT-TNT-001 — 테넌트 관리 (`Tenant Management`)

**목적**

테넌트를 생성·활성화하고 로그인 사용자의 테넌트 소속을 확인한다.

**주요 기능**
- 테넌트 생성
- 테넌트 활성화
- 내 테넌트 소속 조회
- 테넌트 상세·상태 확인

**검색 조건**
- 테넌트 ID
- 테넌트 코드
- 테넌트명
- 상태
- 생성 기간

**입력 항목**
- 테넌트 코드(생성 필수)
- 테넌트명(생성 필수)
- 테넌트 ID(활성화 대상)

**출력 항목**
- 테넌트 ID
- 테넌트 코드·명
- 상태
- 내 역할·소속 테넌트 목록
- 생성·활성화 시각(제공 시)

**사용자 권한**
- PlatformAdministrator: 생성·최초 활성화
- TenantMember: 본인 테넌트 목록 조회
- TenantAdministrator: 테넌트 내부 관리 화면 진입

**API 구현 근거**
- 구현 상태: **일부 구현**
- POST /api/platform/tenants
- POST /api/platform/tenants/{tenantId}/activate
- GET /api/platform/tenants/mine

**추가 구현·확인 사항**
- 전체 테넌트 목록·상세 조회 API 확인/구현
- 테넌트 코드·명 검색 API 확인/구현
- 수정·비활성화는 별도 정책 승인 전 범위 제외

**설계 비고**

Tenant 전체 관리 목록과 내 테넌트 목록을 구분한다. PlatformAdministrator와 TenantMember 인증 정책은 API에서 확인됨.

---

### PLT-ORG-001 — 조직 관리 (`Organization Management`)

**목적**

테넌트 내부 조직을 생성하고 조직 정보를 관리한다.

**주요 기능**
- 조직 생성
- 조직 목록·상세 조회
- 조직과 프로젝트 관계 확인
- 조직 상태 관리(추가 범위)

**검색 조건**
- 테넌트 ID
- 조직 ID
- 조직 코드
- 조직명
- 상태

**입력 항목**
- 테넌트 ID(필수)
- 조직 코드(요청 계약 확인 필요)
- 조직명(요청 계약 확인 필요)
- 조직 설명(계약 확인 필요)

**출력 항목**
- 조직 ID
- 테넌트 ID
- 조직 코드·명(응답 계약 확인 필요)
- 프로젝트 관계(추가 조회 필요)
- 상태(추가 조회 필요)

**사용자 권한**
- TenantAdministrator: 조직 생성·관리(정책 제안)
- OrgManager: 담당 조직 관리(역할·정책 추가 필요)
- TenantMember: 허용 범위 조회(추가 API 필요)

**API 구현 근거**
- 구현 상태: **생성만 확인**
- PlatformEndpoints.cs의 조직 생성 경로

**추가 구현·확인 사항**
- 정확한 생성 DTO 필드 확인
- 조직 목록·상세·수정·상태 변경 API
- 조직별 역할 및 멤버십 범위 정책

**설계 비고**

목록·상세·수정 기능을 구현 완료로 표시하지 않는다. OrgManager는 제안 역할이며 현재 인증 정책 확인되지 않음.

---

### PLT-PJT-001 — 프로젝트 관리 (`Project Management`)

**목적**

테넌트·조직 범위의 프로젝트를 생성하고 프로젝트 업무 경계를 관리한다.

**주요 기능**
- 프로젝트 생성
- 프로젝트 목록·상세 조회
- 상위 조직 확인
- 프로젝트 상태 관리(추가 범위)

**검색 조건**
- 테넌트 ID
- 조직 ID
- 프로젝트 ID
- 프로젝트 코드·명
- 상태

**입력 항목**
- 테넌트 ID(필수)
- 조직 ID(요청 계약에 따름)
- 프로젝트 코드·명(정확한 DTO 확인 필요)
- 설명(계약 확인 필요)

**출력 항목**
- 프로젝트 ID
- 테넌트·조직 ID
- 프로젝트 코드·명(응답 계약 확인 필요)
- 상태(추가 조회 필요)

**사용자 권한**
- TenantAdministrator: 프로젝트 생성·관리(정책 제안)
- OrgManager: 소속 조직 프로젝트 관리(역할 추가 필요)
- ProjectManager: 담당 프로젝트 관리(역할 추가 필요)

**API 구현 근거**
- 구현 상태: **생성만 확인**
- PlatformEndpoints.cs의 프로젝트 생성 경로

**추가 구현·확인 사항**
- 정확한 생성 DTO 및 응답 필드 확인
- 프로젝트 목록·상세·수정·상태 API
- 조직-프로젝트 관계 및 범위 인가 검증

**설계 비고**

OrgManager·ProjectManager는 제안 역할이다. 실제 API 인가 필터와 역할 정책 확인 후 권한 매핑.

---

### PLT-USR-001 — 사용자 초대 관리 (`User Invitation Management`)

**목적**

테넌트에 사용자를 초대하고 초대 요청의 상태를 관리한다.

**주요 기능**
- 사용자 초대 생성
- 초대 목록·상세 조회
- 초대 상태·만료 확인
- 재발송·취소(추가 범위)

**검색 조건**
- 테넌트 ID
- 초대 ID
- 이메일·사용자 식별자
- 초대 상태
- 초대 기간
- 요청자

**입력 항목**
- 테넌트 ID(필수)
- 초대 대상 식별 정보(정확한 DTO 확인 필요)
- 역할·조직·프로젝트 범위(계약 확인 필요)

**출력 항목**
- 초대 ID
- 대상 식별 정보
- 역할·범위
- 초대 상태·요청자·요청 시각·만료 시각(응답 계약 확인 필요)

**사용자 권한**
- TenantAdministrator: 테넌트 사용자 초대(정책 제안)
- OrgManager/ProjectManager: 하위 범위 초대는 별도 정책 필요

**API 구현 근거**
- 구현 상태: **생성만 확인**
- PlatformEndpoints.cs의 사용자 초대 생성 경로

**추가 구현·확인 사항**
- 요청 DTO 및 초대 수명주기 확인
- 초대 목록·상세·상태 API
- 재발송·취소·만료 API

**설계 비고**

이메일 마스킹 및 초대 토큰 비노출. 세부 필드는 DTO를 다시 확인한 후 계약에 맞춰 확정.

---

### PLT-MBR-001 — 멤버십 관리 (`Membership Management`)

**목적**

사용자와 테넌트·조직·프로젝트 사이의 소속 및 역할 관계를 관리한다.

**주요 기능**
- 테넌트 멤버십 부여
- 조직·프로젝트 멤버십 부여(실제 경로별 계약 확인)
- 내 소속 조회
- 멤버십 목록·역할 변경·회수(추가 범위)

**검색 조건**
- 테넌트·조직·프로젝트 ID
- 사용자 ID
- 역할
- 멤버십 상태

**입력 항목**
- 테넌트 ID
- 대상 SubjectId
- 역할
- 조직·프로젝트 범위 ID(범위별)

**출력 항목**
- 멤버십 ID
- 사용자/Subject ID
- 소속 범위
- 역할
- 상태·부여 시각·부여자(조회 API 추가 필요)

**사용자 권한**
- PlatformAdministrator: 테넌트 멤버십 부여(확인됨)
- TenantAdministrator: 테넌트 내부 멤버십 운영(정책 제안)
- TenantMember: 본인 소속 조회(확인됨)

**API 구현 근거**
- 구현 상태: **부여·본인 조회 확인**
- POST /tenants/{tenantId}/memberships
- GET /tenants/mine
- 조직·프로젝트 멤버십 경로는 실제 코드 확인 필요

**추가 구현·확인 사항**
- 테넌트 멤버십 목록·상세
- 조직·프로젝트 멤버십 부여 경로 및 계약 확인
- 역할 변경·회수·상태 API
- 회수 이력 및 감사 기록

**설계 비고**

현재 확인된 테넌트 멤버십 부여는 PlatformAdministrator 전용. 역할 계층을 임의로 확장하지 않는다.

---

### PLT-AUTH-001 — 권한 정책 관리 (`Authorization Policy Management`)

**목적**

테넌트·조직·프로젝트 범위의 사용자 접근 허용·거부 정책을 관리한다.

**주요 기능**
- 정책 목록 조회
- 정책 생성
- 정책 상세 표시
- 정책 수정·활성 상태 변경(추가 API 확인)
- 우선순위 및 적용 범위 확인

**검색 조건**
- 조직·프로젝트 ID
- SubjectId
- Action
- ResourceType
- Effect
- 활성 여부
- 우선순위

**입력 항목**
- 조직 ID(선택)
- 프로젝트 ID(선택)
- SubjectId(선택)
- Action(필수)
- ResourceType(필수)
- Effect Allow/Deny(필수)
- Priority(필수)

**출력 항목**
- 정책 ID
- 조직·프로젝트 ID
- SubjectId
- Action
- ResourceType
- Effect
- Priority
- IsEnabled
- CreatedAtUtc

**사용자 권한**
- TenantAdministrator: 정책 생성·조회(확인됨)
- Auditor: 조회 전용(추가 정책 필요)

**API 구현 근거**
- 구현 상태: **생성·조회 확인**
- POST /tenants/{tenantId}/authorization-policies
- GET /tenants/{tenantId}/authorization-policies
- PlatformAuthorizationService

**추가 구현·확인 사항**
- 정책 수정·삭제·활성화 API 확인
- 정책 변경 이력·충돌 처리
- 실제 역할/정책 기반 UI 권한 매핑

**설계 비고**

정책 생성·조회는 TenantAdministrator 필터로 제한. 허용/거부 평가는 서버에서 수행하며 화면은 결과를 대체하지 않는다.

---

### PLT-AUD-001 — 권한 감사 관리 (`Authorization Audit Management`)

**목적**

권한 평가 감사 기록을 조회하고 감사 데이터의 무결성 및 보존 작업을 운영한다.

**주요 기능**
- 감사 기록 페이지 조회
- 사용자·Action·허용 여부·기간 필터
- 무결성 계산
- 아카이브·보존 운영(해당 API 확인 후 활성화)

**검색 조건**
- SubjectId
- Action
- IsAllowed
- FromUtc
- ToUtc
- 페이지·페이지 크기

**입력 항목**
- TenantId
- SubjectId(선택)
- Action(선택)
- IsAllowed(선택)
- FromUtc·ToUtc
- 무결성 계산 시작·종료 시각

**출력 항목**
- 감사 ID
- SubjectId
- Action
- ResourceType
- Scope
- IsAllowed
- ReasonCode
- MatchedPolicyId
- OccurredAtUtc
- 무결성 결과

**사용자 권한**
- TenantAdministrator: 감사 조회·무결성 계산(확인됨)
- Auditor: 읽기 전용(역할·인가 추가 필요)

**API 구현 근거**
- 구현 상태: **조회·무결성 확인**
- GET /tenants/{tenantId}/authorization-audit
- GET /tenants/{tenantId}/authorization-audit/integrity
- AuthorizationAuditQueryService

**추가 구현·확인 사항**
- 아카이브·보존 작업의 실제 API·권한 재검증
- 감사 상세·내보내기 필요성 결정
- 조회 범위와 보존 데이터 접근 통제

**설계 비고**

감사 데이터는 읽기 전용으로 다룬다. 아카이브·보존 기능은 실제 엔드포인트 및 권한을 확인한 후 명세에 추가.

---

### PLT-QTA-001 — 저장 용량 할당량 관리 (`Storage Quota Management`)

**목적**

테넌트·조직·프로젝트·데이터셋 저장 한도와 경고 임계값 정책을 관리한다.

**주요 기능**
- 정책 페이지 조회
- 정책 생성·수정
- 활성·비활성 전환
- 정책 변경 이력 조회

**검색 조건**
- 정책 ID
- Scope
- TargetId
- 플랜 코드
- 페이지·페이지 크기

**입력 항목**
- Scope
- TargetId
- TenantLimitBytes
- OrganizationLimitBytes
- ProjectLimitBytes
- DataSetLimitBytes
- WarningThresholdPercent
- PlanCode
- ExpectedVersion(수정 시)
- IsEnabled(상태 변경 시)

**출력 항목**
- 정책 ID·범위·대상
- 각 한도(Byte)
- 경고 임계값
- PlanCode
- IsEnabled
- Version
- 변경 이력

**사용자 권한**
- TenantAdministrator: 정책 조회·생성·수정·활성 변경·이력(확인됨)

**API 구현 근거**
- 구현 상태: **CRUD 일부 확인**
- GET/POST/PUT/PATCH /tenants/{tenantId}/storage-quota-policies
- GET .../{policyId}/history

**추가 구현·확인 사항**
- 사용량 집계 화면과 정책 편집 화면 분리 여부
- 범위별 TargetId 검증 및 충돌 메시지 UX

**설계 비고**

수정·활성 변경은 ExpectedVersion 기반 충돌 처리. API의 page/pageSize를 UI에 반영.

---

### DSET-DSM-001 — 데이터셋 관리 (`DataSet Management`)

**목적**

테넌트·조직·프로젝트 범위의 데이터셋 메타데이터와 생명주기 상태를 관리한다.

**주요 기능**
- 데이터셋 생성
- 데이터셋 상세 조회
- 데이터셋 활성화
- 스키마 개정 정보 확인

**검색 조건**
- 데이터셋 ID
- 조직·프로젝트 ID
- 이름
- 저장 어댑터
- 콘텐츠 형식
- 상태

**입력 항목**
- OrganizationId(선택)
- ProjectId(선택)
- Name
- StorageAdapter
- ContentFormat

**출력 항목**
- DataSetId
- TenantId
- OrganizationId
- ProjectId
- Name
- StorageAdapter
- ContentFormat
- Status
- Version
- CurrentSchemaVersion
- SchemaRevisions
- CreatedAtUtc

**사용자 권한**
- TenantAdministrator: 데이터셋 생성(확인됨)
- TenantMember: 데이터셋 상세 조회(확인됨, 테넌트 멤버십 경계)

**API 구현 근거**
- 구현 상태: **생성·상세 확인**
- POST /api/platform/tenants/{tenantId}/datasets
- GET /.../datasets/{dataSetId}
- DataSetManagementService

**추가 구현·확인 사항**
- 데이터셋 전체 목록·검색 API 확인
- 수정·삭제·보관·복구 정책 및 API
- 활성화 API 인가 정책 확인

**설계 비고**

생성은 TenantAdministrator 필터, 상세는 TenantMember와 테넌트 멤버십 경계. 조직·프로젝트 관리자 역할은 추가 인가 확인 필요.

---

### DSET-SCHM-001 — 데이터셋 스키마 관리 (`DataSet Schema Management`)

**목적**

데이터셋 스키마 정의를 등록하고 불변 개정 이력을 확인한다.

**주요 기능**
- 스키마 정의 등록
- 현재 스키마 버전 확인
- 상세 조회에 포함된 개정 이력 표시
- 개정 전용 검색·상세(추가 API 확인)

**검색 조건**
- DataSetId
- SchemaVersion
- 개정 생성 기간

**입력 항목**
- DataSetId
- SchemaJson
- ExpectedDataSetVersion(계약 제공 시)

**출력 항목**
- DataSetId
- CurrentSchemaVersion
- SchemaRevision.Version
- SchemaJson
- CreatedAtUtc

**사용자 권한**
- TenantAdministrator: 스키마 변경(실제 API 인가 확인 필요)
- TenantMember: 데이터셋 상세 및 스키마 정보 조회

**API 구현 근거**
- 구현 상태: **상세 내 개정·등록 확인**
- DataSet 상세 응답의 CurrentSchemaVersion·SchemaRevisions
- DataSetEndpoints.cs의 스키마 등록 경로

**추가 구현·확인 사항**
- 스키마 등록 요청 DTO 및 권한 필터 재확인
- 개정 목록·상세 전용 API
- 스키마 비교·롤백 정책

**설계 비고**

기존 개정을 덮어쓰지 않고 개정 이력을 보존한다. SchemaJson 원문 노출 권한을 검토.

---

### DSET-STO-001 — 데이터셋 콘텐츠·저장소 관리 (`DataSet Storage Management`)

**목적**

데이터셋 콘텐츠 입출력과 저장소 사용량 및 할당량 정보를 확인한다.

**주요 기능**
- 콘텐츠 업로드
- 콘텐츠 조회·다운로드
- 데이터셋 사용량 조회
- 테넌트 저장소 사용량·이력·할당량 이벤트 조회

**검색 조건**
- DataSetId
- 기간
- 저장 어댑터
- 콘텐츠 형식
- 사용량 이벤트 유형

**입력 항목**
- DataSetId
- 업로드 콘텐츠 스트림
- 조회 기간(사용량 이력)

**출력 항목**
- 콘텐츠 응답
- 콘텐츠 크기(제공 시)
- 데이터셋 사용량
- 테넌트 저장소 사용량·이력
- 할당량 이벤트

**사용자 권한**
- TenantAdministrator: 콘텐츠 변경·업로드(업로드 API 인가 확인)
- TenantMember: 멤버십 범위의 콘텐츠 조회(조회 API 인가 확인)

**API 구현 근거**
- 구현 상태: **콘텐츠·사용량 확인**
- DataSetEndpoints.cs의 content·usage·storage-usage 관련 경로

**추가 구현·확인 사항**
- 파일 목록·버전·삭제·복구 API
- 용량 집계의 정확한 응답 필드 확인
- 콘텐츠 다운로드 권한과 마스킹 정책

**설계 비고**

업로드 크기 제한 및 할당량 초과 응답을 사용자에게 명확히 표시. 실제 저장 참조·내부 경로는 노출하지 않는다.

---

### DSET-VLD-001 — 데이터셋 검증 관리 (`DataSet Validation Management`)

**목적**

검증 프로파일과 개정을 관리하고 고정된 데이터셋·스키마·프로파일 버전으로 비동기 검증을 실행한다.

**주요 기능**
- 검증 프로파일 생성
- 프로파일 개정 생성
- 검증 실행 예약
- 실행 결과·집계 확인
- 실행 기록 삭제(완료 상태 정책 준수)

**검색 조건**
- DataSetId
- ProfileId
- ProfileRevision
- ExecutionId
- Status
- 생성·완료 기간
- FailureCode

**입력 항목**
- Profile Name
- DefinitionJson
- DataSetId
- ExpectedDataSetVersion
- SchemaVersion
- ProfileId
- ProfileRevision
- Idempotency-Key

**출력 항목**
- ProfileId·Revision
- ExecutionId
- Status
- ProcessedRecords
- TotalRecords
- ErrorCount
- WarningCount
- DetailItemCount
- DetailTruncated
- RuleCountsJson
- FailureCode·FailureMessage
- CreatedAtUtc·CompletedAtUtc
- WorkerTaskId

**사용자 권한**
- TenantAdministrator: 프로파일·실행 관리(확인됨)
- TenantMember: 결과 조회는 실제 조회 API 인가 확인 필요

**API 구현 근거**
- 구현 상태: **프로파일·실행 생성 확인**
- DataSetValidationEndpoints.cs의 validation-profiles·revisions·validations 생성·삭제 경로

**추가 구현·확인 사항**
- 실행 결과 목록·상세 GET API 확인
- 프로파일 목록·상세 GET API 확인
- 검증 결과 권한 및 Worker Task 연결 조회

**설계 비고**

실행 요청은 ExpectedDataSetVersion·SchemaVersion·ProfileRevision 고정. Idempotency-Key 필수. 지원 범위는 Validation v1으로 제한.

---

### WRK-TSK-001 — 작업 모니터링 (`Worker Task Monitoring`)

**목적**

테넌트 비동기 작업의 등록 및 개별 실행 상태를 모니터링한다.

**주요 기능**
- 작업 등록
- 작업 상세 조회
- 상태·재시도·오류 확인
- 전체 작업 목록·필터(추가 API 필요)

**검색 조건**
- TaskId
- TaskType
- Status
- 생성 기간
- 오류 코드

**입력 항목**
- TaskType
- PayloadReference
- MaxAttempts(기본 5)
- Idempotency-Key

**출력 항목**
- TaskId
- TaskType
- Status
- CreatedAtUtc
- 시도·재시도 정보(상세 응답 제공 범위)

**사용자 권한**
- TenantAdministrator: 작업 등록·상세 조회(현재 경로의 관리자 필터 확인)
- WorkerOperator: 별도 역할·정책 추가 필요

**API 구현 근거**
- 구현 상태: **등록·상세 확인**
- POST /worker/tasks
- GET /worker/tasks/{taskId}
- WorkerTaskQueryService

**추가 구현·확인 사항**
- 작업 목록·검색·정렬·기간 필터 API
- 취소·강제 재시도 정책 및 API
- 상세 응답 필드 계약 확정

**설계 비고**

PayloadReference는 보안 참조로만 취급하고 본문·비밀값을 화면에 노출하지 않는다. Idempotency-Key 필수.

---

### WRK-DLQ-001 — 실패 작업 관리 (`Dead Letter Queue Management`)

**목적**

DLQ에 격리된 실패 작업을 조회하고 멱등성 식별자를 사용해 재처리한다.

**주요 기능**
- DLQ 페이지 조회
- 작업 실패 정보 확인
- 작업 재처리
- 재처리 이력 표시(추가 API 권장)

**검색 조건**
- TaskId
- TaskType
- Status
- LastErrorCode
- 생성 기간

**입력 항목**
- TaskId
- Replay-Request-Id(GUID 필수)
- 재처리 사유(화면 감사용 제안)

**출력 항목**
- TaskId
- TaskType
- Status
- AttemptCount
- MaxAttempts
- LastErrorCode
- CreatedAtUtc
- NextAttemptAtUtc
- OriginalTaskId
- ReplayTaskId

**사용자 권한**
- TenantAdministrator: DLQ 조회·재처리(확인됨)
- WorkerOperator: 별도 역할·정책 추가 필요

**API 구현 근거**
- 구현 상태: **목록·재처리 확인**
- GET /worker/dead-letters
- POST /worker/dead-letters/{taskId}/replay

**추가 구현·확인 사항**
- 재처리 이력·사유 저장 API
- 재처리 가능 상태·오류 원인 사전 점검 UX

**설계 비고**

Replay-Request-Id는 필수 GUID. 재처리는 외부 부작용 및 중복 실행 위험 확인 후 수행.

---

### WRK-XDL-001 — 외부 전달 모니터링 (`External Delivery Monitoring`)

**목적**

외부 HTTP·HTTPS 수신기로 전달할 이벤트를 생성하고 개별 전달 상태를 확인한다.

**주요 기능**
- 외부 전달 생성
- 전달 상세 조회
- 시도 횟수·오류·완료 상태 확인
- 전달 목록·검색(추가 API 필요)

**검색 조건**
- DeliveryId
- TargetUri
- Status
- 오류 코드
- 생성·완료 기간

**입력 항목**
- TargetUri
- Payload(JSON)
- MaxAttempts(기본 5)
- Idempotency-Key

**출력 항목**
- DeliveryId
- TenantId
- TargetUri
- Status
- AttemptCount
- MaxAttempts
- LastErrorCode
- CreatedAtUtc
- CompletedAtUtc
- WorkerTaskId(생성 응답)

**사용자 권한**
- TenantAdministrator: 전달 생성(확인됨)
- TenantMember: 전달 상세 조회(확인됨)
- WorkerOperator: 운영 역할 추가 필요

**API 구현 근거**
- 구현 상태: **생성·상세 확인**
- POST /worker/deliveries
- GET /worker/deliveries/{deliveryId}

**추가 구현·확인 사항**
- 전달 목록·검색 API
- 전용 재시도 API 및 정책
- TargetUri 허용목록·SSRF 방어 정책 확인

**설계 비고**

Idempotency-Key 필수. 동일 키에 다른 TargetUri·Payload·MaxAttempts 사용 시 충돌. Payload 및 URI의 민감정보 노출을 제한.


## 5. 화면 간 업무 이동 기준

1. 테넌트 관리에서 테넌트를 선택한 뒤 조직·사용자·멤버십·권한·감사·할당량 관리로 이동한다.
2. 조직 관리에서 프로젝트 관리로 이동하고, 프로젝트에서 데이터셋 관리로 진입한다.
3. 데이터셋 관리에서 스키마·콘텐츠·검증 기능으로 이동한다.
4. 검증 실행에서 생성된 Worker 작업을 작업 모니터링으로 추적한다.
5. 실패 작업은 DLQ 관리에서 원인을 확인하고 재처리한다.
6. 외부 전달은 별도 전달 상세 화면에서 상태와 오류를 확인한다.

이 연결은 업무 Navigation 제안이며 실제 화면 라우팅 또는 View 연결을 의미하지 않는다.

## 6. 구현 순서 권장

| 순서 | 구현 범위 | 화면 ID |
|---:|---|---|
| 1 | 테넌트·조직·프로젝트 기반 | PLT-TNT-001, PLT-ORG-001, PLT-PJT-001 |
| 2 | 사용자·멤버십 | PLT-USR-001, PLT-MBR-001 |
| 3 | 권한·감사·할당량 | PLT-AUTH-001, PLT-AUD-001, PLT-QTA-001 |
| 4 | 데이터셋·스키마·저장소 | DSET-DSM-001, DSET-SCHM-001, DSET-STO-001 |
| 5 | 검증 | DSET-VLD-001 |
| 6 | 작업·실패 작업·외부 전달 | WRK-TSK-001, WRK-DLQ-001, WRK-XDL-001 |

우선순위는 의존 관계에 따른 권장안이며 구현 착수 승인이나 일정 확정을 의미하지 않는다.

## 7. 완료 및 검증 기준

- 15개 ScreenId가 기존 `screens.csv`와 일치한다.
- 본 문서와 CSV에 15개 화면이 각각 한 번씩만 존재한다.
- MODULE·AREA 코드는 기존 코드 사전의 승인 코드와 일치한다.
- 현재 Registry의 15개 화면 상태는 기존 코드와 문서의 `InDevelopment`를 유지한다. WPF smoke와 테스트host 실행 증거 없이 `Active`로 승격하지 않는다.
- API 근거는 실제 Endpoint·Service에서 확인된 범위만 기재한다.
- 제안 역할을 실제 인증 정책으로 잘못 표시하지 않는다.
- View·ViewModel·Service 경로 및 바인딩은 실제 구현 확인 후에만 별도 Registry에 등록한다.
- 기능 구현 시 Request/Response DTO, Endpoint Filter, DI 등록, 테스트를 함께 확인하고 이 문서의 구현 격차를 갱신한다.

## 8. 변경 이력

| 버전 | 내용 |
|---|---|
| 1.0 | 기존 15개 화면 후보에 대한 목적·기능·검색·입력·출력·권한·API 격차 정의 |

## 9. API·DTO 정합성 조사 부록 (2026-11-15)

이 부록은 `main-auto`의 실제 소스와 테스트를 기준으로 갱신했다. 화면 UI·ViewModel 구현을 의미하지 않으며, 상세 필드·소스 경로·심볼·테스트 근거는 [screen-api-mapping.csv](screen-api-mapping.csv)와 [screen-api-validation.csv](screen-api-validation.csv)에 기록한다.

| ScreenId | 실제 확인된 API 범위 | 실제 상태 | 조사 결과의 핵심 |
|---|---|---|---|
| PLT-TNT-001 | tenant 생성·활성화·directory·mine membership | Implemented | directory는 PlatformAdministrator의 search/status/page/pageSize 계약이며 mine은 활성 소속 배열 |
| PLT-ORG-001 | organization 생성·목록·상세·수정·상태 | Implemented | ID/date filter는 실제 source contract에 없음 |
| PLT-PJT-001 | project 생성·목록·상세·수정·상태 | Implemented | ID/date filter는 실제 source contract에 없음 |
| PLT-USR-001 | user invitation 생성·invitation 검색·재전송·철회·token 수락·directory 검색·상태 변경 | Implemented | Invitation aggregate는 token hash만 저장하고 raw token은 Delivery Adapter로 전달 |
| PLT-MBR-001 | tenant/org/project membership 부여·목록·mine 조회·역할 변경·회수 | Implemented | query-enabled 목록은 subject/status/page/pageSize를 지원; mine은 활성 배열 |
| PLT-AUTH-001 | authorization policy 생성·목록·필터·평가·수정·enabled 변경 | Implemented | evaluation과 tenant-scoped page filter route 추가 |
| PLT-AUD-001 | audit query·integrity·archive·archive download·schedule·job·retention·job 목록 | Implemented | archive download는 tenant 및 checksum 검증 후 제공 |
| PLT-QTA-001 | quota CRUD·enabled·history·scope/target/enabled filter | Implemented | PlanCode는 응답 필드이며 별도 query 의미는 없음 |
| DSET-DSM-001 | dataset 생성·목록·상세·content·activate·rename·archive·restore·delete | Implemented | lifecycle route와 storage reset 연결 |
| DSET-SCHM-001 | schema 추가·상세·전용 revisions 목록·diff·rollback | Implemented | version/date filter는 실제 계약에 없음 |
| DSET-STO-001 | content·dataset/tenant usage·history·quota events·storage reset | Implemented | aggregate usage list filter는 실제 계약에 없음 |
| DSET-VLD-001 | profile/revision·profile/execution 목록·validation 실행·상세·details·삭제·profile lifecycle | Implemented | date/failureCode execution filter와 profile update/delete 연결 |
| WRK-TSK-001 | worker task 생성·목록·상세·cancel·force retry·result metadata/reference·raw result upload/download | Implemented | IWorkerResultStore와 개발용 파일 Adapter가 Tenant·Task 경계를 검증한 뒤 bytes를 스트리밍 |
| WRK-DLQ-001 | DLQ 목록·필터·replay·replay history | Implemented | taskId/status는 item/detail 계약 |
| WRK-XDL-001 | external delivery 생성·목록·상세·필터·수동 retry | Implemented | safe projection만 반환하고 payload는 비노출 |

### 9.1 실제 계약 정정 사항

- `PLT-TNT-001`의 `GET /tenants/mine`은 전체 tenant DTO가 아니라 `TenantId`, `Role`만 제공한다. 생성은 `CreateTenantRequest(Code, Name)`, 활성화는 request body가 없다.
- `PLT-AUTH-001`의 생성 DTO는 `OrganizationId?`, `ProjectId?`, `SubjectId`, `Action`, `ResourceType`, `Effect`, `Priority`이며, 목록에는 `IsEnabled`, `CreatedAtUtc`가 추가된다. 목록은 organization/project/subject/action/effect/enabled/page/pageSize를 지원하고 평가 결과는 `IsAllowed`, `ReasonCode`, `MatchedPolicyId`다.
- `PLT-QTA-001`은 `ExpectedVersion`을 사용하는 optimistic concurrency와 page/history를 실제 지원하며 scope/isEnabled filter가 추가됐다. target/plan query는 실제 계약에 없다.
- `DSET-DSM-001`은 tenant-scoped collection에 organization/project/search/status/page/pageSize query를 제공하고, 생성·상세·content·activate 경로를 유지한다. 상세에 `SchemaRevisions`가 내장된다.
- `DSET-SCHM-001`은 `GET /datasets/{dataSetId}/schemas`의 `TotalCount`, `Page`, `PageSize`, `Items[Version; SchemaJson; CreatedAtUtc]` page contract를 사용한다.
- `DSET-STO-001` history는 from/to/take, quota event는 scope/eventType/from/to/take를 사용한다.
- `DSET-VLD-001` profile list는 search/page/pageSize, execution list는 dataSetId/profileId/status/page/pageSize를 사용한다.
- `PLT-USR-001`은 invitation metadata page, resend, revoke, token-only accept를 제공하며 raw token을 API 응답에 넣지 않는다.
- `WRK-TSK-001`, `WRK-DLQ-001`, `WRK-XDL-001`은 각각 task list/raw result, DLQ filter/replay history, delivery list/filter/manual retry route를 제공하며 payload와 checkpoint 본문은 반환하지 않는다.
- `DSET-VLD-001` 실행 생성은 `Idempotency-Key`와 dataset version/schema/profile revision을 요구하고, 실행 상세와 details는 서로 다른 응답 구조를 사용한다.
- `WRK-DLQ-001` replay는 `Replay-Request-Id` GUID를 요구하고 원본 task를 보존한 새 task ID를 반환한다.
- `WRK-XDL-001` 응답은 내부 payload를 반환하지 않는다. 자동 retry는 Worker 내부 동작이며 admin 수동 retry는 `POST /worker/deliveries/{deliveryId}/retry`에서 새 worker task를 등록한다.

### 9.2 인증·인가·DI·테스트 확인

- 외부 JWT issuer/audience/lifetime/signing key 검증, `TenantMember`/`PlatformAdministrator` 정책, `X-Tenant-Id`와 route tenant 비교, active membership 및 TenantAdministrator 검증을 실제 소스에서 확인했다.
- API Application Service의 DI 등록과 WPF View/ViewModel의 `IDesktopScreenService` → `DesktopApiSession` 바인딩을 실제 소스에서 확인했다. API 서비스 등록을 UI 바인딩으로 추정하지 않는다.
- `dotnet build EDAN.Tensor.sln --configuration Release --no-restore -m:1`은 오류 0건으로 완료했다. 원격 GitHub Actions run `43736265722`에서 전체 `dotnet test`, HTTP smoke 2건, WPF UI smoke 1건이 모두 통과했다. 로컬 testhost 연결 실패는 별도 환경 미확인으로 기록한다.
- 테스트 및 상세 격차·우선순위·UI 단계 판단은 [screen-implementation-gaps.md](screen-implementation-gaps.md)에 고정했다.

### 9.3 조사 후 관리 원칙

1. 기존 설계 기능은 API가 제공하지 않는 필드를 실제 제공 필드처럼 표시하지 않는다.
2. `NotImplemented`와 `NotVerified`를 구분하고, 테스트를 실행하지 않은 경우 `NotRun` 또는 `NotFound`로 기록한다.
3. ScreenId 및 MODULE·AREA 코드는 변경하지 않는다.
4. UI 아키텍처는 실제로 연결된 15개 화면의 공통 경계를 따르되, API가 없는 lifecycle/control은 구현하지 않는다. 확인된 mutation은 공통 명령 영역으로 연결하고, 구조화 입력과 UI smoke/test 증거를 확보한 뒤 `Active`로 승격한다.

### 9.4 부록 변경 이력

| 버전 | 내용 |
|---|---|
| 1.1 | 15개 화면 API·DTO 전수 조사 결과, 실제 계약 정정, 테스트 결과 및 구현 격차 문서 연결 |
| 1.2 | collection/filter/retry/evaluation/membership 계약과 화면 연결 추가, 실제 테스트호스트 미확인 상태 반영 |

### 9.5 2026-11-15 추가 구현 계약 우선순위

아래 항목은 본 문서 앞부분의 설계 제안 중 실제 소스에 추가로 확정·구현된 계약이다. 상세 필드와 근거는 `screen-api-mapping.csv`의 API ID를 우선한다.

| 화면 | 추가 확정 계약 |
|---|---|
| PLT-MBR-001 | Tenant·조직·프로젝트 membership query 목록(`subjectId`, `status`, `page`, `pageSize`)과 각 범위의 `PATCH` 역할 변경·회수. 기존 no-query 배열과 `mine` 활성 배열은 호환성 때문에 유지 |
| PLT-AUTH-001 | 정책 필터·페이징과 `POST .../authorization-policies/evaluate`의 `AuthorizationDecision` |
| PLT-AUD-001 | `GET .../authorization-audit/archive-jobs`의 status·UTC 기간·페이징 |
| PLT-QTA-001 | quota `scope`, `isEnabled`, `page`, `pageSize` 필터 |
| DSET-DSM-001 | DataSet collection의 organization/project/search/status/page/pageSize |
| DSET-SCHM-001 | 전용 schema revision page |
| DSET-STO-001 | history 기간 필터와 quota-event scope/eventType/기간 필터 |
| DSET-VLD-001 | validation profile/execution collection page |
| WRK-TSK-001 | task collection의 taskType/status/UTC 기간/page 필터 |
| WRK-DLQ-001 | DLQ type/error/UTC 기간 필터와 replay history page |
| WRK-XDL-001 | delivery collection filter와 admin manual retry |

### 9.6 2026-11-16 확정·구현 계약

| 화면 | 추가 구현 계약 |
|---|---|
| PLT-USR-001 | `Invitation` aggregate, SHA-256 token hash, Pending/Accepted/Expired/Revoked lifecycle, 개발 전달 Adapter, resend/revoke/accept API |
| WRK-TSK-001 | `IWorkerResultStore`, 개발용 `FileSystemWorkerResultStore`, 성공 작업의 raw result 저장·metadata·streaming API, 64 MiB 상한 |

HTTP smoke와 WPF STA smoke는 별도 테스트 프로젝트/CI 단계에 등록했고, 원격 CI run `43736265722`에서 통과했다. action-level UI 명령은 해당 smoke 범위에 포함되지 않으므로 관련 UI 검증 행은 `NotRun`으로 유지한다.

이 추가 계약은 UI에서 실제 typed Client 호출과 공통 대상 선택·구조화 Request DTO 입력으로 연결되어 있다. 다만 WPF 화면의 직접 실행 smoke는 테스트host 연결 실패로 확인하지 못했으므로 화면 Registry 상태는 `InDevelopment`를 유지한다.
