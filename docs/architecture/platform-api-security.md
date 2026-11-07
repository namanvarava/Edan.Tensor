# Platform API 인증 및 Tenant 보안 설계

## 목적

Platform API는 인증된 외부 주체의 Tenant 소속을 확인한 후 요청을 처리합니다. Tenant 식별자를 JWT에 고정하지 않고 요청마다 선택하며, 실제 접근 권한은 데이터베이스의 소속 레코드로 확인합니다.

## 인증 구조

- API Host는 JWT Bearer 토큰을 검증합니다.
- 토큰의 발급자(`iss`), 대상(`aud`), 만료(`exp`), 서명을 검증합니다.
- 서명 키는 환경 설정 또는 비밀 저장소에서 주입하며 저장소에 포함하지 않습니다.
- 외부 Identity Provider가 사용자 인증과 토큰 발급을 담당합니다.
- API Host는 토큰 발급자가 아니며 수신 토큰의 신뢰성을 검증합니다.
- 개발·자동 검증에서는 `Authentication:MetadataAddress`로 로컬 OpenID Connect 메타데이터와 JWKS 서버를 지정할 수 있습니다.
- 메타데이터 주소를 사용하면 `JwtBearer`가 JWKS를 조회하고 `RefreshOnIssuerKeyNotFound`로 알 수 없는 `kid`의 키를 다시 조회합니다. 키 회전은 새 키와 기존 키가 겹치는 안전한 전환 구간에서 검증합니다.
- `Authentication:RequireHttpsMetadata=false`는 로컬 HTTP 테스트 서버에서만 허용하며, 운영에서는 HTTPS 메타데이터 주소와 비밀 저장소를 사용합니다.
- `Authentication:Mode=Bypass`는 Development 환경에서만 외부 JWT 검증을 생략하는 개인 개발 모드입니다. 토큰이 없는 모든 요청도 고정된 개발 주체로 인증하지만 Tenant 소속과 조직·프로젝트 권한 검사는 수행하며, 운영 환경에서는 시작 단계에서 거부합니다.
- `Authentication:Mode=External`은 Metadata·Issuer·Audience·JWKS 검증을 사용합니다. MetadataAddress와 Issuer는 도메인 또는 개인 개발망의 IP 주소를 포함할 수 있으며, HTTP 주소는 Development에서만 허용합니다.
- Development Bypass가 활성화되면 `DevelopmentData:SeedBypassTenant=true` 설정에 따라 `EDAN-NONPROD` Tenant와 Bypass `sub`의 소속을 시작 시 멱등적으로 준비합니다. 이미 같은 Tenant 또는 소속 레코드가 있으면 역할·상태를 덮어쓰지 않으며, 이 초기화는 외부 IdP 설정이나 운영 데이터의 대체 수단이 아닙니다.

## 비운영 외부 IdP 권장 구성

향후 유지보수자는 외부 인증 경계를 검증할 때 운영 인증 서버를 직접 사용하지 않고 `Edan.Identity`라는 별도 비운영 OIDC 서비스를 사용한다. API와 IdP는 별도 프로세스 또는 별도 배포 환경으로 운영하여 인증 서버 장애와 API 장애를 구분한다.

| 설정 | 권장값 또는 규칙 |
|---|---|
| 테스트 주소 | `https://auth-test.<소유한도메인>` |
| Issuer | `https://auth-test.<소유한도메인>`이며 토큰의 `iss` 값과 정확히 일치해야 함 |
| Audience | `edan-tensor-api`이며 토큰의 `aud` 값과 정확히 일치해야 함 |
| Metadata | `https://auth-test.<소유한도메인>/.well-known/openid-configuration` |
| JWKS | Metadata 응답의 `jwks_uri`를 사용함 |
| 테스트 Tenant | `edan-nonprod`와 같은 별도 비운영 Tenant를 사용함 |
| 테스트 주체 | 이메일이 아닌 변경되지 않는 opaque `sub` 값을 Tenant 멤버십에 등록함 |
| 서명 | RS256 기반 비대칭 키를 사용하고 기존 키와 새 키가 겹치는 회전 구간을 제공함 |

`auth-test.<소유한도메인>`은 실제 소유 도메인의 HTTPS 주소로 교체한다. `localhost`, `.local`, `.test` 주소와 자체 서명 인증서는 로컬 개발에만 사용하며 외부 경계 검증의 증거로 사용하지 않는다. 개인키, 클라이언트 비밀값, 비밀번호와 액세스 토큰은 저장소·문서·로그에 기록하지 않고 환경 변수 또는 승인된 비밀 저장소로 주입한다.

개인 개발에서 외부 IdP를 아직 준비하지 못하면 `src/Hosts/EDAN.Tensor.ApiHost/appsettings.Development.json`의 Bypass 설정을 사용한다. Bypass는 외부 로그인 서버를 대체하는 임시 개발 경계이며 서비스 배포 설정으로 복사하지 않는다. 외부 IdP 연결 시에는 `Authentication:Mode=External`로 바꾸고 실제 Metadata 주소, Issuer, Audience와 HTTPS 정책을 주입한다.

기본 Development Bypass 설정은 `EDAN-NONPROD`, `Edan Non-Production Development`, `edan-nonprod-test-subject`, `platform_admin`을 사용한다. 고정 시뮬레이션 프로필은 `https://auth-test.example.com`을 Issuer로, `https://auth-test.example.com/.well-known/openid-configuration`을 Metadata 주소로, `https://auth-test.example.com/jwks`를 JWKS 주소로, `edan-tensor-api`를 Audience로 사용한다. 이 값은 RFC 예약 도메인과 개발 식별자이며 실제 외부 IdP의 Issuer·Audience·JWKS·키 회전 권한을 증명하지 않는다. 다른 로컬 Tenant가 필요하면 `DevelopmentData:TenantCode`, `DevelopmentData:TenantName`, `Authentication:BypassSubject`를 Development 전용 구성으로 바꿀 수 있지만 실제 외부 IdP 검증으로 기록하지 않는다.

외부 경계 검증은 정상 토큰, 잘못된 Issuer, 잘못된 Audience, 만료 토큰, 서명 실패, 알 수 없는 `kid`, 기존 키와 새 키가 겹치는 키 회전을 모두 확인한다. 테스트 사용자는 IdP의 안정적인 `sub` 값을 확인한 뒤 `POST /api/platform/tenants/{tenantId}/memberships`로 Tenant의 Member 또는 Administrator 역할에 연결한다.

대칭 서명 구성의 필수 설정은 `Authentication:Issuer`, `Authentication:Audience`, `Authentication:SigningKey`입니다. 서명 키는 UTF-8 기준 32바이트 이상이어야 합니다. JWKS 구성에서는 `Authentication:MetadataAddress`를 사용하고 발급자와 대상을 계속 설정합니다. 설정이 없거나 유효하지 않으면 JWT 검증은 성공할 수 없으며 보호 API는 닫힌 상태를 유지합니다. 상태 확인 경로는 계속 사용할 수 있습니다.

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

통합 테스트는 테스트 전용 인증 핸들러와 로컬 TCP IdP/JWKS 서버를 구분하여 사용합니다. 테스트 전용 헤더는 테스트 호스트에서만 처리하며 실제 API Host에는 등록하지 않습니다. 로컬 JWKS 경계 테스트는 정상 토큰, 잘못된 Issuer/Audience, 만료, 서명 실패, 알 수 없는 `kid`, 새 키와 기존 키의 겹침 회전을 확인합니다.

- 익명 Tenant 생성 요청은 401
- 관리자 역할이 없는 Tenant 생성 요청은 403
- Tenant 경로와 `X-Tenant-Id` 선택값이 다르면 403
- 활성 소속이 없는 주체는 403
- 활성 소속을 부여받은 주체는 선택 Tenant의 기본 API에 접근
- 자원·정책 요청 본문에는 TenantId를 포함하지 않음

## Policy 기반 권한 평가 및 감사

Platform API는 Tenant·Organization·Project 계층의 접근 정책을 평가합니다. 정책은 외부 주체, Action, ResourceType, Allow·Deny 효과와 우선순위를 가집니다. Tenant 정책은 하위 범위에 상속되고 Organization 정책은 해당 프로젝트에 상속됩니다. 명시적 Deny는 Allow 및 역할 권한보다 우선하며, 활성 Tenant 소속은 정책 Allow로 우회할 수 없습니다. 평가 결과는 platform_authorization_audit 테이블에 기록합니다. 상세 설계는 [Policy 기반 권한 평가 및 감사 설계](platform-authorization-policy.md)를 참고하십시오.

## 외부 운영 검증 조건

이번 릴리스의 자동 검증은 저장소에 비밀을 남기지 않는 로컬 IdP/JWKS 서버로 완료합니다. 최종 외부 경계는 위 권장 구성을 따르는 비운영 외부 IdP 테스트 Tenant에서 같은 토큰 조건을 실행해야 합니다. 실행에는 Metadata 주소, 기대 Issuer/Audience, 테스트 주체의 Tenant 소속 및 키 회전 권한이 필요합니다.

현재 구현은 기본 인증·인가 경계와 로컬 JWKS 검증을 제공합니다. 외부 테스트 테넌트가 제공되기 전에는 그 운영 경계를 완료했다고 선언하지 않습니다.
