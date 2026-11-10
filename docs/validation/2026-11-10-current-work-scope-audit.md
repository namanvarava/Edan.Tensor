# 2026-11-10 현재 Work 범위 감사

이 문서는 PR #198까지 병합된 최신 main 커밋 `908a0f303bf3bb73d9f53ffa72c86482bb9070e2`를 기준으로 현재 Work에서 실제 확인할 수 있는 제품 범위와 아직 구현·검증할 수 없는 조건을 구분합니다. 아래의 과거 PR·CI 항목은 당시의 증거를 보존한 역사 기록이며, 최신 제품 코드 판정에는 최신 main 검증 항목을 사용합니다. 테스트 통과나 프로젝트 존재만으로 미구현 업무를 완료로 판정하지 않습니다.

## 최신 main 판정

PR #198은 `908a0f303bf3bb73d9f53ffa72c86482bb9070e2`로 Squash Merge되었다. PR CI Run #42820375842와 병합 후 main CI Run #42820650083이 Restore·Release Build·Release Test·정리 단계까지 모두 성공했다. 실행 상세는 [PR #198](https://github.com/namanvarava/Edan.Tensor/pull/198)과 [main CI Run #42820650083](https://github.com/namanvarava/Edan.Tensor/actions/runs/42820650083)에서 확인할 수 있다.

최신 main에서 정확한 Release Restore·Build·Test를 실제 호스트로 다시 실행하여 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 262개 통과·실패 0개·건너뜀 0개를 확인했다. 변경 대상 API 통합 테스트는 37개 모두 통과했다. PR #198의 [CI Run #42820375842](https://github.com/namanvarava/Edan.Tensor/actions/runs/42820375842)와 병합 후 main의 [CI Run #42820650083](https://github.com/namanvarava/Edan.Tensor/actions/runs/42820650083)가 Restore·Build·Test·정리 단계까지 모두 성공했다. 최신 main에서 별도 LicenseAuthority 솔루션도 Restore 성공, Release Build 경고 0개·오류 0개, Test 10개 통과·실패 0개·건너뜀 0개를 재확인했다. Host Smoke는 ApiHost·WorkerHost의 health·database health·OpenAPI·Swagger UI·Development Bypass Tenant API를 모두 HTTP 200으로 반환하고 양쪽 stderr 0바이트와 임시 경로 삭제를 확인했다. LicenseAuthority HTTP Smoke는 개발 라이선스 단일 발급·5년 기간·공개 JWK·정상 검증 200·운영 환경 차단 403·폐기 검증 차단 403을 확인하고 토큰 로그와 임시 경로가 없음을 확인했다. Commercial 결제 경계는 공급자 미선정 시 fail-closed와 선정 공급자 위임을 2개 회귀 테스트로 확인했다. 최신 패키지 취약점 감사에서도 취약 패키지가 없었다. 이 증거는 승인된 내부 SQLite·Swagger/OpenAPI·Platform·DataSet·Worker·LicenseAuthority 범위와 Commercial 결제 경계의 현재 기준이다.

PR #197은 등록·활성화되지 않은 Worker `taskType`이 작업을 만들지 않고 HTTP 422와 `worker.enqueue.task_type_unavailable` 오류를 반환한다는 운영 API 계약을 문서와 통합 테스트에 고정했다. 이 변경은 기존 Tenant·인증·Worker 공개 계약을 확장하지 않으며, 제품 API 자체 라이선스 전달·공개키 공급·장애 시 fail-closed 계약은 별도 승인 전 상태로 유지한다.

## 2026-11-10 PR #168 DLQ 페이지 오버플로 API 회귀

typed Client의 페이지 오프셋 선행 검증에 더해 API Host 통합 테스트에서 `page=int.MaxValue&pageSize=100` 요청을 실제로 수행했다. 서버는 checked 오프셋 계산의 오버플로를 HTTP 400과 `worker.dead_letter.invalid_page` 오류 코드로 반환했으며, 이 검사는 서버·Client 양쪽 경계를 함께 고정한다.

PR #168의 head `cdf1d7f34b8af7679bd1fd62b81d8f3c8d8a3639`에 대한 [PR CI Run #956](https://github.com/namanvarava/Edan.Tensor/actions/runs/42738402094)와 Squash Merge 후 main 커밋 `bd4753f0c4ba41d906d6ca8556257eb3765db04d`의 [main CI Run #957](https://github.com/namanvarava/Edan.Tensor/actions/runs/42738642780)이 Restore·Release Build·Release Test를 성공시켰다. 병합 후 로컬에서도 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 재확인했다. 이 변경은 승인된 내부 SQLite·Swagger/OpenAPI·Platform·DataSet·Worker 범위만 보강하며, DuckDB·Parquet·Commercial·Licensing·LicenseAuthority·WPF와 실제 외부 환경 검증을 완료 처리하지 않는다.

## 2026-11-10 최신 제품 코드 기준 Host Smoke·의존성·GitHub 상태 재검증

PR #175가 병합한 최신 제품 코드 기준 `212f5b64a6d27ecd4ea21cb5f3597b1a461a5cf2`의 Release 바이너리를 임시 SQLite 네 개와 Development 환경의 고정 Bypass 프로필로 실제 기동했다. ApiHost의 `/health`, `/health/database`, `/openapi/v1.json`, `/swagger/index.html`, `/api/platform/tenants/mine`과 WorkerHost의 `/health`, `/health/database`가 모두 HTTP 200을 반환했다. 두 Host의 표준 오류 출력은 각각 0바이트였고, 검증 후 임시 Smoke 디렉터리를 삭제했다.

PR #175 자체 CI [Run #42748810618](https://github.com/namanvarava/Edan.Tensor/actions/runs/42748810618)와 병합 후 main CI [Run #42749039001](https://github.com/namanvarava/Edan.Tensor/actions/runs/42749039001)이 Restore·Release Build·Release Test를 성공시켰다. 병합 후 로컬에서도 전체 Release Test 248개 통과·실패 0개·건너뜀 0개와 Host Smoke를 재확인했다. 열린 Pull Request는 0개이고 원격 브랜치는 `main`만 남아 있다. GitHub Issue #84는 실제 비운영 외부 IdP와 main 보호 규칙 검증을 위한 Milestone #1에 열린 상태로 유지한다.

이 Smoke는 개발 전용 고정 Bypass와 loopback만 사용하므로 실제 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전, 운영 DB, 외부 수신 시스템, GitHub 보호 규칙 HTTP 403 검증을 대체하지 않는다.

## 2026-11-10 재현 가능한 Host Smoke 도구와 중단 복구

실제 Host Smoke를 반복할 때 수동 PowerShell 하네스가 재현 절차와 정리 증거를 흩어지게 만드는 공백을 확인하여 `tools/host-smoke.ps1`을 추가했다. 도구는 Release 산출물과 임시 SQLite 네 개를 사용하고, `Development` 환경의 고정 Bypass에서만 ApiHost·WorkerHost의 health, database health, OpenAPI, Swagger UI와 Tenant API를 확인한다. 운영·External·CI 설정에는 Bypass를 주입하지 않는다.

첫 실행은 Windows PowerShell 5.1의 `ProcessStartInfo.ArgumentList` 부재로 시작 단계에서 중단되었다. PowerShell 7만 요구하는 대안은 개발 환경 제약을 늘리므로 단일 DLL 인수를 `ProcessStartInfo.Arguments`에 따옴표로 전달하는 호환 수정을 선택했다. 두 번째 실행에서는 Host 응답은 성공했지만 `Process.Kill(bool)` 오버로드가 없어 정리 단계가 중단되었다. Host DLL은 자식 프로세스를 만들지 않으므로 `Process.Kill()`과 `WaitForExit()`를 사용하는 최소 수정으로 정리 경계를 복구했다.

구문 분석과 실제 재실행에서 ApiHost health·database health·OpenAPI·Swagger UI·Development Bypass Tenant API·WorkerHost health·database health가 모두 HTTP 200이었고, 두 Host stderr는 0바이트이며 임시 Smoke 디렉터리는 존재하지 않았다. 이후 Release Restore·Build·Test도 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 247개 통과·실패 0개·건너뜀 0개로 통과했다. 이 기록은 재발 방지용 일회성 검증 도구의 증거이며 예약 실행이나 운영 인증 우회를 추가하지 않는다.

## 2026-11-10 PR #166 DLQ 페이지 오프셋 정합성 보강

Worker 서버는 `(page - 1) * pageSize`를 checked 계산하여 정수 오버플로를 `worker.dead_letter.invalid_page`로 거부하지만 typed Client가 양의 `page`를 모두 허용하던 계약 불일치를 확인했다. Client가 `page`·`pageSize` 조합의 오프셋을 정수 범위 안에서 선행 검증하도록 최소 수정하고, 한국어 XML 문서와 운영 가이드에 오버플로 조건을 기록했다. Client 단위 테스트는 `page=int.MaxValue`를 서버에 전송하지 않고 차단한다.

PR #166의 head `c57952206fca5e161a8c392911f45cac82e1bc57`에 대한 [PR CI Run #952](https://github.com/namanvarava/Edan.Tensor/actions/runs/42736302956)와 Squash Merge 후 main 커밋 `7be3d93fbfc56ed2767ea985ad5b59ece30af2e7`의 [main CI Run #953](https://github.com/namanvarava/Edan.Tensor/actions/runs/42736549387)이 Restore·Release Build·Release Test를 성공시켰다. 병합 후 로컬에서도 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 재확인했다. 이 변경은 승인된 내부 SQLite·Swagger/OpenAPI·Platform·DataSet·Worker 범위만 보강하며, DuckDB·Parquet·Commercial·Licensing·LicenseAuthority·WPF와 실제 외부 환경 검증을 완료 처리하지 않는다.

## 2026-11-10 PR #164 Client 페이지 계약 정합성 보강

서버가 DataSet 검증 상세 `take`를 1~500, Worker DLQ `pageSize`를 1~100으로 제한하는데 typed Client가 각각 1000·200까지 허용하던 계약 불일치를 확인했다. Client의 선행 검증을 서버 범위와 일치시키고, 한국어 XML 문서와 API 운영 가이드에 입력 제약·HTTP 400 오류 코드를 기록했다. API Host 통합 테스트는 `take=501`과 `pageSize=101`을 각각 HTTP 400 및 안정적인 오류 코드로 확인하며, Client 단위 테스트는 서버에 요청하기 전에 잘못된 입력을 차단한다.

PR #164의 head `89a62cd526ab98667f4d75f4c340499b96bbdb21`에 대한 [PR CI Run #948](https://github.com/namanvarava/Edan.Tensor/actions/runs/42733699062)와 Squash Merge 후 main 커밋 `f2d303c607b21b418e98f688000262441f48d837`의 [main CI Run #949](https://github.com/namanvarava/Edan.Tensor/actions/runs/42733986905)이 Restore·Release Build·Release Test를 성공시켰다. 병합 후 로컬에서도 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 재확인했고, 작업 브랜치는 로컬·원격에서 삭제했다. 이 변경은 승인된 로컬 SQLite·Swagger/OpenAPI·Platform·DataSet·Worker 범위만 보강하며, DuckDB·Parquet·Commercial·Licensing·LicenseAuthority·WPF와 실제 외부 환경 검증을 완료 처리하지 않는다.

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
| Commercial | 공급자 중립 결제 경계 구현·검증 | `IPaymentProvider`·`PaymentProviderGateway`가 공급자 미선정 시 `commercial.payment_provider_not_selected`로 실패하고 선정 공급자에게만 요청을 전달하는 회귀 테스트를 보유 | 상품·요금·실제 결제·Webhook·Tenant 권리·취소·환불 계약과 공급자 선정 |
| Licensing·LicenseAuthority | 승인된 개발 라이선스 기능 구현·검증 완료 | 별도 솔루션의 발급·공개키·검증·폐기, 5년·ES256·개발 환경 제한, 정상·위조·만료·시작 전·Tenant·기능·폐기·KeyId·운영 차단 테스트 | 실제 운영 라이선스·외부 키 보관·배포 환경 검증은 별도 운영 계약과 환경 제공 후 수행 |
| 운영·외부 검증 | 환경 미검증 | 실제 운영 DB, 외부 수신 시스템, GitHub main 보호 규칙 API 접근이 현재 Work에 없음 | 외부 시스템·권한·비용·서비스 계정 제공 후 런북 실행 |

## 현재 내부 검증 증거

필수 Release 명령은 최신 main에서 다음 결과를 반환했습니다.

```text
dotnet restore EDAN.Tensor.sln                         성공
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
  경고 0개·오류 0개
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
  통과 262개·실패 0개·건너뜀 0개
```

추가 경계 테스트도 다음과 같이 통과했습니다.

- 인증·JWKS·키 회전·Development Bypass: 12개 통과
- Worker 강제 종료·Lease/Fencing·종료 인계·저장 실패 복구: 24개 통과
- DataSet SQLite·Tenant 격리·Quota·Fencing·백업/복구: 11개 통과
- 알려진 취약 패키지 감사: `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`에서 취약 패키지 없음
- 실제 Release Host Smoke: 임시 SQLite 네 개로 ApiHost·WorkerHost를 기동하여 health, database health, OpenAPI, Swagger UI, Development Bypass Tenant API가 모두 HTTP 200; 양쪽 stderr 0바이트; 종료 후 프로세스·포트·임시 경로 없음
- LicenseAuthority HTTP Smoke: Development 발급·단일 라이선스 재사용·5년 기간·공개 JWK·정상 검증·Production 개발 라이선스 차단·폐기 검증 차단을 확인하고 토큰 로그 없음·임시 경로 삭제
- Commercial 결제 경계: 공급자 미선정 거부와 선정 공급자 전달 2개 테스트 통과; 실제 결제 호출·가격·Webhook·환불·유료 권리는 실행하지 않음

## 인증 우회와 외부 경계

Development Bypass는 고정된 비운영 Tenant·사용자·`sub`를 사용하는 로컬 개발 설정에서만 활성화됩니다. External·운영 환경은 Issuer·Audience·Metadata·JWKS·HTTPS·서명 검증을 유지하며 Bypass가 운영 인증을 대체하지 않습니다. CI와 배포 구성에는 Bypass를 성공 조건으로 포함하지 않습니다.

실제 외부 IdP와 GitHub 보호 규칙은 Issue #84 및 Milestone #1에서 추적합니다. GitHub main 보호 규칙 API는 현재 HTTP 403 `Resource not accessible by integration`으로 조회가 거부되었으므로 환경 미검증으로 남깁니다. 토큰·개인키·비밀번호는 이 문서와 저장소에 기록하지 않습니다.

## 다음 실행 순서

1. DuckDB·Parquet의 포맷·스키마·읽기·쓰기·원자성·quota·복구 수용 기준이 승인되면 구현하고 검증합니다.
2. Commercial 상품·요금·결제 공급자·Webhook·Tenant 권리·취소·환불 계약이 승인되면 현재 공급자 중립 경계에 어댑터를 연결하고 회귀 검증합니다.
3. WPF 화면·업무 흐름·배포 대상·권한 계약이 승인되면 typed Client와 기존 API 계약을 유지하며 구현합니다.
4. 실제 외부 IdP·운영 DB·외부 수신 시스템·배포·GitHub 보호 규칙은 조건이 제공된 뒤 별도 실행합니다.

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

## 2026-11-10 WorkerHost 데이터베이스 health 경계 보강

WorkerHost의 기존 `/health`는 프로세스가 응답하는지만 확인하여, 네 개 로컬 SQLite의 연결 또는 기본 스키마 손상을 구분하지 못했다. API Host가 사용하는 데이터베이스 health 계약과 동일하게 WorkerHost에 `/health/database`를 추가하고, Platform·Worker·DataSet 메타데이터와 DataSet 콘텐츠 저장소를 각각 연결·간단한 읽기로 확인하도록 공통 SQLite probe를 HostServices로 이동했다. 하나라도 실패하면 `Disconnected/Red`, HTTP 503을 반환하며 취소 요청은 그대로 전파한다.

실제 검증은 `WorkerHostHealthIntegrationTests`의 정상·콘텐츠 DB 열기 실패 2개와 솔루션 전체 Release Test 248개에서 실패 0개·건너뜀 0개로 통과했다. `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\host-smoke.ps1`도 임시 SQLite 네 개로 ApiHost·WorkerHost의 health와 database health, OpenAPI, Swagger UI, Development Bypass Tenant API를 모두 HTTP 200으로 확인했고, 양쪽 stderr 0바이트와 임시 Smoke 디렉터리 삭제를 확인했다.

이 변경은 로컬 SQLite 운영 관찰성을 보강하지만 실제 운영 DB 장애 복구, 외부 IdP, 외부 수신 시스템, GitHub 보호 규칙 HTTP 403 검증을 완료 처리하지 않는다.

## 2026-11-10 WorkerHost 테스트 임시 경계 정리 안정화

WorkerHost health 통합 테스트의 `finally`가 임시 DB 디렉터리를 삭제할 때 `using var`로 선언한 WebApplicationFactory·HttpClient·응답이 아직 메서드 스코프에 남아 있을 수 있는 정리 순서 결함을 확인했다. Windows에서는 이 순서가 파일 핸들 잠금으로 임시 디렉터리 삭제 실패와 후속 테스트 오염을 만들 수 있다. 삭제 재시도나 지연을 추가하는 대안은 원인을 숨기므로 선택하지 않고, 테스트 블록 안에서 Host·Client·응답을 먼저 폐기하는 최소 수정으로 정리 순서를 고정했다.

수정 후 정상·콘텐츠 DB 열기 실패 WorkerHost health 통합 테스트 2개가 통과했다. 이 변경은 제품 실행 경계를 바꾸지 않으며, 임시 자원 정리 실패가 재발하지 않도록 테스트 구조 자체를 고정한 것이다.

## 2026-11-10 PR #177 병합 후 Release 재검증

PR #177은 WorkerHost health 통합 테스트의 임시 자원 정리 순서를 고정하여 `9207f318b13b17a422f1dbd404aff7658b07d7ee` 변경을 검증했고, PR CI [Run #42750885076](https://github.com/namanvarava/Edan.Tensor/actions/runs/42750885076)이 성공한 뒤 `f4c54b8f294b0845dabf62f689a3a985c4b567af`로 `main`에 Squash Merge되었다. 병합 후 main CI [Run #42751232084](https://github.com/namanvarava/Edan.Tensor/actions/runs/42751232084)도 성공했다.

병합 후 실제 호스트에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 다시 실행했다. Restore는 모든 프로젝트가 최신 상태였고, Build는 경고 0개·오류 0개, Test는 248개 통과·실패 0개·건너뜀 0개였다. `tools/host-smoke.ps1`는 ApiHost·WorkerHost health와 database health, OpenAPI, Swagger UI, Development Bypass Tenant API를 모두 HTTP 200으로 확인했고 양쪽 stderr 0바이트와 임시 디렉터리 삭제를 확인했다.

제한된 Work 실행 경계에서는 병렬 솔루션 복원·빌드가 MSBuild 프로젝트 그래프 단계에서 오류·경고 없이 종료 코드 1을 반환했고, VSTest가 testhost에 연결하지 못하는 현상이 직접 `dotnet vstest`에서도 재현되었다. 동일 커밋을 외부 실행 경계에서 위 명령과 Smoke로 성공시켜 코드·테스트 실패와 실행 환경 제한을 분리했다. 이 환경 현상은 제품 결함으로 기록하지 않으며, 다음 로컬 재현에서는 외부 실행 경계 또는 직렬 MSBuild를 사용한다.

이 증거는 승인된 내부 SQLite·Swagger/OpenAPI·Platform·DataSet·Worker 범위에 한정한다. 실제 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403, Licensing·Commercial·DuckDB/Parquet·WPF는 Issue #84 및 Milestone #1의 외부 조건 또는 별도 승인 계약이 필요하므로 완료로 판정하지 않는다.

## 2026-11-10 DataSet 검증 Idempotency 보상 경계 보강

DataSet 검증 API가 Idempotency-Key로 기존 실행을 저장소에서 재사용하는 동시 경로를 재감사했다. API가 저장 전 조회에서 실행을 찾지 못한 뒤 다른 요청이 먼저 생성한 실행을 저장소의 유일 키 충돌 처리로 돌려받을 수 있는데도, 저장 전 조회 결과만으로 새 실행이라고 판단하고 Worker 등록 실패 시 `DeleteUnqueuedExecutionAsync`를 호출할 수 있었다. 이 경로에서는 다른 요청이 만든 검증 실행을 삭제할 가능성이 있었다.

근본 원인은 저장소의 `CreateExecutionAsync` 결과가 실행 Snapshot만 반환하여 현재 요청이 실제로 레코드를 만들었는지 호출자가 알 수 없었던 점이다. 삭제 재시도나 동시성 예외 무시는 원인을 숨기므로 선택하지 않았다. 저장소에 생성 여부를 함께 반환하는 `CreateExecutionWithStatusAsync`를 추가하고 기존 메서드는 Snapshot 호환 래퍼로 유지했으며, API는 `Created=true`일 때만 내부 보상 삭제 대상으로 등록하도록 최소 수정했다.

Idempotency 유일 키 충돌에서 기존 실행을 재사용하고 `Created=false`를 반환하는 대상 회귀 테스트 1개, 솔루션 전체 Release Test 249개, Release Build 경고 0개·오류 0개와 실제 Host Smoke를 통과했다. 이 수정은 공개 HTTP 계약과 Tenant 경계를 변경하지 않으며, 동시 요청의 보상 삭제 재발을 저장소 결과 계약으로 방지한다.

## 2026-11-10 최신 main 통합 상태와 잔여 범위

PR #180 병합 후 로컬·원격에는 `main`만 남았고 열린 Pull Request는 없다. 열린 Issue #84는 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한과 GitHub main 보호 규칙 HTTP 403을 별도로 검증하는 Milestone #1의 후속 조건으로 유지한다.

현재 내부 구현·검증 완료 기준은 로컬 SQLite·Migration·Swagger/OpenAPI, Platform·DataSet·Worker API 흐름, Tenant·개발 인증 경계, Worker Lease/Fencing·장애 복구, typed Client와 관련 회귀 테스트다. DuckDB·Parquet·Commercial·WPF는 승인된 계약이 없고, Licensing·LicenseAuthority는 발급 규칙·서명 포맷·키 보관·폐기 계약이 없어 구현하지 않았다. 따라서 이 감사는 내부 범위의 병합·검증 완료를 기록하지만 전체 제품 개발 완료를 선언하지 않는다.

## 2026-11-10 승인 라이선스 계약 반영

사용자가 개발·테스트 전용 라이선스 계약을 승인하여 별도 `EDAN.Tensor.LicenseAuthority.sln`에 LicenseAuthority 발급·공개키·검증·폐기 경계를 구현했다. 대상은 `EDAN-NONPROD` Tenant, `EDAN.Tensor` 제품, `Platform`·`DataSet`·`Worker` 기능이며 발급일 기준 5년, 자동 LicenseId·KeyId, JWS ES256과 개인키 비공개 보관을 적용했다. 정상·위조·만료·시작 전·다른 Tenant·폐기·알 수 없는 KeyId 회귀 테스트는 별도 솔루션과 전체 솔루션에 포함한다.

RBAC, 기본 거부, 최소 권한, Tenant·Organization·Project 범위와 감사 경계는 기존 Platform 계약을 유지한다. 결제 공급자는 확정하지 않고 Paddle 1순위, Stripe 2순위, PayPal 대안 권고만 관리 문서에 기록했으며, 실제 결제·가격·환불·유료 entitlement는 구현하지 않았다.

제한된 Work 실행 경계에서는 LicenseAuthority와 기존 Host를 별도 프로세스로 시작할 때 `Now listening`·`Application started` 로그까지 확인했으나 loopback HTTP 요청이 `액세스 권한에 의해 숨겨진 소켓을 시도했습니다`로 차단되었다. 권한이 허용된 실행 환경에서 동일 스모크를 재실행하여 LicenseAuthority의 Development 발급·공개키·정상 검증·Production 개발 라이선스 차단·폐기 검증과 ApiHost·WorkerHost의 health·database health·OpenAPI·Swagger UI를 성공 확인했다. 제한된 Work 경계의 실패는 실행 환경 제한으로 기록하고 제품 성공으로 대체하지 않는다.
