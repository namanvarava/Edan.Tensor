# Platform API 인증 및 Tenant 보안 설계

## 목적

Platform API는 인증된 주체의 Tenant 범위를 기준으로 요청을 처리합니다. 클라이언트가 임의로 전달한 Tenant 식별자를 신뢰하지 않으며, 인증·인가 경계를 API Host에 명시합니다.

## 인증 구조

- API Host는 JWT Bearer 토큰을 검증합니다.
- 토큰의 발급자(`iss`), 대상(`aud`), 만료(`exp`), 서명을 검증합니다.
- 서명 키는 환경 설정 또는 비밀 저장소에서 주입하며 저장소에 포함하지 않습니다.
- 외부 Identity Provider가 사용자 인증과 토큰 발급을 담당합니다.
- API Host는 토큰 발급자가 아니며 수신 토큰의 신뢰성을 검증합니다.

필수 설정은 `Authentication:Issuer`, `Authentication:Audience`, `Authentication:SigningKey`입니다. 서명 키는 UTF-8 기준 32바이트 이상이어야 합니다. 설정이 없거나 유효하지 않으면 API Host 시작을 거부합니다.

## 표준 클레임

| 클레임 | 목적 |
|---|---|
| `sub` | 인증 주체의 고유 식별자 |
| `tenant_id` | 현재 요청에서 사용할 Tenant GUID |
| `role` | 플랫폼 관리자 등 전역 역할 |

현재 지원 역할은 다음과 같습니다.

- `platform_admin`: Tenant 생성 및 최초 활성화
- `tenant_member`: Tenant 범위의 조직·프로젝트·사용자·자원·정책 API 접근

## Tenant 경계

### Tenant 경로 API

`/tenants/{tenantId}/...` 경로는 인증된 `tenant_id`와 경로의 `tenantId`가 일치해야 합니다. 불일치는 HTTP 403으로 거부합니다.

### 자원 및 정책 API

자원·정책 생성 요청은 TenantId를 본문에서 받지 않습니다. `ITenantContext`가 검증된 JWT의 `tenant_id`를 읽어 ResourceScope를 생성합니다. 조직·프로젝트 식별자는 응용 서비스의 소속 검증을 추가로 통과해야 합니다.

### Tenant 생성과 활성화

Tenant 생성과 최초 활성화는 `platform_admin` 역할만 수행할 수 있습니다. Tenant 구성원 토큰만으로는 이 작업을 수행할 수 없습니다.

## 계층 및 책임

- API Host: JWT 검증, 클레임에서 Tenant 컨텍스트 제공, HTTP 경로와 Tenant 범위 비교
- Platform.Application: Tenant·Organization·Project 소속 관계 및 업무 규칙 검증
- Platform.Domain: Tenant 소유권과 상태 전이 규칙 유지
- Platform.Infrastructure: 데이터베이스 제약과 영속성 보장

도메인 계층은 ASP.NET Core 또는 JWT 구현에 의존하지 않습니다.

## 테스트 기준

통합 테스트는 외부 인증 서버 대신 테스트 전용 인증 핸들러를 사용합니다. 테스트 전용 헤더는 테스트 호스트에서만 처리하며 실제 API Host에는 등록하지 않습니다.

- 익명 Tenant 생성 요청은 401
- 관리자 역할이 없는 Tenant 생성 요청은 403
- Tenant 경로와 토큰의 Tenant가 다르면 403
- Tenant 클레임이 없는 자원·정책 요청은 403
- 정상 Tenant 클레임으로 기존 프로비저닝 흐름이 동작
- 자원·정책 요청 본문에는 TenantId를 포함하지 않음

## 후속 보안 과제

현재 구현은 하나의 `tenant_id` 클레임을 현재 요청 범위로 사용합니다. 다음 항목은 별도 단계에서 설계·검증해야 합니다.

1. 다중 Tenant 소속 사용자와 안전한 Tenant 전환
2. 사용자·조직·프로젝트별 세부 권한 및 정책 평가
3. 초대 수락과 사용자 계정 연결
4. Identity Provider 연동, 토큰 발급·갱신·폐기
5. 비대칭 서명 및 JWKS 키 조회·회전
6. 보안 감사 로그, 속도 제한, 비밀 키 운영 정책

현재 구현은 기본 인증·인가 경계이며, 위 과제까지 완료되기 전에는 운영 보안 적합성을 최종 승인하지 않습니다.
