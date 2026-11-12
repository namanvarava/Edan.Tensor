# Issue #84 비운영 IdP HTTPS·키 회전·GitHub 보호 검증 기록

## 판정

이 문서는 Issue #84의 현재 증거를 갱신합니다. 내부 구현과 로컬·Release 검증은 통과했지만, 실제 비운영 IdP에서 수행해야 하는 부정 시나리오와 키 폐기 시나리오가 남아 있으므로 Issue #84와 전체 개발 목표를 완료로 판정하지 않습니다.

- 기록일: 2026-11-12
- 검증 기준 원격 main SHA: `ee72aa3b22958da0ec9303479ae7168b518f023a`
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

기본 실행 경계에서 .NET workload resolver가 Windows Service Control Manager 접근 거부를 반환한 적이 있습니다. 이는 소스 오류가 아니라 실행 환경 권한 문제였으며, 허용된 실행 경계에서 같은 Release 검증을 성공시켜 결과를 확인했습니다.

## 아직 완료하지 못한 Issue #84 수용 기준

다음 항목은 실제 비운영 IdP에서 추가로 확인해야 합니다.

1. 유효한 서명을 유지한 잘못된 issuer 토큰이 HTTP 401인지 확인합니다.
2. 유효한 서명을 유지한 잘못된 audience 토큰이 HTTP 401인지 확인합니다.
3. 실제 서명 기반 만료 토큰이 HTTP 401인지 확인합니다.
4. 서명이 변조된 토큰과 알 수 없는 `kid` 토큰이 HTTP 401인지 확인합니다.
5. 기존 키와 신규 키가 동시에 게시된 동안 기존 토큰과 신규 토큰이 모두 HTTP 200인지 확인합니다.
6. 기존 키를 폐기한 뒤 기존 토큰은 HTTP 401, 신규 토큰은 HTTP 200인지 확인합니다.
7. Tenant에 소속되지 않은 subject가 보호된 Tenant 경로에서 HTTP 403인지 확인합니다.

현재 Codex 실행 환경에서는 사용자의 VirtualBox VM `192.168.56.101:8443`에 직접 연결할 수 없으므로 위 외부 실패·폐기 시나리오를 대신 수행하지 않았습니다. 사용자가 VM에서 결과를 확인하면 이 문서와 Issue #84에 결과와 검증 당시 main SHA를 추가할 수 있습니다.

Issue #84는 위 7개 기준이 실제 응답 증거로 채워질 때까지 열린 상태로 유지합니다. 예약 작업이나 자동화는 생성하지 않았습니다.
