# EDAN.Tensor 라이선스 v1 승인 계약

이 문서는 2026-11-10에 승인된 개발·테스트용 라이선스 계약을 기록합니다. 문서에 없는 유료 권리, 결제 완료, 운영 라이선스 또는 공급자별 동작은 승인된 계약으로 간주하지 않습니다.

## 개발 라이선스

LicenseAuthority는 개발·테스트 전용 라이선스 하나를 발급합니다. 발급 시점의 UTC 시각을 발급일과 시작일로 사용하며, 시작일부터 5년 뒤를 만료일로 계산합니다.

| 항목 | 승인 값과 규칙 |
| --- | --- |
| 대상 Tenant | `EDAN-NONPROD` |
| 제품 | `EDAN.Tensor` |
| 기능 | `Platform`, `DataSet`, `Worker` |
| LicenseId | 발급 시 새 GUID를 자동 생성합니다. |
| KeyId | LicenseAuthority의 ES256 공개키 식별자를 자동 생성합니다. |
| 상태 | `Active`, `Expired`, `Revoked`, `NotYetValid`를 사용합니다. |
| 서명 | compact JWS ES256을 사용합니다. |
| 유효 기간 | 발급일 기준 5년입니다. |

LicenseAuthority만 ES256 개인키를 보관합니다. 개인키는 저장소, 로그, HTTP 응답, 테스트 산출물에 기록하지 않으며 API에는 공개 JWK와 KeyId만 제공합니다. 기본 저장 경로는 사용자 로컬 응용 프로그램 데이터 경로이고, 저장소 안의 개발 경로를 사용하더라도 비밀 파일을 추적하지 않도록 보호합니다.

개발 라이선스 발급·폐기 API는 `Development` 환경과 명시적인 개발 발급 플래그가 모두 켜진 경우에만 열립니다. 운영 환경에서는 발급 경로가 닫히며, 개발 라이선스를 운영 Tenant나 운영 배포의 권리 증거로 사용할 수 없습니다. 인증 Bypass는 별도 개발 인증 경계이며 라이선스 검증을 대신하지 않습니다.

## 검증 계약

이번 승인에서 말하는 라이선스 적용 API는 별도 `LicenseAuthority`의 `/api/licenses/validate` 검증 API를 뜻합니다. 이 API는 공개키로 발급자, ES256 서명, KeyId, 제품, 대상 Tenant, 기능, 시작일, 만료일, 상태를 검증합니다. 폐기 목록은 LicenseAuthority의 상태 저장소에서 별도로 확인합니다. 기본 권한은 거부하고 라이선스에 없는 기능은 허용하지 않습니다.

제품 `ApiHost`의 Platform·DataSet·Worker 요청에 라이선스를 자동으로 강제하는 전달 헤더, 공개키 공급 방식, 기능 매핑, 장애 시 fail-closed 계약은 이 승인에 포함되지 않습니다. 제품 API의 기존 공개 계약·Tenant 격리·인증·Worker 계약을 변경하지 않으며, 별도 계약이 승인되기 전에는 제품 API에 임의의 라이선스 미들웨어를 추가하지 않습니다.

다음 사례를 각각 거부하는 회귀 테스트를 유지합니다.

- 서명과 Tenant·제품·기능이 일치하는 정상 라이선스는 허용합니다.
- 서명을 바꾼 위조 토큰은 거부합니다.
- 만료된 토큰은 거부합니다.
- 시작 전 토큰은 거부합니다.
- 다른 Tenant를 요청하면 거부합니다.
- 폐기 목록에 있는 라이선스는 거부합니다.
- 공개키에 없는 KeyId는 거부합니다.

현재 구현은 `EDAN.Tensor.LicenseAuthority.sln`에서 발급·공개키·검증·폐기 경계를 독립적으로 구성하며, `EDAN.Tensor.sln`에도 동일한 계약과 회귀 테스트를 포함합니다. 실제 HTTP 프로세스 스모크는 실행 환경의 loopback 소켓 권한을 허용한 환경에서 재실행해야 합니다.

LicenseAuthority만 개인키를 포함한 서명 구현을 참조합니다. ApiHost는 Licensing Contracts만 참조하고 WorkerHost는 Licensing Infrastructure를 참조하지 않으므로, 제품 Host 산출물에 LicenseAuthority 개인키 처리 코드를 배포하지 않습니다.

## 권한 모델

EDAN.Tensor는 세계적으로 널리 사용되는 역할 기반 접근 제어(RBAC)를 권고하고 적용합니다. Tenant, Organization, Project 범위는 서로 분리하고 최소 권한과 기본 거부를 유지합니다. 기존 Tenant 관리자, 조직 역할, 프로젝트 역할 체계를 보존하며 권한 변경은 감사 기록으로 남깁니다.

## 결제 경계

이번 구현에서는 결제 공급자를 확정하지 않습니다. `IPaymentProvider`와 `PaymentProviderGateway`가 공급자를 교체할 수 있는 추상화 경계를 제공하며, 공급자가 없으면 `commercial.payment_provider_not_selected` 오류로 실패합니다. 상품 가격, 실제 결제 호출, Webhook, 환불, 유료 entitlement는 공급자 선정과 별도 업무 계약 승인 뒤 구현합니다. 공급자가 선정되기 전에는 결제 완료나 유료 권한을 성공으로 처리하지 않습니다.

