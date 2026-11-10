# 2026-11-11 연속 개발 재검증 기록

이 문서는 현재 Work에서 EDAN.Tensor 개발을 재개한 뒤 `main`의 승인된 저장소 내부 범위를 다시 점검한 기록이다. 초기 검증 기준은 main 커밋 `2f66ab2bf1b4e35f0b7c5341b5feb4dbfe1740e9`였고, 이후 개발 라이선스 동시 발급 경쟁 수정이 PR #215로 병합되었다. 아래에서 초기 검증과 최신 main 검증을 구분한다.

## 현재 상태

- 검증 시작 시 로컬과 원격 `main`은 위 커밋을 가리켰고 작업 트리는 깨끗했다.
- 검증 시작 시 열린 Pull Request는 없었고 이전 작업 브랜치는 로컬·원격에서 삭제되었다. 이 기록을 위한 문서 브랜치는 병합 후 삭제한다.
- 승인된 LicenseAuthority, RBAC, SQLite·Swagger/OpenAPI, Platform·DataSet·Worker, Client 계약은 기존 main에 포함되어 있다.
- 제품 ApiHost에 라이선스를 자동 적용하는 헤더·기능 매핑·fail-closed 계약은 별도 승인 전 상태이므로 추가하지 않았다.

## 이번 Work 실행 결과

제한된 Work 실행 경계에서 필수 명령을 다시 실행한 결과는 다음과 같다.

| 명령 | 결과 | 판정 근거 |
| --- | --- | --- |
| `dotnet restore EDAN.Tensor.sln` | 종료 코드 1 | MSBuild 병렬 Restore가 경고·오류 출력 없이 Work의 IPC 경계에서 중단되었다. |
| `dotnet restore EDAN.Tensor.sln -m:1 --verbosity minimal` | 성공 | 모든 프로젝트가 최신 상태이며 종료 코드 0이다. |
| `dotnet build EDAN.Tensor.sln --no-restore --configuration Release` | 종료 코드 1 | 경고 0개·오류 0개 상태에서 병렬 실행 경계가 중단되었다. |
| `dotnet build EDAN.Tensor.sln --no-restore --configuration Release -m:1` | 성공 | 전체 프로젝트가 빌드되었고 경고 0개·오류 0개이다. |
| `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release` | 종료 코드 1 | 제한된 병렬 testhost 실행이 출력 없이 종료되었다. |
| `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release -m:1` | 환경 중단 | 첫 테스트 프로젝트에서 VSTest가 90초 동안 testhost에 연결하지 못했고 다음 프로젝트에서도 같은 현상이 반복되어 실행을 중단했다. |

위 두 Build 실패와 Testhost 연결 실패는 소스 컴파일·테스트 assertion 실패가 아니라 Work 실행 경계 문제다. 동일 원인을 무한 재시도하지 않고 직렬 Build와 권한이 허용된 자체 호스팅 CI를 근거로 분리했다.

## PR #213 당시 권한이 허용된 회귀 증거

- PR #213 CI [Run #1048](https://github.com/namanvarava/Edan.Tensor/actions/runs/42862332589)와 병합 후 main CI [Run #1049](https://github.com/namanvarava/Edan.Tensor/actions/runs/42862682305)가 성공했다.
- PR #213 당시 제품 Release 회귀는 258개 통과·실패 0개·건너뜀 0개이며, 별도 LicenseAuthority 회귀는 11개 통과·실패 0개·건너뜀 0개였다.
- LicenseAuthority Smoke는 단일 `EDAN-NONPROD` 5년 개발 라이선스, 공개 JWK, 정상 검증 HTTP 200, Production·폐기 검증 HTTP 403, 토큰 미기록과 임시 경로 삭제를 확인했다.
- Host Smoke는 ApiHost·WorkerHost의 health, database health, OpenAPI, Swagger UI와 Development Bypass Tenant API를 HTTP 200으로 확인했으며 표준 오류 출력과 임시 경계를 정리했다.
- `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive --no-restore --format json`은 NuGet 소스를 조회한 뒤 취약 항목 없이 모든 프로젝트를 반환했다.
- 저장소 Markdown 상대 링크 점검 결과 누락 링크는 0개였다.

## 2026-11-11 최신 main 동시 발급 경쟁 수정 검증

- PR [#215](https://github.com/namanvarava/Edan.Tensor/pull/215)는 개발 라이선스 저장 경쟁에서 두 요청이 같은 저장소를 동시에 읽을 때 한 요청이 `IOException`으로 실패하는 원인을 수정했다. 저장 충돌이 발생하면 저장소를 다시 읽어 이미 저장된 라이선스를 검증하고 재사용한다.
- PR #215의 커밋 `6b25625efad522a36d6127854971461b77b717cf`는 squash merge 커밋 `12aaf46924695768449a9886f5a601738963fb9c`로 main에 병합되었다. PR CI [Run #1052](https://github.com/namanvarava/Edan.Tensor/actions/runs/42869361424)와 병합 후 main CI [Run #1053](https://github.com/namanvarava/Edan.Tensor/actions/runs/42869727011)는 Restore·Build·Test를 모두 성공했다.
- 실제 파일 기반 `FileDevelopmentLicenseStore` 두 인스턴스를 사용한 동시 발급 경쟁 회귀 테스트가 1개 통과했다. 별도 LicenseAuthority 테스트는 12개 통과했고, 전체 Release 테스트는 271개 통과·실패 0개·건너뜀 0개였다.
- 다음 Release 명령을 권한이 허용된 환경에서 다시 실행해 모두 종료 코드 0을 확인했다: `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`. Build 경고와 오류는 0개였다.
- 최신 main에서 LicenseAuthority Smoke는 단일 `EDAN-NONPROD` 5년 개발 라이선스, 공개 JWK, 정상 검증 HTTP 200, Production·폐기 검증 HTTP 403, 토큰 미기록과 임시 경로 삭제를 확인했다. Host Smoke는 ApiHost·WorkerHost의 health, database health, OpenAPI, Swagger UI와 Development Bypass Tenant API를 HTTP 200으로 확인했다.
- 검증 종료 시 로컬 `main`과 `origin/main`은 `12aaf46924695768449a9886f5a601738963fb9c`로 일치했고, PR #215 작업 브랜치는 로컬·원격에서 삭제했다.

## 2026-11-11 최신 main 문서 병합 후 재확인

- 문서 검증 보강 PR [#216](https://github.com/namanvarava/Edan.Tensor/pull/216)은 `e480102a6d033f5d6fb96186568086bebc665201`로 squash merge되었다. PR CI [Run #1054](https://github.com/namanvarava/Edan.Tensor/actions/runs/42870682956)와 병합 후 main CI [Run #1055](https://github.com/namanvarava/Edan.Tensor/actions/runs/42871076194)는 Restore·Build·Test를 모두 성공했다.
- 최신 main에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. Restore와 Test는 종료 코드 0이었고, Build는 경고 0개·오류 0개였다. Test 프로젝트별 결과는 총 271개 통과·실패 0개·건너뜀 0개이며 ApiHost 통합 테스트 43개도 포함한다.
- 최신 main의 Host Smoke는 ApiHost·WorkerHost health와 database health, OpenAPI, Swagger UI, Development Bypass Tenant API를 모두 HTTP 200으로 확인했고 양쪽 표준 오류 출력은 0바이트이며 임시 Smoke 디렉터리는 삭제되었다.
- 최신 main의 LicenseAuthority Smoke는 Development 라이선스 단일 발급과 5년 기간, 공개 JWK, 정상 검증 HTTP 200, Production 개발 라이선스 검증 HTTP 403, 폐기 검증 HTTP 403을 확인했으며 stdout에 토큰을 기록하지 않고 임시 디렉터리를 삭제했다.
- `rg -n "TODO|NotImplementedException" src tests tools`는 일치 항목을 반환하지 않았다. 로컬 `main`과 `origin/main`은 위 병합 SHA로 일치했고, 작업 트리·원격 작업 브랜치·열린 Pull Request를 최종 확인한 뒤 문서 브랜치를 삭제한다.

## 2026-11-11 API 요청 관측성 보강과 최신 main 재검증

- 성공 요청에는 호출자가 서버 로그를 찾을 식별자가 없고 오류 로그에만 `TraceIdentifier`가 남아 있던 것이 관측성 결함의 원인이었다. 요청 본문·Bearer 토큰·개인키를 추가 기록하는 방법은 보안 경계를 훼손하므로 선택하지 않고, 서버 생성 `X-Request-Id` 응답 헤더와 메서드·경로·상태 코드·처리 시간·요청 식별자의 구조화 로그를 추가했다. 기존 오류 본문과 인증·Tenant·Worker 공개 계약은 변경하지 않았다.
- PR [#219](https://github.com/namanvarava/Edan.Tensor/pull/219)의 소스 커밋 `1248e2e4219df62f5cf1edf475250004381a811e`는 squash merge 커밋 `a85be5acfb945e4d235df7ad1d301f82f80cbe60`으로 main에 병합되었다. PR CI [Run #1060](https://github.com/namanvarava/Edan.Tensor/actions/runs/42880400782)와 병합 후 main push CI [Run #1061](https://github.com/namanvarava/Edan.Tensor/actions/runs/42880714052)는 Restore·Build·Test를 모두 성공했다.
- 승인된 권한 환경에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실제 실행했다. Restore는 최신 상태, Build는 경고 0개·오류 0개, Test는 272개 통과·실패 0개·건너뜀 0개였으며 ApiHost 통합 테스트 44개를 포함한다. 변경 대상 요청 식별자 테스트 1개도 통과했다.
- Host Smoke는 ApiHost와 WorkerHost의 health·database health, OpenAPI, Swagger UI, Development Bypass Tenant API를 HTTP 200으로 확인했고 양쪽 표준 오류 출력은 0바이트이며 임시 Smoke 디렉터리는 삭제되었다. LicenseAuthority Smoke는 단일 `EDAN-NONPROD` 5년 개발 라이선스, 공개 JWK, 정상 검증 HTTP 200, Production 개발 라이선스·폐기 라이선스 검증 HTTP 403, 토큰 미기록과 임시 디렉터리 삭제를 확인했다.
- PR 병합 후 로컬 `main`과 `origin/main`은 `a85be5acfb945e4d235df7ad1d301f82f80cbe60`으로 일치했다. `codex/add-api-request-observability-20261111` 원격 브랜치는 삭제되었고 로컬 브랜치도 삭제되어 현재 로컬에는 `main`만 남았다.

## 2026-11-11 typed Client 요청 식별자 보존 최신 검증

- 서버가 반환하는 `X-Request-Id`를 typed Client의 `TensorApiException.RequestId`로 보존하지 않던 결함을 수정했다. 원인은 서버 응답 헤더가 API Host에서 추가되어도 Client가 상태 코드·오류 코드·설명만 읽고 요청 식별자를 폐기한 데 있었다. nullable 속성을 추가하는 최소 수정으로 기존 오류 본문, 인증, Tenant, Worker 공개 계약을 유지했으며 헤더가 없는 외부 호환 응답은 null로 처리한다.
- PR [#221](https://github.com/namanvarava/Edan.Tensor/pull/221)의 소스 커밋 `fee735468c8b13ddd5999ae7c302e6808f27dbd1`은 squash merge 커밋 `73bd8aac5da1d8277ac2c24b8ae46aa96e3beeed`로 main에 병합되었다. PR CI [Run #1064](https://github.com/namanvarava/Edan.Tensor/actions/runs/42883627568)와 병합 후 main push CI [Run #1065](https://github.com/namanvarava/Edan.Tensor/actions/runs/42883971328)는 Restore·Build·Test를 모두 성공했다.
- 승인된 권한 환경에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 병합 후 최신 main에서 다시 실행했다. Restore는 최신 상태, Build는 경고 0개·오류 0개, Test는 272개 통과·실패 0개·건너뜀 0개였으며 Client 테스트 7개와 ApiHost 통합 테스트 44개를 포함한다. 요청 식별자를 예외에 보존하는 Client 회귀 테스트도 통과했다.
- Client 변경 후 Host Smoke와 LicenseAuthority Smoke를 다시 실행했다. ApiHost·WorkerHost health, database health, OpenAPI, Swagger UI, Development Bypass Tenant API는 HTTP 200이며 양쪽 표준 오류 출력은 0바이트였다. LicenseAuthority는 단일 `EDAN-NONPROD` 5년 개발 라이선스, 공개 JWK, 정상 검증 HTTP 200, Production 개발 라이선스·폐기 라이선스 검증 HTTP 403, 토큰 미기록과 임시 디렉터리 삭제를 확인했다.
- PR 병합 후 로컬 `main`과 `origin/main`은 `73bd8aac5da1d8277ac2c24b8ae46aa96e3beeed`로 일치했다. `codex/preserve-client-request-id-20261111` 원격·로컬 브랜치는 삭제되었고 현재 로컬에는 `main`만 남았다.

## 2026-11-11 OpenAPI 요청 식별자 응답 계약 최신 검증

- 실행 응답에 이미 추가되는 `X-Request-Id`가 OpenAPI 응답 문서에는 표시되지 않던 원인은 변환기가 선택적 `Headers` 사전을 초기화하지 않은 데 있었다. 사전을 안전하게 초기화하고 모든 응답에 헤더를 기록하도록 수정했으며, 기존 인증·Tenant·Worker·오류 본문 계약은 변경하지 않았다.
- `/health` 200 응답의 OpenAPI `headers.X-Request-Id`와 전체 OpenAPI 문서 문자열을 회귀 테스트로 고정했다. 대상 테스트 `OpenApiDocumentContainsPlatformProvisioningRoutes` 1개가 통과했고, 변경 후 전체 Release 테스트는 272개 통과·실패 0개·건너뜀 0개였다.
- 병합 전 실제 명령은 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`이며 Restore는 성공, Build는 경고 0개·오류 0개, Test는 272개 통과·실패 0개·건너뜀 0개였다. Host Smoke는 ApiHost·WorkerHost health와 database health, OpenAPI, Swagger UI, Development Bypass Tenant API를 HTTP 200으로 확인했고 양쪽 표준 오류 출력은 0바이트이며 임시 디렉터리는 삭제되었다. LicenseAuthority Smoke는 단일 `EDAN-NONPROD` 5년 개발 라이선스, 공개 JWK, 정상 검증 HTTP 200, Production 개발 라이선스·폐기 검증 HTTP 403, 토큰 미기록과 임시 디렉터리 삭제를 확인했다.
- PR [#223](https://github.com/namanvarava/Edan.Tensor/pull/223)의 소스 커밋 `7fb37964d9de87ad10d909045a55dfc3cc7b7d1a`는 squash merge 커밋 `b28aa38914351f7a1e3c3630a36e4ce93bcc380f`로 main에 병합되었다. PR CI [Run #1068](https://github.com/namanvarava/Edan.Tensor/actions/runs/42912131321)와 병합 후 main push CI [Run #1069](https://github.com/namanvarava/Edan.Tensor/actions/runs/42912694690)는 Restore·Build·Test·후처리를 모두 성공했다.
- 병합 후 로컬 `main`과 `origin/main`은 `b28aa38914351f7a1e3c3630a36e4ce93bcc380f`로 일치했다. `codex/document-request-id-openapi-20261111` 원격 브랜치는 삭제했고, squash merge 특성상 일반 삭제가 거부된 뒤 병합 커밋과 CI를 확인하고 로컬 브랜치를 강제 삭제했다.

## 인증 Bypass 경계

Development Bypass는 `Development` 환경과 `Authentication:Mode=Bypass`에서만 활성화된다. 고정 프로필은 RFC 예약 Metadata/JWKS 주소, `EDAN-NONPROD`, `edan-nonprod-test-user`, `edan-nonprod-test-subject`, 개발 키 회전 권한 식별자를 사용한다. 이 값은 실제 외부 IdP의 증거가 아니며 운영·External·CI·배포 구성에서는 활성화되지 않는다. Bypass 사용과 실행 주체는 로그에 남기고 Tenant·조직·프로젝트 권한 검사는 유지한다.

## 외부 조건과 재개 조건

실제 비운영 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전/폐기 권한, 운영 DB, 외부 수신 시스템은 아직 제공되지 않았다. 저장소는 private이고 현재 main 보호 메타데이터는 `protected=false`이며 필요한 보호 규칙 조회는 HTTP 403으로 거부되었다. 따라서 이 항목들은 Issue [#84](https://github.com/namanvarava/Edan.Tensor/issues/84)의 별도 환경 조건으로 유지한다.

외부 조건이 제공되면 External 모드에서 정상·Issuer/Audience 오류·만료·서명 실패·알 수 없는 `kid`, Tenant 격리, 키 중첩 회전·폐기를 실행하고 GitHub main 보호 규칙을 실제 API 또는 UI에서 확인한다. 해당 검증 전에는 전체 외부 운영 완료로 표시하지 않는다.

DuckDB·Parquet, 실제 결제·Webhook·환불·유료 권리, WPF 화면, 제품 API 라이선스 강제는 각각 포맷·업무·화면·공개 API 계약이 승인되기 전까지 구현하지 않는다. 이는 미처리 결함이 아니라 현재 승인 계약의 경계다.

