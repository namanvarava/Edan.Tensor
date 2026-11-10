# 2026-11-10 승인 라이선스 구현 검증 기록

## 범위

사용자가 승인한 개발 라이선스 계약을 구현했습니다. 별도 `EDAN.Tensor.LicenseAuthority.sln`에 기존 Licensing 계층을 조합하고, 개발·테스트 전용 발급·공개키·검증·폐기 API와 파일 기반 로컬 저장소를 구성했습니다.

구현은 다음 계약을 고정합니다.

- Tenant `EDAN-NONPROD`, 제품 `EDAN.Tensor`, 기능 `Platform`·`DataSet`·`Worker`
- 발급일 기준 5년, 자동 LicenseId·KeyId, Active·Expired·Revoked·NotYetValid 상태
- compact JWS ES256, LicenseAuthority 전용 개인키, API 공개 JWK
- Development 환경과 명시적 플래그가 동시에 있어야 하는 개발 발급·폐기 경계
- RBAC, 기본 거부, 최소 권한, Tenant·Organization·Project 범위, 기존 감사 경계 보존
- 결제 공급자 미확정, Paddle·Stripe·PayPal 권고만 기록, 공급자 중립 `IPaymentProvider` 경계와 미선정 시 fail-closed 처리, 유료 권한 미부여

## 변경 근거

기존 Licensing 프로젝트는 경계 표시만 제공했으므로, 승인 전에는 발급 규칙을 추측하지 않았습니다. 승인 후 최소 안전 범위로 다음을 추가했습니다.

- Contracts에 공개 라이선스와 검증 요청·응답 계약을 추가했습니다.
- Application에 단일 개발 라이선스 발급·재사용과 검증 서비스를 추가했습니다.
- Infrastructure에 파일 개인키, 공개 JWK, ES256 compact JWS, 폐기 상태 저장소를 추가했습니다.
- LicenseAuthority Host에 개발 발급, 공개키, 검증, 폐기 경로를 추가했습니다.
- 개인키·토큰을 로그와 저장소에 남기지 않는 경계를 유지했습니다.
- 정상·위조·만료·시작 전·다른 Tenant·폐기·알 수 없는 KeyId 회귀 테스트를 추가했습니다.
- WebApplicationFactory 기반 HTTP 통합 테스트로 발급·재사용·공개키·정상·위조·만료·시작 전·Tenant 불일치·알 수 없는 KeyId·폐기·운영 차단 응답을 추가로 확인했습니다.
- Commercial에는 공급자를 교체할 수 있는 `IPaymentProvider`·`PaymentProviderGateway`를 추가하고, 공급자 미선정 상태에서 결제 완료나 유료 권한을 성공으로 만들지 않는 회귀 테스트를 추가했습니다.
- 개인키 서명 구현은 LicenseAuthority Host만 참조하도록 WorkerHost의 불필요한 Licensing Infrastructure 참조를 제거했습니다. ApiHost는 공개 Contracts만 참조합니다.
- 발급 회귀 테스트에서 자동 LicenseId·KeyId, 승인된 전체 기능 목록, 공개 JWK의 개인키 값(`d`) 부재를 확인합니다.
- 이번 승인에서 “라이선스 적용 API”는 LicenseAuthority의 `/api/licenses/validate`로 한정합니다. 제품 ApiHost에 라이선스를 강제하는 헤더·기능 매핑·장애 계약은 별도 승인 전까지 구현하지 않습니다.

## 자동 검증

```text
dotnet build EDAN.Tensor.LicenseAuthority.sln --configuration Release --no-restore
  경고 0개·오류 0개

dotnet test EDAN.Tensor.LicenseAuthority.sln --configuration Release --no-build --no-restore
  통과 10개·실패 0개·건너뜀 0개
```

별도 솔루션 테스트는 키 생성, 5년 기간, 단일 발급 재사용, 정상 Tenant·제품·기능, 위조 서명, 만료, 시작 전, 다른 Tenant, 기능 불일치, 폐기, 알 수 없는 KeyId, 운영 환경의 개발 라이선스 차단을 서비스 경계에서 확인합니다.

전체 솔루션의 최종 Release Restore·Build·Test는 Restore 성공, Build 경고 0개·오류 0개, 12개 테스트 프로젝트에서 262개 통과·실패 0개·건너뜀 0개를 기록했습니다. 취약 패키지 감사도 모든 프로젝트에서 취약 패키지 없음으로 끝났습니다.

권한이 허용된 실행 환경에서 `tools/license-authority-smoke.ps1`를 실행하여 Development health 200, 단일 개발 라이선스 발급·재사용과 5년 기간, 공개 JWK 200, 정상 검증 200, Production 개발 라이선스 차단 403, 폐기 라이선스 차단 403을 확인했습니다. 로그에는 토큰이 없었고 임시 디렉터리는 삭제되었습니다. 같은 환경에서 `tools/host-smoke.ps1`의 ApiHost·WorkerHost health, database health, OpenAPI, Swagger UI, Development Bypass Tenant API가 모두 HTTP 200이고 양쪽 stderr 0바이트임을 재확인했습니다.

제한된 Work 실행 경계에서는 처음 loopback 소켓 요청이 `액세스 권한에 의해 숨겨진 소켓을 시도했습니다`로 차단되고 MSBuild·testhost가 오류 0개 상태에서 멈추는 현상이 있었으나, 권한이 허용된 실행 환경에서 동일 명령을 재실행하여 코드·테스트 실패가 아닌 실행 환경 제한으로 분리했습니다.

## 남은 조건

실제 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB, 외부 수신 시스템, GitHub 보호 규칙 HTTP 403은 이 구현에 포함하지 않습니다. DuckDB·Parquet, Commercial 상품·가격·실제 결제·Webhook·환불·유료 권리 업무 흐름, WPF 화면도 별도 승인 계약이 필요합니다.

