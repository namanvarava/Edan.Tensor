# 2026-11-12 Work 실행 및 Issue #84 감사 기록

## 기준과 범위

이 기록의 작업 시작 기준은 `main` 커밋 `53bc8535d7f95aeaa2e9f42705a5f45e11cdc3b2`입니다. 현재 승인된 내부 범위의 Release 검증과 Smoke 검증을 다시 실행하고, Issue [#84](https://github.com/namanvarava/Edan.Tensor/issues/84)의 외부 조건을 실제 증거와 분리해 기록합니다.

이 변경은 제품 API 계약, Tenant 격리, 인증·인가, Worker Lease/Fencing, LicenseAuthority의 서명 경계를 변경하지 않습니다. Smoke 도구의 PowerShell 응답 처리만 보완합니다.

## 발견한 원인과 수정

`tools/license-authority-smoke.ps1`의 `Get-ErrorResponse`가 운영 환경과 폐기 라이선스의 HTTP 403 응답을 읽을 때 `GetResponseStream()`을 직접 호출했습니다. PowerShell 7 계열의 `Invoke-RestMethod`는 `HttpResponseMessage`를 반환하고, 예외가 생성된 뒤 응답 Content를 폐기할 수 있으므로 PowerShell 5.1과 최신 PowerShell에서 Smoke가 서로 다르게 실패했습니다.

최소 안전 수정으로 다음 경계를 적용했습니다.

- `HttpResponseMessage`는 `ErrorDetails.Message`를 우선 읽고, 값이 없을 때만 응답 Content를 읽습니다.
- Windows PowerShell 5.1의 WebException 응답은 기존 스트림 읽기 경로를 유지합니다.
- HTTP 403 본문이 비어 있거나 읽기 실패한 경우에도 예외 메시지를 사용하여 검증 실패를 숨기지 않습니다.
- 제품 소스와 인증 우회 설정은 변경하지 않았습니다.

## 내부 검증 결과

권한이 허용된 실행 경계에서 다음 결과를 확인했습니다.

| 검증 | 결과 |
| --- | --- |
| `dotnet restore EDAN.Tensor.sln` | 기본 병렬 실행은 실행 경계에서 출력 없이 종료 코드 1을 반환했으며, `-m:1` 직렬 실행은 경고 0개·오류 0개로 성공했습니다. |
| `dotnet build EDAN.Tensor.sln --no-restore --configuration Release` | `-m:1` 직렬 실행에서 경고 0개·오류 0개로 성공했습니다. |
| `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release` | 총 273개 통과·실패 0개·건너뜀 0개로 성공했습니다. |
| `dotnet restore EDAN.Tensor.LicenseAuthority.sln` | 성공했습니다. |
| `dotnet build EDAN.Tensor.LicenseAuthority.sln --no-restore --configuration Release` | 경고 0개·오류 0개로 성공했습니다. |
| `dotnet test EDAN.Tensor.LicenseAuthority.sln --no-build --no-restore --configuration Release` | 12개 통과·실패 0개·건너뜀 0개로 성공했습니다. |
| `tools/host-smoke.ps1` | ApiHost·WorkerHost의 health, database health, OpenAPI, Swagger UI, Tenant API가 모두 HTTP 200이고 stderr 0바이트·임시 경계 삭제로 성공했습니다. |
| `tools/license-authority-smoke.ps1` | 개발 라이선스 단일 발급·5년 유효기간·공개 JWK·정상 검증 HTTP 200, Production 개발 라이선스 차단 HTTP 403, 폐기 라이선스 차단 HTTP 403, stderr 0바이트·임시 경계 삭제로 성공했습니다. |

제한된 실행 경계에서 기본 Build와 Test가 출력 없이 종료되거나 첫 `testhost` 연결 시간 초과로 중단된 원인은 SDK의 Windows Service Control Manager 접근 거부와 테스트 호스트 실행 권한으로 확인했습니다. 권한이 허용된 실행에서 같은 필수 명령을 성공시켰으므로 코드 컴파일 오류나 테스트 Assert 실패로 분류하지 않습니다.

## Issue #84 판정

Issue #84는 현재 열린 상태이며, 저장소 내부 구현·문서·회귀 검증의 결함을 의미하지 않습니다. GitHub 저장소는 현재 private이고, main 보호 규칙 조회 API는 `403 Resource not accessible by integration`을 반환했습니다. 따라서 보호 규칙의 실제 설정·필수 Check·관리자 우회 여부는 아직 검증하지 않았습니다.

현재 로컬 Keycloak은 VM의 `192.168.56.101`에서 개발용 HTTP Metadata를 제공하므로, 저장소의 External 개발 연결과 JWT 계약을 확인하는 데 사용할 수 있습니다. 그러나 이 환경은 HTTPS를 사용하는 실제 외부 비운영 IdP의 증거가 아니며, 다음 Issue #84 수용 기준을 아직 충족하지 않습니다.

- 실제 비운영 Metadata·JWKS HTTPS 주소와 기대 Issuer·Audience 제공
- 운영과 분리된 테스트 Tenant·사용자·안정적인 opaque `sub` 제공
- 정상·잘못된 Issuer/Audience·만료·서명 실패·알 수 없는 `kid` 검증
- 기존·신규 키 중첩과 기존 키 폐기 검증
- 다른 Tenant 접근 차단 검증
- GitHub main 보호 규칙 설정과 API 또는 UI의 실제 적용 확인

외부 조건이 제공되기 전에는 Issue #84를 완료 또는 Close로 표시하지 않습니다. 조건이 제공되면 `docs/validation/external-idp-boundary-test-runbook.md`를 따라 수동으로 한 번 실행하고, 토큰·개인키·비밀번호를 증거에 기록하지 않습니다. 예약 작업이나 자동화는 생성하지 않습니다.

## 재개 조건

1. 외부 IdP 관리자가 Metadata/JWKS HTTPS 주소, Issuer, Audience, 테스트 Tenant·사용자·안정적인 `sub`, 키 중첩·폐기 권한을 제공합니다.
2. 저장소 관리자가 private 저장소의 보호 규칙 권한을 제공하거나 저장소를 public으로 전환합니다.
3. External 모드로 API를 시작하고 런북의 모든 정상·실패·Tenant·키 회전 검증을 실행합니다.
4. GitHub main 보호 규칙을 실제 설정하고 필수 Check 적용 결과를 확인합니다.
5. 결과와 실행 당시 main SHA를 Issue #84에 기록한 뒤에만 수용 기준을 판정합니다.
