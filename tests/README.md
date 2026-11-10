# 테스트

저장소에는 도메인·응용·인프라·API 통합 테스트 프로젝트가 포함되어 있으며, 보안·복원력·아키텍처 경계는 해당 프로젝트의 테스트에서 검증한다.

현재 확인된 테스트 구성은 다음과 같다.

- SharedKernel: 11개
- Platform Domain: 29개
- Platform Application: 30개
- Platform Infrastructure: 12개
- DataSet Domain: 6개
- DataSet Application: 14개
- DataSet Infrastructure: 11개
- Worker Domain: 21개
- Worker Infrastructure: 71개
- Client: 7개
- API 통합: 34개

Development Bypass는 Development 환경에서만 고정된 비운영 IdP 시뮬레이션 프로필을 등록합니다. 프로필은 RFC 예약 도메인, `EDAN-NONPROD` 테스트 Tenant, `edan-nonprod-test-user`, 안정적인 `edan-nonprod-test-subject`를 사용하고 실제 외부 IdP에 연결하지 않으며, 첫 Bypass 인증 요청에 시뮬레이션 사용 경고를 기록합니다. Tenant와 안정적인 `sub`는 설정값으로 재정의할 수 없습니다. 운영·External 인증과 실제 비운영 IdP 검증의 완료 근거로 사용하지 않습니다.

## 현재 main 기준: 2026-11-10

현재 `main`은 PR #168이 `bd4753f0c4ba41d906d6ca8556257eb3765db04d`로 Squash Merge된 상태다. PR 자체 CI [Run #956](https://github.com/namanvarava/Edan.Tensor/actions/runs/42738402094)와 병합 후 main CI [Run #957](https://github.com/namanvarava/Edan.Tensor/actions/runs/42738642780)이 성공했다. 병합 후 로컬 Release 검증은 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 기록했다. OpenAPI 핵심 경로·Tenant 헤더·Idempotency-Key·Replay-Request-Id, 외부 전달 취소 경계, 저장량 이력·quota 이벤트, DataSet 검증 상세·Worker DLQ 페이지·Client 선행 검증과 API 오버플로 응답 경계를 포함한다. 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 이 내부 증거에 포함하지 않는다.

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

DataSet 검증 통합 테스트는 API가 예약한 `dataset.validation.v1` 작업을 실제 `WorkerTaskExecutionCoordinator`와 `DataSetValidationTaskHandler`에 전달하고, Lease/Fencing 실행 후 Worker 작업 성공과 Invalid 검증 결과·처리 행 수를 확인한다. API Host 통합 프로젝트는 이 경계를 포함하여 34개 테스트가 통과한다.
