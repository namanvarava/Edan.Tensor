# Issue #84 비운영 IdP HTTPS·키 회전·GitHub 보호 검증 기록

## 판정

이 문서는 Issue #84의 현재 증거를 갱신합니다. 내부 구현과 로컬·Release 검증은 통과했지만, 실제 비운영 IdP에서 수행해야 하는 부정 시나리오와 키 폐기 시나리오가 남아 있으므로 Issue #84와 전체 개발 목표를 완료로 판정하지 않습니다.

- 기록일: 2026-11-13
- 검증 기준 원격 main SHA: `9097e8e67ec69d365318a053973e36703bcf6495`
- 검증 환경: Windows 호스트와 VirtualBox Ubuntu 비운영 VM
- 외부 IdP: Keycloak, `idp.edan.test:8443`
- 인증 모드: `External`
- 개발 Bypass: 외부 IdP 검증에서는 사용하지 않음

## 사용자 수동 검증으로 확인된 항목

| 항목 | 결과 | 증거 |
| --- | --- | --- |
| OIDC Metadata HTTPS | 통과 | Metadata 요청 HTTP 200 |
| Metadata issuer | 통과 | `https://idp.edan.test:8443/realms/EDAN-NONPROD` |
| JWKS HTTPS | 통과 | Metadata의 `jwks_uri` 요청 HTTP 200 |
| JWKS 공개키 | 통과 | RS256·SIG 공개키 2개 확인 |
| JWKS 개인키 비노출 | 통과 | JWKS에 개인키 필드가 없음 |
| 실제 토큰 발급 | 통과 | Keycloak에서 Bearer 토큰 발급 성공 |
| 토큰 issuer·audience | 통과 | issuer가 Metadata와 일치하고 `edan-tensor-api` audience 포함 |
| 안정적인 subject | 통과 | 테스트 사용자의 고정 subject 확인 |
| 정상 Tenant 조회 | 통과 | API가 HTTP 200과 Administrator 역할을 반환 |
| 다른 Tenant 경계 | 통과 | 다른 Tenant 접근이 HTTP 403 |
| 토큰 없는 보호 요청 | 통과 | HTTP 401 |
| API의 토큰 원문 기록 | 통과 | 토큰 원문을 로그와 증거 문서에 기록하지 않음 |
| 개발 Bypass 경고 | 통과 | 외부 모드 실행에서 Bypass 경고가 발생하지 않음 |

비밀번호, Access Token 원문, 개인키는 문서·로그·저장소에 기록하지 않았습니다.

## 2026-11-13 사용자 수동 부정 토큰 검증

사용자는 비운영 Keycloak에서 정상적으로 서명된 별도 테스트 토큰을 발급하고, 같은 Tenant 보호 경로에 전송하여 다음 결과를 확인했습니다. 토큰 원문과 비밀번호는 기록하지 않았습니다.

| 시나리오 | 실행 조건 | 실제 결과 | 판정 |
| --- | --- | --- | --- |
| 잘못된 issuer | `EDAN-WRONG-ISSUER` Realm의 `edan-wrong-issuer-client`와 테스트 사용자로 발급한 토큰을 `GET /api/platform/tenants/{tenantId}/storage-usage`에 전송 | HTTP 401, `The issuer 'https://idp.edan.test:8443/realms/EDAN-WRONG-ISSUER' is invalid` | 통과 |
| 잘못된 audience | `edan-wrong-audience` 클라이언트로 발급한 토큰을 같은 보호 경로에 전송 | HTTP 401 | 통과 |

## 2026-11-13 만료 토큰 검증

사용자가 앞서 Keycloak에서 발급해 둔 정상 서명 토큰을 보호 경로에 다시 전송했습니다. 토큰 원문은 기록하지 않았으며, API가 반환한 인증 헤더의 만료 사유만 증거로 남겼습니다.

| 시나리오 | 실행 조건 | 실제 결과 | 판정 |
| --- | --- | --- | --- |
| 정상 서명 만료 토큰 | `/tmp/edan-token-after-rotation.json`에 보관된 Keycloak 발급 토큰을 `GET /api/platform/tenants/{tenantId}/storage-usage`에 전송 | HTTP 401, `Bearer error="invalid_token", error_description="The token expired at '11/12/2026 11:36:56'"` | 통과 |

이 결과는 토큰이 위조된 것이 아니라 만료된 경우에도 API가 인증을 거부하는 fail-closed 동작을 증명합니다.

위 두 결과는 Payload 문자열만 임의로 바꾼 토큰이 아니라 Keycloak이 발급한 서명 토큰으로 확인했습니다. 따라서 잘못된 issuer와 audience에 대한 API의 fail-closed 동작을 증명하지만, Issue #84 전체 완료를 의미하지는 않습니다.

## 2026-11-13 외부 JWKS 기반 서명 실패·알 수 없는 `kid` 검증

Release ApiHost를 별도 포트와 임시 SQLite 데이터베이스로 기동하여 기존 사용자 프로세스와 데이터에 영향을 주지 않도록 격리했습니다. ApiHost는 `External` 모드에서 `EDAN-NONPROD` Metadata와 JWKS를 사용했고, `idp.edan.test`를 비운영 VM `192.168.56.101`에 연결하여 외부 JWKS를 실제로 조회했습니다. Metadata 요청은 HTTP 200을 반환했고 격리된 API의 `/health`도 HTTP 200을 반환했습니다.

검증 토큰은 비운영 IdP의 개인키를 사용하거나 기록하지 않고, 메모리에서 생성한 임시 키로만 만들었습니다. 알려진 RS256 서명 `kid`를 사용하되 다른 키로 서명한 토큰과, JWKS에 없는 임시 `kid`를 사용한 토큰을 보호 경로에 전송했습니다.

| 시나리오 | 실제 결과 | 판정 |
| --- | --- | --- |
| 알려진 `kid`를 사용하지만 서명이 일치하지 않는 토큰 | `GET /api/platform/tenants/{tenantId}/storage-usage` HTTP 401 | 통과 |
| 외부 JWKS에 없는 `kid` 토큰 | 같은 보호 경로 HTTP 401 | 통과 |

토큰 원문·임시 개인키·비밀번호는 출력·로그·저장소에 기록하지 않았고, 임시 API 프로세스와 데이터베이스는 검증 후 정리했습니다. 이 결과로 서명 실패와 알 수 없는 `kid`에 대한 API의 fail-closed 동작을 확인했습니다.

## 2026-11-13 실행 환경 재확인

Codex Windows 실행 환경에서 외부 검증 경계를 읽기 전용으로 다시 확인했습니다.

- Keycloak HTTPS 포트의 TCP 연결이 성공했습니다.
- OIDC Metadata HTTPS 요청이 HTTP 200을 반환했습니다.
- Metadata의 issuer와 jwks_uri가 예상한 `EDAN-NONPROD` 외부 IdP 주소를 가리켰습니다.
- JWKS 응답에서 RS256·SIG 공개키 2개를 확인했고, 개인키 필드는 발견하지 못했습니다.
- 이전 실행에서 같은 실행 환경의 API `/health` 요청이 HTTP 200을 반환했습니다. 이번 재개 시점에는 API가 HTTP 503을 반환했고, 응답 본문에서 `Worker=false`, `DataSet=false`로 확인되었습니다. 이는 현재 개발 VM의 DB 연결 상태이며, 보호 API의 만료 토큰 판정과는 별개입니다.
- 초기 읽기 전용 재확인에서는 비대화식 SSH 인증이 `Permission denied (publickey,password)`로 실패했습니다. 이후 사용자가 별도 SSH 공개키 인증을 설정했으며, 사용자 수동 검증에서만 Keycloak이 발급한 토큰을 이용한 잘못된 issuer와 audience 결과를 확인했습니다. Codex 실행 환경은 여전히 사용자 비밀번호·Access Token·개인키를 수집하지 않았습니다.

재개 시점에 Codex 실행 환경에서 외부 VM을 읽기 전용으로 다시 확인했지만, `192.168.56.101`에 대한 ICMP·SSH(22)·Keycloak HTTPS(8443) 연결이 모두 실패했고 `idp.edan.test`도 연결할 수 없었습니다. 호스트의 hosts 파일 항목은 존재했으므로 이름 설정 문제가 아니라 VirtualBox Host-Only 네트워크 또는 VM 실행 상태를 확인해야 하는 인프라 차단으로 분류했습니다. 이 확인에서는 재시도를 반복하지 않았고, 사용자 비밀번호·Access Token·개인키를 수집하지 않았습니다.

이 결과는 외부 HTTPS·Metadata·JWKS 연결 가능성을 갱신하는 증거입니다. 잘못된 issuer·audience·만료·서명 실패·알 수 없는 `kid` 검증은 실제 HTTP 결과로 보강되었으며, 키 폐기·미소속 주체 검증은 별도 수용 기준으로 유지합니다.

## 키 회전 중간 증거

Keycloak `EDAN-NONPROD`에서 다음 상태를 확인했습니다.

- `rsa-generated` 서명 Provider가 Priority 200과 100으로 2개 존재합니다.
- JWKS에 RS256·SIG 공개키 2개가 게시되었습니다.
- 신규 발급 토큰의 `kid`가 JWKS에 게시된 서명 키와 일치했습니다.
- 신규 토큰을 사용한 API Tenant 조회가 HTTP 200이었습니다.

이 결과는 새 키 게시와 새 서명 검증의 중간 통과 증거입니다. 기존 토큰을 보존한 중첩 구간과 기존 키 폐기 후 거부 결과가 없으므로 키 회전 전체 완료로 판정하지 않습니다.

## GitHub main 보호 증거

현재 원격 저장소는 공개 저장소입니다. GitHub branch API의 main 응답에서 다음을 확인했습니다.

- `protected: true`
- 보호 기능 활성화
- required status checks enforcement: `everyone`
- 필수 Check: `build-and-test`

사용자가 제공한 GitHub UI에서도 main 보호와 `build-and-test` 필수 Check 설정을 확인했습니다. 다만 현재 GitHub 연결에서는 상세 보호 API인 `/branches/main/protection` 요청이 HTTP 403 `Resource not accessible by integration`을 반환했고, rulesets 조회 결과는 비어 있습니다. 따라서 main 보호의 요약 상태와 필수 Check 등록은 확인했지만, 모든 세부 보호 옵션을 API로 독립 확인했다고 기록하지 않습니다.

## 내부 검증 증거

다음 검증은 허용된 실행 경계에서 실제 수행했습니다.

- `dotnet restore EDAN.Tensor.sln`: 성공
- `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`: 성공, 경고 0·오류 0
- `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`: 성공, 273 통과·0 실패·0 건너뜀
- `dotnet restore EDAN.Tensor.LicenseAuthority.sln`: 성공
- `dotnet build EDAN.Tensor.LicenseAuthority.sln --no-restore --configuration Release`: 성공, 경고 0·오류 0
- `dotnet test EDAN.Tensor.LicenseAuthority.sln --no-build --no-restore --configuration Release`: 성공, 12 통과·0 실패·0 건너뜀
- Host Smoke: ApiHost·WorkerHost health, database health, OpenAPI, Swagger UI, Bypass Tenant API HTTP 200; stderr 0바이트
- LicenseAuthority Smoke: 5년 개발 라이선스 발급, 공개 JWK, 정상 검증 HTTP 200, 운영 환경 개발 라이선스 거부 HTTP 403, 폐기 라이선스 거부 HTTP 403
- 전이 NuGet 취약점 조회: 알려진 취약 패키지 항목 없음
- GitHub main의 `build-and-test`: 성공

## 2026-11-13 PR #242 병합 후 main 재검증

외부 JWKS 서명 실패·알 수 없는 `kid` 검증 증거를 PR #242로 제출했고, 필수 `build-and-test` Check 성공 후 squash merge했습니다.

- PR: `https://github.com/namanvarava/Edan.Tensor/pull/242`
- 병합 커밋: `62c6d51584a0fb442d72ed71ca155013d8682b9b`
- 병합 후 `dotnet restore EDAN.Tensor.sln`: 성공
- 병합 후 `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`: 성공, 경고 0·오류 0
- 병합 후 `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`: 성공, 273 통과·0 실패·0 건너뜀
- 작업 브랜치 `codex/issue-84-signature-kid-20261113`: 로컬·원격 삭제 확인

병합 후 재검증에서도 인증 Bypass를 사용하지 않았고, 저장소에는 Access Token·비밀번호·개인키를 기록하지 않았습니다.

## 2026-11-13 PR #243 병합 후 main 재검증

PR #243에서 위 검증 문서의 기준 SHA와 병합 후 증거를 최신 `main`에 맞게 갱신했습니다.

- PR: `https://github.com/namanvarava/Edan.Tensor/pull/243`
- 필수 `build-and-test` Check: 성공
- squash 병합 커밋: `421a96c3780848868e313de20085263b4331088d`
- 병합 후 `main` 기준 Release 복원·빌드·전체 회귀 테스트: 성공
- 작업 브랜치 `codex/update-main-validation-20261113`: 로컬·원격 삭제 확인

PR #243에서도 Issue #84의 키 중첩·구 키 폐기·미소속 subject 검증과 GitHub 상세 보호 API 403을 완료로 표시하지 않았습니다.

기본 실행 경계에서 .NET workload resolver가 Windows Service Control Manager 접근 거부를 반환한 적이 있습니다. 이는 소스 오류가 아니라 실행 환경 권한 문제였으며, 허용된 실행 경계에서 같은 Release 검증을 성공시켜 결과를 확인했습니다.

## 2026-11-13 PR #244 병합 후 main 재검증

PR #244 병합 후 기준 커밋은 `9097e8e67ec69d365318a053973e36703bcf6495`입니다. 이번 실행에서는 코드 변경 없이 Release 검증을 다시 수행했습니다.

- `dotnet restore EDAN.Tensor.sln`: 상세 출력 기준 성공, 경고 0·오류 0
- `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`: 기본 병렬 실행은 종료 코드 1을 반환했지만 경고 0·컴파일 오류 0이었고, VBCSCompiler named pipe에 `UnauthorizedAccessException`이 발생했습니다.
- 원인 분리 실행: `dotnet build EDAN.Tensor.sln --no-restore --configuration Release -m:1 -p:UseSharedCompilation=false`가 전체 프로젝트를 경고 0·오류 0으로 통과했습니다.
- `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`: VSTest가 첫 테스트host에 연결하지 못해 90초 후 중단되었습니다. 동일 현상이 여러 테스트 어셈블리에서 재현되었고, 단일 프로젝트에 연결 제한을 180초로 늘린 실행에서도 testhost 연결 실패가 재현되었습니다.

이 결과는 소스 assertion 실패가 아니라 현재 Windows 실행 환경의 Roslyn named pipe와 VSTest testhost 프로세스 연결 장애로 분류합니다. 테스트 우회나 실패 무시는 하지 않았으며, PR #243 병합 전 동일 코드 기준 전체 273개 테스트 통과 증거는 보존합니다. 이 재검증에서는 소스 수정이 필요하다는 근거가 발견되지 않았습니다.

이번 재개 시점의 Codex Windows 환경에서는 `192.168.56.101`의 SSH 22번 포트와 Keycloak HTTPS 8443번 포트가 모두 TCP 연결 실패였습니다. 따라서 외부 VM에 대한 키 중첩·키 폐기·미소속 subject 재검증은 실행하지 않았습니다.

## 아직 완료하지 못한 Issue #84 수용 기준

다음 항목은 실제 비운영 IdP에서 추가로 확인해야 합니다.

1. 기존 키와 신규 키가 동시에 게시된 동안 기존 토큰과 신규 토큰이 모두 HTTP 200인지 확인합니다.
2. 기존 키를 폐기한 뒤 기존 토큰은 HTTP 401, 신규 토큰은 HTTP 200인지 확인합니다.
3. Tenant에 소속되지 않은 subject가 보호된 Tenant 경로에서 HTTP 403인지 확인합니다.

2026-11-13 재확인에서는 외부 HTTPS·Metadata·JWKS 연결과 잘못된 issuer·audience·만료·서명 실패·알 수 없는 `kid` 토큰 거부를 확인했습니다. 키 중첩·키 폐기·미소속 subject 시나리오는 아직 실제 HTTP 결과가 없으므로 완료로 기록하지 않습니다.

Issue #84는 위 3개 기준이 실제 응답 증거로 채워질 때까지 열린 상태로 유지합니다. 예약 작업이나 자동화는 생성하지 않았습니다.

## 2026-11-13 최신 재개 검증

사용자 설정이 반영된 현재 실행 환경에서 읽기 전용 재검증을 수행했습니다.

- VirtualBox Host-Only 경계에서 `192.168.56.101:22` SSH 연결이 성공했습니다.
- Keycloak HTTPS 경계에서 `192.168.56.101:8443` TCP 연결이 성공했습니다.
- VM의 Keycloak 컨테이너가 `192.168.56.101:8443`에 게시되어 실행 중이고, PostgreSQL·SQL Server·Oracle 컨테이너도 실행 중임을 확인했습니다.
- `https://192.168.56.101:8443/realms/EDAN-NONPROD/.well-known/openid-configuration` 요청이 HTTP 200을 반환했습니다.
- 현재 API `/health` 요청이 HTTP 200을 반환했고 Platform·Worker·DataSet·DataSetContent 연결 상태가 모두 `true`였습니다.
- 기존 테스트 세션의 Access Token은 만료되어 HTTP 401을 반환했습니다. 응답에 포함된 refresh token으로 새 Access Token을 갱신한 뒤 보호 경로 `GET /api/platform/tenants/{tenantId}/storage-usage`가 HTTP 200을 반환했습니다.
- 갱신된 토큰은 원문을 출력하지 않고 검증했으며, issuer·audience는 `EDAN-NONPROD` 외부 IdP와 `edan-tensor-api` 계약에 맞는지 확인했습니다.

이 재검증은 외부 네트워크·Metadata·정상 토큰·보호 경로의 현재 연결성을 보강합니다. 키 중첩 구간의 기존·신규 토큰 동시 200, 기존 키 제거 후 기존 토큰 401 및 신규 토큰 200, 미소속 subject의 보호 경로 403은 아직 별도 실제 응답 증거가 없으므로 Issue #84 완료 조건으로 남겨 둡니다.

같은 재개 시점에 관리자 권한으로 다음 Release 검증도 수행했습니다.

- `dotnet restore EDAN.Tensor.sln`: 성공, 경고 0·오류 0
- `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`: 성공, 경고 0·오류 0
- `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`: 성공, 273 통과·0 실패·0 건너뜀

일반 권한에서 `dotnet --info`가 Windows Service Control Manager 액세스 거부로 종료했던 현상은 관리자 권한 복원에서 재현되지 않았습니다. 이는 소스 결함이 아니라 실행 권한 차이였으며, 권한 상승으로 테스트를 우회하거나 실패를 무시하지 않았습니다.
