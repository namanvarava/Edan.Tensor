# EDAN.Tensor 화면 구현 확정 계약

- 확정일: 2026-11-15
- 적용 브랜치: `main-auto`
- 적용 범위: 기존 Screen Registry 15개
- 구현 기술: .NET 10 WPF Desktop
- UI 상태: `View → ViewModel → 화면 Service 계약 → EDAN.Tensor.Client → API`

## 1. 이번 작업에서 확정한 결정

1. 기존 Registry의 ScreenId와 MODULE·AREA 코드는 변경하지 않는다.
2. 15개 화면을 하나의 WPF Desktop 프로젝트에 구현한다. 화면마다 대표 View, ViewModel, 화면 Service 계약을 둔다.
3. View는 API Client, Repository 또는 concrete Application Service를 직접 참조하지 않는다.
4. API가 이미 제공하는 필드는 typed Client 계약으로 연결하고, 화면 요구에 필요한 목록·상세·검색·lifecycle이 없으면 API 계약과 서버 테스트를 먼저 추가한다.
5. 모든 Tenant 범위 호출은 route `tenantId`, `X-Tenant-Id`, 인증 주체, active membership 및 필요한 TenantAdministrator 검사를 유지한다.
6. 서버가 제공하지 않는 필드는 로컬 임시 필드나 추정 DTO로 채우지 않는다. API가 보완되기 전에는 화면에서 동작을 비활성화하고 상태를 표시한다.
7. `PLT-AUD-001`의 감사 운영 API, `PLT-QTA-001`의 page/history, `DSET-VLD-001`의 idempotency, `WRK-DLQ-001`의 replay, `WRK-XDL-001`의 opaque payload 경계는 현재 계약을 유지한다.
8. WPF 화면 구현은 API 보완과 함께 진행하되, UI 완료 상태는 전체 화면의 build·unit test·API integration test·registry path 검증을 통과한 뒤에만 표시한다.

## 2. API 보완 범위

### P1

- Platform: tenant/org/project/user/membership 목록·상세와 허용된 lifecycle/역할 변경 계약
- Authorization: 정책 필터 및 화면용 평가 조회 범위
- DataSet: dataset 목록, schema revision 조회, validation profile/execution 목록
- Worker: task 목록·필터, DLQ filter/history, external delivery 목록·상태 조회

### P2

- quota, storage usage, DLQ의 범위 필터와 paging 상한
- audit archive 목록/검색 또는 archive ID 중심 운영 화면 중 하나를 확정
- anonymous projection을 안정적인 named response 계약으로 승격

기존 Domain 모델이 제공하지 않는 business rule은 UI에서 임의로 만들지 않는다. 필요한 lifecycle은 Domain method, Application Service, repository, endpoint, Client contract, test 순서로 추가한다.

## 3. 화면 수용 기준

- 15개 View와 15개 ViewModel이 Registry의 ScreenId와 일치한다.
- 화면 Service 바인딩은 `screen-service-bindings.csv`에 실제 코드와 함께 기록한다.
- navigation에서 각 ScreenId를 선택할 수 있고, tenant 선택·인증 오류·API 오류·취소를 공통 처리한다.
- 목록 화면은 서버 paging/filter 계약을 사용하며, API가 지원하지 않는 로컬 필터를 추가하지 않는다.
- Create/Update/Activate/Replay/Delete 같은 명령은 성공·실패·optimistic concurrency·idempotency 결과를 사용자에게 표시한다.
- 테스트 실행 전에는 `Passed`를 문서에 기록하지 않는다.

## 4. 제외 범위

- Commercial 결제 성공·Webhook·환불·유료 entitlement
- Licensing 제품 API 강제 계약
- DuckDB·Parquet 저장소
- 운영 IdP와 운영 DB

제외 범위는 이번 WPF Shell의 navigation 항목으로 임의 추가하지 않는다.

