# Platform API 인증 및 Tenant 보안 설계

## 목적

Platform API는 인증된 외부 주체의 Tenant 소속을 확인한 후 요청을 처리합니다. Tenant 식별자를 JWT에 고정하지 않고 요청마다 선택하며, 실제 접근 권한은 데이터베이스의 소속 레코드로 확인합니다.

## 인증 구조

- API Host는 JWT Bearer 토큰을 검증합니다.
- 토큰의 발급자(`iss`), 대상(`aud`), 만료(`exp`), 서명을 검증합니다.
- 서명 키는 환경 설정 또는 비밀 저장소에서 주입하며 저장소에 포함하지 않습니다.
- 외부 Identity Provider가 사용자 인증과 토큰 발급을 담당합니다.
- API Host는 토큰 발급자가 아니며 수신 토큰의 신뢰성을 검증합니다.

필수 설정은 `Authentication:Issuer`, `Authentication:Audience`, `Authentication:SigningKey`입니다. 서명 키는 UTF-8 기준 32바이트 이상이어야 합니다. 설정이 없거나 유효하지 않으면 JWT 검증은 성공할 수 없으며 보호 API는 닫힌 상태를 유지합니다. 상태 확인 경로는 계속 사용할 수 있습니다.

## 표준 클레임

| 클레임 | 목적 |
|---|---|
| `sub` | 외부 인증 주체의 고유 식별자 |
| `role` | 플랫폼 관리자 등 전역 역할 |

- `platform_admin`: Tenant 생성·활성화 및 Tenant 소속 부여
- Tenant 내부 역할은 데이터베이스의 `platform_tenant_memberships`에서 관리합니다.
- 현재 `Member`와 `Administrator`는 모두 Tenant 기본 API에 접근할 수 있습니다. 세부 권한 차등은 후속 단계에서 적용합니다.

## 다중 Tenant 소속

`platform_tenant_memberships`는 외부 인증 주체(`sub`)와 Tenant를 연결합니다. 하나의 외부 주체는 여러 Tenant에 각각 소속될 수 있으며, 요청마다 `X-Tenant-Id`로 작업 범위를 선택합니다. Tenant 소속 상태와 역할은 데이터베이스에서 조회하므로 JWT의 Tenant 클레임을 권한 근거로 사용하지 않습니다.

플랫폼 관리자는 `POST /api/platform/tenants/{tenantId}/memberships`로 소속을 부여합니다. 요청 본문에는 외부 Identity Provider가 발급한 주체 식별자와 Tenant 역할을 전달합니다. 동일한 Tenant와 주체의 중복 소속은 차단합니다.

## Tenant 경계

### Tenant 경로 API

`X-Tenant-Id` 요청 헤더로 현재 작업 Tenant를 선택합니다. `/tenants/{tenantId}/...` 경로는 선택된 Tenant와 일치해야 하며, 불일치는 HTTP 403으로 거부합니다.

### 자원 및 정책 API

자원·정책 생성 요청은 TenantId를 본문에서 받지 않습니다. `ITenantContext`가 `X-Tenant-Id` 선택값을 읽고, `TenantMembershipAuthorizationFilter`가 JWT의 `sub`와 선택 Tenant의 활성 소속을 데이터베이스에서 확인합니다. 조직·프로젝트 식별자는 응용 서비스의 소속 검증을 추가로 통과해야 합니다.

### Tenant 생성·활성화 및 소속 부여

Tenant 생성, 최초 활성화, 구성원 소속 부여는 `platform_admin` 역할만 수행할 수 있습니다. Tenant 구성원 토큰만으로는 이 작업을 수행할 수 없습니다.

## 계층 및 책임

- API Host: JWT 검증, 요청 Tenant 선택값 처리, 소속 권한 검사 및 HTTP 경로 비교
- Platform.Application: Tenant 소속 부여와 조직·프로젝트 소속 업무 규칙 검증
- Platform.Domain: TenantMembership의 역할·상태 전이 규칙 유지
- Platform.Infrastructure: TenantMembership 영속성과 Tenant 외래 키·고유 제약 보장

도메인 계층은 ASP.NET Core 또는 JWT 구현에 의존하지 않습니다.

## 테스트 기준

통합 테스트는 외부 인증 서버 대신 테스트 전용 인증 핸들러를 사용합니다. 테스트 전용 헤더는 테스트 호스트에서만 처리하며 실제 API Host에는 등록하지 않습니다.

- 익명 Tenant 생성 요청은 401
- 관리자 역할이 없는 Tenant 생성 요청은 403
- Tenant 경로와 `X-Tenant-Id` 선택값이 다르면 403
- 활성 소속이 없는 주체는 403
- 활성 소속을 부여받은 주체는 선택 Tenant의 기본 API에 접근
- 자원·정책 요청 본문에는 TenantId를 포함하지 않음

## 후속 보안 과제

1. 사용자 초대 수락과 이메일 기반 외부 계정 연결
2. 사용자·조직·프로젝트별 세부 권한 및 정책 평가
3. Tenant 역할별 차등 권한과 권한 변경 감사
4. Identity Provider 실제 연동 및 토큰 발급·갱신·폐기
5. 비대칭 서명 및 JWKS 키 조회·회전
6. 보안 감사 로그, 속도 제한, 비밀 키 운영 정책

현재 구현은 기본 인증·인가 경계입니다. 위 과제가 완료되고 실제 Identity Provider와 운영 키 관리가 검증되기 전에는 운영 보안 적합성을 최종 승인하지 않습니다.
