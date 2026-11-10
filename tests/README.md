# 테스트

저장소에는 도메인·응용·인프라·API 통합 테스트 프로젝트가 포함되어 있으며, 보안·복원력·아키텍처 경계는 해당 프로젝트의 테스트에서 검증한다.

현재 확인된 테스트 구성은 다음과 같다.

- SharedKernel: 11개
- Platform Domain: 29개
- Platform Application: 30개
- Platform Infrastructure: 12개
- DataSet Domain: 6개
- DataSet Application: 14개
- DataSet Infrastructure: 12개
- Worker Domain: 21개
- Worker Infrastructure: 72개
- Client: 7개
- API 통합: 44개
- Licensing: 12개
- Commercial: 2개

현재 통합 솔루션의 Release 테스트는 총 272개이며 실패 0개·건너뜀 0개입니다.

Development Bypass는 Development 환경에서만 고정된 비운영 IdP 시뮬레이션 프로필을 등록합니다. 프로필은 RFC 예약 도메인, `EDAN-NONPROD` 테스트 Tenant, `edan-nonprod-test-user`, 안정적인 `edan-nonprod-test-subject`를 사용하고 실제 외부 IdP에 연결하지 않으며, 첫 Bypass 인증 요청에 시뮬레이션 사용 경고를 기록합니다. Tenant와 안정적인 `sub`는 설정값으로 재정의할 수 없습니다. 운영·External 인증과 실제 비운영 IdP 검증의 완료 근거로 사용하지 않습니다.

Release Host 경계는 저장소 루트의 `powershell -ExecutionPolicy Bypass -File tools/host-smoke.ps1`로 일회성 재현할 수 있습니다. 이 도구는 Development Bypass와 임시 SQLite만 사용하고, 운영 인증·CI·예약 실행을 변경하지 않습니다.

## 현재 검증 기준: API 요청 관측성

ApiHost는 모든 응답에 서버가 생성한 `X-Request-Id`를 추가하고 HTTP 메서드·경로·상태 코드·처리 시간을 구조화 로그로 기록합니다. 요청 본문·Bearer 토큰·개인키는 로그에 기록하지 않으며, 기존 오류 응답의 `Code`·`Description` 계약은 유지합니다. API 통합 회귀 테스트는 `/health` 성공 응답과 잘못된 JSON 오류 응답에서 서로 다른 요청 식별자가 반환되는지 확인합니다.

typed Client는 오류 응답의 `X-Request-Id`를 `TensorApiException.RequestId`로 보존하여 호출자가 서버 로그와 장애 문의를 연결할 수 있게 합니다. 서버가 헤더를 반환하지 않는 외부 호환 응답에서는 이 값이 null이며 기존 상태 코드·오류 코드·설명은 그대로 유지합니다.

관측성 변경 대상 테스트 1개와 전체 ApiHost 통합 테스트 44개가 통과했으며, 변경 후 전체 Release Test는 272개 통과·실패 0개·건너뜀 0개였습니다.

## 현재 검증 기준: 개발 라이선스 동시 발급 경쟁

개발 라이선스 발급에서 조회와 저장 사이의 경쟁으로 두 번째 동시 요청이 저장 충돌을 오류로 반환할 수 있던 결함을 확인했다. `DevelopmentLicenseService`는 저장 충돌 뒤 실제 파일 저장소에 기록된 승인 라이선스를 다시 읽어 재사용하도록 수정했고, 서로 다른 `FileDevelopmentLicenseStore` 인스턴스가 같은 파일에 동시에 저장하는 회귀 테스트를 추가했다. 대상 테스트 1개와 LicenseAuthority 전체 12개가 통과했으며, 수정 후 전체 Release Test는 271개 통과·실패 0개·건너뜀 0개였다.

## 과거 main 기준 기록: PR #210 병합 후

위 단락은 PR #210 병합 직후의 Release 검증 증거를 보존한 역사 기록입니다. 현재 기준은 아래 최신 main 기록을 사용합니다.

## 과거 제품 코드 검증 기준: PR #212 병합 후

제품 코드 Release 검증 기준은 PR #212 병합 커밋 `8c0c6ac5329935c5d4d4930ec2802d1db2070908`이다. PR #212 [CI Run #42859604333](https://github.com/namanvarava/Edan.Tensor/actions/runs/42859604333)와 병합 후 main [CI Run #42860037294](https://github.com/namanvarava/Edan.Tensor/actions/runs/42860037294)이 Restore·Build·Test를 성공했다. 당시 main에서 직렬 Restore는 경고·오류 0개, Release Build는 경고 0개·오류 0개, 전체 Test는 258개 통과·실패 0개·건너뜀 0개였다. 별도 LicenseAuthority 솔루션은 Restore·Release Build(경고·오류 0개)·Test 11개를 성공했다. LicenseAuthority Smoke는 health·공개 JWK·정상 검증을 HTTP 200, Production 차단·폐기 검증을 HTTP 403으로 확인했고, Host Smoke는 두 Host의 health·DB health·OpenAPI·Swagger·Development Bypass Tenant API를 HTTP 200으로 확인했다. 두 Smoke의 stderr는 0바이트이고 임시 경로는 삭제되었다. 병렬 Restore의 종료 코드 1은 Work 실행 경계의 Named Pipe 문제이며 직렬 Restore 성공으로 코드 실패와 구분했다. 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙은 별도 미검증 조건이다.

## PR #210 당시 상세 증거

PR #210 당시 Release 검증 기준은 PR #210 병합 커밋 `81281a7e014d6bd377dc911cd39b5eac3aa8e3ae`이다. PR #210 [CI Run #42853202736](https://github.com/namanvarava/Edan.Tensor/actions/runs/42853202736)와 병합 후 main [CI Run #42853575636](https://github.com/namanvarava/Edan.Tensor/actions/runs/42853575636)이 Restore·Build·Test를 성공했다. 이번 재검증에서 `dotnet restore EDAN.Tensor.sln`은 병렬 MSBuild IPC 권한으로 종료 코드 1을 반환했지만 `-m:1` 직렬 복원은 경고·오류 0개로 성공했다. 권한이 허용된 동일 실행 경계에서 지정된 Release Build는 경고 0개·오류 0개, 전체 Test는 258개 통과·실패 0개·건너뜀 0개였다. API 통합 테스트는 43개가 통과했다. `tools/license-authority-smoke.ps1`는 5년 개발 라이선스·공개 JWK·정상 200·Production 차단 403·폐기 차단 403과 토큰 미기록·임시 경로 삭제를 확인했고, `tools/host-smoke.ps1`는 ApiHost·WorkerHost의 health·DB health·OpenAPI·Swagger UI·Development Bypass Tenant API를 모두 HTTP 200으로 확인하고 두 Host stderr 0바이트·임시 경로 삭제를 확인했다. 기존 API 계약 회귀는 `Idempotency-Key`의 공백·200자 상한을 서버에서도 적용하고, 잘못된 JSON·잘못된 요청을 공통 `ApiError`로 변환하며, OpenAPI에 길이·UUID 제약을 표시한다. Worker Infrastructure 테스트는 72개가 통과했다. OpenAPI 핵심 경로·Tenant 헤더·Idempotency-Key·Replay-Request-Id, 등록되지 않은 Worker 작업 유형의 HTTP 422 오류 계약, 외부 전달 취소 경계, 저장량 이력·quota 이벤트, DataSet 검증 상세·Worker DLQ 페이지·Client 선행 검증과 API 오버플로 응답, WorkerHost 네 저장소 database health 경계, LicenseAuthority HTTP·서비스 경계 및 Commercial 결제 경계를 포함한다. 취약 패키지 감사는 모든 프로젝트에서 알려진 취약 패키지 없음으로 끝났다. 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 이 내부 증거에 포함하지 않는다.

## 현재 작업의 LicenseAuthority 입력 경계 재검증

서명이 유효해도 `status`·시간·Claim 형식이 손상된 라이선스 토큰이 `ArgumentException`으로 API 밖으로 누출될 수 있던 결함을 확인했다. 원인은 ES256 토큰 해석기의 예외 필터가 JSON·형식·속성 오류만 처리하고 `Enum.Parse`와 Unix 시간 변환에서 발생하는 인자 오류를 안정적인 라이선스 입력 오류로 변환하지 않은 것이었다. 예외를 `licensing.token.invalid`로 닫는 최소 수정과 서비스·HTTP 회귀 테스트를 추가했다.

PR #202 병합 후 main 기준으로 별도 LicenseAuthority Restore·Release Build는 경고 0개·오류 0개, 별도 테스트는 11개 통과·실패 0개·건너뜀 0개, 전체 Release Test는 263개 통과·실패 0개·건너뜀 0개였다. `tools/license-authority-smoke.ps1`는 발급·공개 JWK·정상 검증 200·운영 개발 라이선스 차단 403·폐기 차단 403을 확인했고, `tools/host-smoke.ps1`는 ApiHost·WorkerHost의 health·DB health·OpenAPI·Swagger UI·Development Bypass Tenant API를 모두 200으로 확인했다. 두 Smoke의 표준 오류 출력은 0바이트이고 임시 디렉터리는 삭제되었다. PR #202의 head·PR CI·병합 후 main CI와 브랜치 삭제 결과는 위 최신 main 기록과 일치한다.

## 과거 단일 실행 기록

이번 실행의 검증 대상 `main`은 PR #132 병합 커밋 SHA `8a82bf9f2a6546c593e44928d588f12d2fa2126c`이다. PR #132([최신 main CI 실패 상태 기록](https://github.com/namanvarava/Edan.Tensor/pull/132))의 head `451a3db88a93bfbcfbdf72ccb067e55813829ccd`는 squash merge되었다. PR #131 자체 CI [Run #881](https://github.com/namanvarava/Edan.Tensor/actions/runs/42672505068), PR #132 자체 CI [Run #884](https://github.com/namanvarava/Edan.Tensor/actions/runs/42675745493)와 병합 후 main CI [Run #885](https://github.com/namanvarava/Edan.Tensor/actions/runs/42676094148)는 Restore·Build·Test를 성공시켰다. 병합 직후 main CI [Run #882](https://github.com/namanvarava/Edan.Tensor/actions/runs/42672786178)와 PR #132 이전 head의 [Run #883](https://github.com/namanvarava/Edan.Tensor/actions/runs/42673460577)는 Job 생성 전 GitHub Actions `Internal server error`로 실패했지만, Run #884·#885에서 최신 문서 변경을 성공으로 재검증했다. 전체 테스트의 마지막 성공 증거는 main CI Run #885이며, PR #128 변경 브랜치에서 전체 232개·API 통합 32개가 통과했고 실패 0개·건너뜀 0개였다. 현재 main의 최신 SHA는 병합 후 GitHub 기록을 기준으로 확인하며, 외부 IdP·운영 DB·GitHub 보호 규칙 HTTP 403은 별도 환경 미검증으로 유지한다.

## 과거 단계별 검증 기록

PR #135 병합 후 최신 `main`은 `ce460ec7f3c4efcc54b4da5297879149b2f52b97`이다. PR 자체 CI Run #890과 병합 후 main CI [Run #891](https://github.com/namanvarava/Edan.Tensor/actions/runs/42681139618)이 Restore·Release Build·Release Test를 성공했다. Work에서 다음 구현 순서는 API 계약·멱등성 경쟁 검증 → Tenant·인증 경계 → Worker 장애 복구 → 승인된 비라이선스 기능 → Licensing·LicenseAuthority → 라이선스 적용 검증 → 운영·외부 환경 검증이다. 실제 외부 IdP, 운영 DB, 외부 수신 시스템, GitHub main 보호 규칙 HTTP 403은 이 테스트 결과에 포함하지 않는다.

PR #135는 동일 Tenant·Idempotency Key의 동시 외부 전달 생성에서 DB 유일성 충돌과 API 보상 삭제가 서로의 레코드에 영향을 줄 수 있던 경쟁 조건을 수정했다. `CreateOrGet_ReturnsExistingDeliveryWithoutCreatingDuplicate` 회귀 테스트와 main CI Run #891로 재검증했으며, 승인 계약이 없는 Commercial·DuckDB·Parquet·WPF는 임의 구현하지 않는다.

PR #137·#138 병합 후 최신 `main`은 `6a5ff9e7227ace12e074a9d13759ac2403f59aa6`이다. PR #137 자체 CI Run #894와 병합 후 main CI Run #895, PR #138 자체 CI Run #896과 병합 후 main CI Run #897은 Restore·Release Build·Release Test를 모두 성공했다. PR #137은 API 사용 가이드와 Development Bypass 고정 프로필의 경계를 정정했고, PR #138은 외부 IdP 런북에서 폐기된 `Authentication__BypassSubject` 설정명을 제거했다. 현재 원격·로컬 작업 브랜치는 `main`만 남아 있고 열린 PR은 없다. 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub main 보호 규칙 HTTP 403은 이 내부 CI 결과에 포함하지 않으며 Issue #84에서 별도 추적한다.

PR #139 병합 후 최신 `main`은 `a0c990aca0ee3466701e4ccee21bc1c61815e956`이다. PR 자체 CI Run #898과 병합 후 main CI [Run #899](https://github.com/namanvarava/Edan.Tensor/actions/runs/42686600210)이 Restore·Release Build·Release Test를 모두 성공했다. 최신 main의 권한 확장 로컬 실행도 Restore 성공, Release Build 경고 0개·오류 0개, 전체 Release Test 233개 통과·실패 0개·건너뜀 0개를 기록했다. 제한된 Work 권한에서 최초 Restore가 Win32 오류 5로 중단된 원인은 Service Control Manager 접근 거부였으며, 코드 실패가 아님을 확인한 뒤 권한 확장 환경에서 재검증했다.

최신 main의 직접 Host Smoke에서 임시 SQLite 4개로 ApiHost와 WorkerHost를 기동했고, ApiHost health·database health·OpenAPI·Swagger·Development Bypass Tenant API와 WorkerHost health가 모두 HTTP 200을 반환했다. 종료 후 프로세스·포트·임시 데이터가 남지 않았다. `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`에서도 모든 프로젝트에 알려진 취약 패키지가 없었다.

GitHub API의 `main` 보호 규칙 조회는 HTTP 403 `Resource not accessible by integration`으로 거부되었다. 보호 규칙의 실제 설정과 필수 Check은 별도 권한이 제공될 때만 검증하며, 이 결과를 제품 CI 성공으로 대체하지 않는다.

PR #81 병합 기준 main 문서는 SHA `030d4c91db86b13d2eb198ace1f699989a59318a`였다. 병합 후 [Actions Run #783](https://github.com/namanvarava/Edan.Tensor/actions/runs/42539919996)은 Restore·Release Build·Release Test·정리 단계를 모두 성공시켰고, 전체 테스트는 219개 통과·실패 0개·건너뜀 0개였다. 실제 비운영 외부 IdP 검증은 아직 수행하지 않았으며, 수동 절차는 [외부 IdP 경계 수동 검증 런북](../docs/validation/external-idp-boundary-test-runbook.md)에 기록되어 있다.

이번 Bypass 외부 설정 무시와 로컬 Tenant 초기화 회귀를 포함한 검증 기준에서 자체 호스팅 Windows Release 전체 테스트는 총 218개가 실패 0개·건너뜀 0개로 통과했다. 이 실행에는 개발 Bypass 선택, 외부 IdP 설정 없는 토큰 없는 요청의 인증 성공, 잘못된 외부 IdP 설정을 Bypass에서 무시하는 동작, 고정 Claim 발급, 비개발 환경 Bypass 차단, 로컬 Tenant·관리자 소속의 중복 방지 및 외부 인증 경계 회귀 테스트가 포함된다. PR #63 검증은 [Actions Run #744](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399072656)에서 확인했고, 병합 후 최신 `main`은 [Actions Run #746](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399622276)의 `EDAN-TENSOR-WIN` Runner에서 Restore·Build·Test를 다시 성공시켰다.

2026-11-07 DataSet 검증 Fencing Token 코드 기준 커밋 `2e2a49efdac38417037b7072c1220af17eeb8afb`에서 총 211개 테스트가 실패 0개·건너뜀 0개로 통과했다. 같은 결과는 [Actions Run #692](https://github.com/namanvarava/Edan.Tensor/actions/runs/42326121824)에서 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`으로 확인할 수 있다. 이 기록은 해당 시점의 인수인계 증거이며 최신 테스트 수를 대체하지 않는다. 저장소 루트에서 다음 명령으로 최신 Release 검증을 재현할 수 있다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

원격 CI는 Windows 자체 호스팅 Runner에서 동일한 명령을 실행하며, 로컬 실행 결과와 원격 CI 결과를 구분해 검증 문서에 기록한다.

## 2026-11-10 loopback 설정 경계 회귀

`ExternalDelivery:AllowLoopback=true`는 Development 환경에서만 허용하도록 두 Host의 옵션 생성 경계를 고정했다. 비개발 환경에서는 잘못된 운영 설정을 호스트 시작 단계에서 거부하고, Development에서는 격리된 로컬 수신기 테스트를 계속 허용한다.

대상 외부 전달 테스트 7개와 솔루션 전체 Release 테스트 235개가 실패 0개·건너뜀 0개로 통과했다. PR Check와 병합 후 main CI 결과가 추가되기 전까지 이 결과는 작업 브랜치 검증으로만 판정한다.

## 2026-11-10 main 최종 검증

PR #143의 [PR CI Run #906](https://github.com/namanvarava/Edan.Tensor/actions/runs/42693768814)과 [main CI Run #907](https://github.com/namanvarava/Edan.Tensor/actions/runs/42694097462)이 성공했다. 병합된 main에서 Restore 성공, Release Build 경고 0개·오류 0개, 전체 Release Test 235개 통과·실패 0개·건너뜀 0개를 다시 확인했다. 실제 Development Host Smoke는 ApiHost와 WorkerHost의 health·DB health·OpenAPI·Swagger·보호 Tenant API를 모두 HTTP 200으로 확인하고 자원을 정리했다.

## Client API 연동 회귀

`EDAN.Tensor.Client.Tests`는 인증 헤더, 기본 Tenant 헤더, `Idempotency-Key`, 구조화 API 오류, 404 선택 조회, 다운로드 스트림 수명, 검증 결과 삭제, 입력·페이지 상한을 검증한다. 실제 임시 SQLite API Host에 typed Client를 연결하는 통합 테스트 1개도 추가했으며, 새 Client 테스트 7개를 포함한 솔루션 전체 Release Test는 243개 통과·실패 0개·건너뜀 0개다. PR #153의 [PR CI Run #926](https://github.com/namanvarava/Edan.Tensor/actions/runs/42715584623)과 병합 후 [main CI Run #927](https://github.com/namanvarava/Edan.Tensor/actions/runs/42715896376)도 Restore·Release Build·Release Test를 성공했다.

DataSet 검증 통합 테스트는 API가 예약한 `dataset.validation.v1` 작업을 실제 `WorkerTaskExecutionCoordinator`와 `DataSetValidationTaskHandler`에 전달하고, Lease/Fencing 실행 후 Worker 작업 성공과 Invalid 검증 결과·처리 행 수를 확인한다. API Host 통합 프로젝트는 이 경계를 포함하여 35개 테스트가 통과한다.

## 2026-11-10 WorkerHost 데이터베이스 health 경계

`WorkerHostHealthIntegrationTests`는 임시 SQLite 네 개를 실제 WorkerHost에 연결하여 정상 상태의 `/health/database`가 `Connected/Green`으로 보고하는지, 콘텐츠 데이터베이스를 열 수 없을 때 HTTP 503과 `Disconnected/Red`를 반환하는지 확인한다. Release 전체 테스트는 이 회귀를 포함하여 248개 통과·실패 0개·건너뜀 0개이며, `tools/host-smoke.ps1`는 정상 경계를 실제 Host 기동으로 재확인한다. 이 검증은 Development Bypass와 로컬 SQLite만 사용하므로 실제 운영 DB·외부 IdP·외부 시스템 검증을 대체하지 않는다.

## 2026-11-10 PR #177 병합 후 최신 검증

PR #177의 [PR CI Run #42750885076](https://github.com/namanvarava/Edan.Tensor/actions/runs/42750885076)과 병합 후 [main CI Run #42751232084](https://github.com/namanvarava/Edan.Tensor/actions/runs/42751232084)이 성공했다. 병합된 main SHA는 `f4c54b8f294b0845dabf62f689a3a985c4b567af`이며, 실제 호스트에서 지정된 Release Restore·Build·Test를 성공시켜 Build 경고 0개·오류 0개, 전체 Test 248개 통과·실패 0개·건너뜀 0개를 확인했다. Host Smoke도 ApiHost·WorkerHost의 필수 HTTP 경계를 모두 200으로 확인하고 stderr 0바이트와 임시 데이터 삭제를 확인했다.

제한된 Work 실행 경계에서 발생한 병렬 MSBuild 종료 코드 1과 VSTest testhost 연결 실패는 외부 실행 경계의 동일 커밋 성공으로 환경 문제임을 확인했다. 이 내부 검증은 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403·Licensing을 포함하지 않는다.
