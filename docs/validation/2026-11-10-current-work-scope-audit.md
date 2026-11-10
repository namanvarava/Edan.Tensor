# 2026-11-10 현재 Work 범위 감사

이 문서는 PR #162 기능 병합 커밋 `21ecd234e2dda4e34ab896bd36f3b96be2e07085`를 기준으로 현재 Work에서 실제 확인할 수 있는 제품 범위와 아직 구현·검증할 수 없는 조건을 구분합니다. 테스트 통과나 프로젝트 존재만으로 미구현 업무를 완료로 판정하지 않습니다.

## 감사 결과

| 영역 | 현재 상태 | 확인 근거 | 다음 완료 조건 |
| --- | --- | --- | --- |
| Platform API | 구현·검증 완료 | Tenant·Organization·Project, 정책, 감사·아카이브, 저장 용량 정책 API와 통합 테스트 | 운영 DB와 실제 배포 환경에서 Migration·백업·복구 재검증 |
| Tenant·인증·인가 | 내부 경계 검증 완료 | 경로 Tenant, `X-Tenant-Id`, 활성 소속, 관리자·조직·프로젝트 범위, External JWT/JWKS·키 회전 로컬 테스트, Development Bypass 차단 테스트 | 실제 비운영 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한 제공 |
| DataSet SQLite | 구현·검증 완료 | Tenant 소유 메타데이터, Schema revision, 콘텐츠 격리, UTF-8 CSV·JSON 객체 배열 검증, Quota, Fencing, SQLite 백업·복구 | 운영 저장소 공급자와 대용량·장애 복구 실행 환경 제공 |
| DataSet DuckDB·Parquet | 미구현 | README와 v1 계약이 다음 릴리스로 명시하며 현재 저장소에 승인된 어댑터 계약이 없음 | 포맷, 스키마, 읽기·쓰기, 원자성, quota, 복구 수용 기준 승인 |
| Worker | 구현·검증 완료 | 전용 DB, 작업 카탈로그, Lease/Fencing, 체크포인트, 재시도, DLQ, 재처리, 종료 인계, 강제 종료 복구, DataSet Validation Handler | 실제 운영 외부 수신 시스템의 응답 유실·중복·재시도 종단 간 환경 제공 |
| Client API | 현재 가능한 기반 구현 | `EDAN.Tensor.Client.Contracts`와 typed HTTP Client가 Bearer·Tenant·Idempotency·재처리 헤더와 구조화 오류를 보존 | 승인된 WPF 화면·업무 흐름 계약 |
| WPF | 미구현 | 현재 저장소에는 WPF 프로젝트와 승인된 화면·상태·권한 계약이 없음 | 화면·업무 흐름·배포 대상·수용 기준 승인 |
| Commercial | 모듈 경계만 존재 | Contracts·Domain·Application·Infrastructure가 AssemblyMarker만 제공하며 상품·요금·결제·Webhook·entitlement 업무가 없음 | 상품·요금·결제 공급자·Webhook·Tenant 권리·취소·환불 계약 승인 |
| Licensing·LicenseAuthority | 업무 기능 미구현 | Licensing 네 프로젝트와 LicenseAuthority Host는 경계만 있으며 발급 규칙·서명 포맷·저장 계약이 없음 | 사용자가 승인한 발급 규칙, LicenseId·Site/Tenant·제품·기능·기간·KeyId·폐기 규칙과 키 보관 정책 확정. 비라이선스 범위 안정 후 마지막으로 구현 |
| 운영·외부 검증 | 환경 미검증 | 실제 운영 DB, 외부 수신 시스템, GitHub main 보호 규칙 API 접근이 현재 Work에 없음 | 외부 시스템·권한·비용·서비스 계정 제공 후 런북 실행 |

## 현재 내부 검증 증거

필수 Release 명령은 최신 main에서 다음 결과를 반환했습니다.

```text
dotnet restore EDAN.Tensor.sln                         성공
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
  경고 0개·오류 0개
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
  통과 246개·실패 0개·건너뜀 0개
```

추가 경계 테스트도 다음과 같이 통과했습니다.

- 인증·JWKS·키 회전·Development Bypass: 12개 통과
- Worker 강제 종료·Lease/Fencing·종료 인계·저장 실패 복구: 24개 통과
- DataSet SQLite·Tenant 격리·Quota·Fencing·백업/복구: 11개 통과
- 알려진 취약 패키지 감사: `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`에서 취약 패키지 없음
- 실제 Release Host Smoke: 임시 SQLite 네 개로 ApiHost·WorkerHost를 기동하여 health, database health, OpenAPI, Swagger UI, Development Bypass Tenant API가 모두 HTTP 200; 양쪽 stderr 0바이트; 종료 후 프로세스·포트·임시 경로 없음

## 인증 우회와 외부 경계

Development Bypass는 고정된 비운영 Tenant·사용자·`sub`를 사용하는 로컬 개발 설정에서만 활성화됩니다. External·운영 환경은 Issuer·Audience·Metadata·JWKS·HTTPS·서명 검증을 유지하며 Bypass가 운영 인증을 대체하지 않습니다. CI와 배포 구성에는 Bypass를 성공 조건으로 포함하지 않습니다.

실제 외부 IdP와 GitHub 보호 규칙은 Issue #84 및 Milestone #1에서 추적합니다. GitHub main 보호 규칙 API는 현재 HTTP 403 `Resource not accessible by integration`으로 조회가 거부되었으므로 환경 미검증으로 남깁니다. 토큰·개인키·비밀번호는 이 문서와 저장소에 기록하지 않습니다.

## 다음 실행 순서

1. 승인된 DuckDB·Parquet·Commercial·WPF 계약이 제공되면 비라이선스 기능을 먼저 구현하고 각 수용 기준을 검증합니다.
2. 그 범위가 안정된 뒤 승인된 발급 규칙으로 별도 LicenseAuthority와 개발 라이선스 1개를 구현합니다.
3. 라이선스 적용 API에서 정상·위조·만료·시작 전·다른 Tenant·폐기·알 수 없는 `KeyId`를 검증합니다.
4. 외부 IdP·운영 DB·외부 수신 시스템·배포·GitHub 보호 규칙은 조건이 제공된 뒤 별도 실행합니다.

계약 또는 외부 환경이 제공되기 전에는 해당 범위를 임의 구현하거나 완료로 표시하지 않습니다. 중단이 발생하면 재현, 영향, 로그·코드·설정 근거, 원인과 대안, 최소 수정, 대상·전체 회귀 테스트, 재발 방지, 증거 기록 순서로 처리합니다.

## 2026-11-10 typed Client 삭제 계약 병합 후 재검증

PR #153의 [PR CI Run #926](https://github.com/namanvarava/Edan.Tensor/actions/runs/42715584623)과 Squash Merge 후 [main CI Run #927](https://github.com/namanvarava/Edan.Tensor/actions/runs/42715896376)이 자체 호스팅 Windows Runner에서 Restore·Release Build·Release Test를 모두 성공했다. 병합된 main 커밋은 `7b1ca7a7fdaa1bd2fc6a760e153f53b8896d27b5`이며, 로컬에서 동일한 Release 명령을 재실행하여 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 243개 통과·실패 0개·건너뜀 0개를 확인했다.

변경은 기존 승인된 `DELETE /api/platform/tenants/{tenantId}/datasets/validations/{executionId}` 경로를 `TensorApiClient.DeleteDataSetValidationAsync`와 단위·실제 API Host 통합 회귀에 연결한 범위다. Commercial·Licensing 업무, DuckDB·Parquet, WPF, 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 검증 범위는 확장하지 않았다.

## 2026-11-10 최신 main 문서 정합성 재검증

PR #155의 [PR CI Run #930](https://github.com/namanvarava/Edan.Tensor/actions/runs/42718223926)과 Squash Merge 후 [main CI Run #931](https://github.com/namanvarava/Edan.Tensor/actions/runs/42718607611)이 성공했다. 병합된 `main`은 `8139a07ebdbe85bb42acdabbc8a58ad1bb4983e9`이며, 문서의 현재 기준 SHA·테스트 구성·CI 증거를 최신 상태로 정정했다. 병합 후 로컬 Release Restore·Build·Test는 Restore 성공, Build 경고 0개·오류 0개, Test 243개 통과·실패 0개·건너뜀 0개를 기록했다.

이번 변경은 문서 정합성만 보정하며 DuckDB·Parquet·Commercial·Licensing·LicenseAuthority·WPF 업무 기능이나 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 검증을 완료 처리하지 않는다.

## 2026-11-10 최신 main OpenAPI 계약 회귀

PR #157의 [PR CI Run #934](https://github.com/namanvarava/Edan.Tensor/actions/runs/42721451802)와 Squash Merge 후 [main CI Run #935](https://github.com/namanvarava/Edan.Tensor/actions/runs/42721752528)이 성공했다. 병합된 `main`은 `a379c6077b671e27f74dda267ae3715dfc374e0c`이며, OpenAPI 통합 테스트가 Platform·DataSet·Validation·사용량·Worker·외부 전달 경로와 Tenant/Bearer/멱등성 헤더를 확인한다. 병합 후 로컬 Release Restore·Build·Test는 Restore 성공, Build 경고 0개·오류 0개, Test 243개 통과·실패 0개·건너뜀 0개를 기록했다.

이번 변경은 OpenAPI 회귀 증거만 보강하며 DuckDB·Parquet·Commercial·Licensing·LicenseAuthority·WPF 업무 기능이나 실제 외부 환경 검증을 완료 처리하지 않는다.

## 2026-11-10 외부 전달 취소 경계 보강

Worker 외부 전달 Handler가 조정기의 취소 토큰과 별도의 토큰으로 HTTP 호출을 시작하여 작업 취소·종료 중에도 외부 부작용이 계속될 수 있는 결함을 확인했다. 시작 전 취소를 즉시 거부하고, 조정기 취소와 종료 인계 취소를 동일한 외부 HTTP 실행에 연결하는 최소 수정으로 경계를 보강했다. 대상 회귀 테스트 9개와 Worker Infrastructure 전체 테스트 71개가 통과했으며, 시작 전 취소와 진행 중 HTTP 호출 취소 전파를 각각 확인했다.

이 수정은 외부 수신 시스템의 실제 응답·중복 처리 검증을 대체하지 않는다. 실제 외부 수신기와 운영 환경 검증은 Issue #84 및 Milestone #1의 외부 조건이 제공될 때 별도로 수행한다.

## 2026-11-10 PR #159 병합 후 Worker 취소 경계 증거

PR #159는 외부 전달 Handler의 조정기 취소 토큰을 실제 HTTP 실행에 연결하고, 시작 전 취소와 진행 중 호출 취소 전파 회귀 테스트를 추가하여 `52fa640cb833caff05947b35f3239ad71d11f4a4`로 Squash Merge되었다. PR CI [Run #938](https://github.com/namanvarava/Edan.Tensor/actions/runs/42724689384)와 병합 후 main CI [Run #939](https://github.com/namanvarava/Edan.Tensor/actions/runs/42724934985)이 자체 호스팅 Runner에서 Restore·Release Build·Release Test를 성공시켰다.

병합 후 최신 main에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행하여 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 확인했다. 이 증거는 외부 수신 시스템의 실제 중복 처리와 외부 IdP·운영 DB·GitHub 보호 규칙 검증을 포함하지 않는다.

## 2026-11-10 PR #162 저장량 조회 개수 계약 보강

저장량 스냅샷 이력과 quota 임계치 이벤트 API가 범위를 벗어난 `take` 값을 `Math.Clamp`로 조용히 보정하던 계약 불일치를 확인했다. 저장량 이력은 1~744, quota 이벤트는 1~500 범위로 명시적으로 검증하고, 범위를 벗어나면 HTTP 400 `api.invalid_request`를 반환하도록 최소 수정했다. Tenant 관리자 경계에서 네 가지 잘못된 입력을 확인하는 API 통합 회귀 테스트와 공개 API·운영 가이드를 추가했다.

PR #162의 head `61483289fd4319aba1ad69c7f482d31b30aab9e7`에 대한 [PR CI Run #944](https://github.com/namanvarava/Edan.Tensor/actions/runs/42730316819)와 Squash Merge 후 main 커밋 `21ecd234e2dda4e34ab896bd36f3b96be2e07085`의 [main CI Run #945](https://github.com/namanvarava/Edan.Tensor/actions/runs/42730617911)이 Restore·Release Build·Release Test를 성공시켰다. 병합 후 로컬에서도 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 재확인했다. 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 여전히 Issue #84의 외부 조건이다.
