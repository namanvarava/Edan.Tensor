# Platform 관리 API 사용 안내

## 기본 주소

API Host를 실행한 뒤 다음 경로를 사용합니다.

- 기본 상태: `/`
- 서비스 상태: `/health`
- 데이터베이스 상태: `/health/database`
- Platform API: `/api/platform`

## OpenAPI 문서

.NET 10 기본 OpenAPI 생성 기능을 사용하며, 개발 환경에서 다음 JSON 문서를 제공합니다.

- OpenAPI 3.1 문서: `/openapi/v1.json`
- API 경로 및 요청 모델 확인: OpenAPI 문서의 `paths` 및 `components/schemas`
- Swagger UI를 사용하는 경우 위 JSON 주소를 문서 원본으로 등록할 수 있습니다.

OpenAPI 문서는 개발 환경에서만 노출됩니다. 현재 API Host에는 Swagger UI 화면을 내장하지 않았으며, OpenAPI JSON을 Swagger UI 또는 호환 도구에 연결해 사용할 수 있습니다.

## HTTP 통합 테스트

`tests/EDAN.Tensor.ApiHost.IntegrationTests`는 실제 API Host를 시작하고 임시 SQLite 데이터베이스를 사용합니다.

- OpenAPI 문서가 Platform 경로를 포함하는지 확인합니다.
- 테넌트 생성 → 활성화 → 조직 생성 → 프로젝트 생성 → 사용자 초대 흐름을 HTTP로 검증합니다.
- 테스트마다 별도의 데이터베이스 파일을 사용하고 종료 시 정리합니다.

## 테넌트 생성 및 활성화

`POST /api/platform/tenants`

```json
{
  "code": "ACME",
  "name": "에이씨미"
}
```

성공하면 HTTP 201과 생성된 테넌트 식별자를 반환합니다. 생성 직후 테넌트 상태는 Provisioning입니다.

`POST /api/platform/tenants/{tenantId}/activate`

활성화 성공 시 HTTP 204를 반환합니다. 활성화된 테넌트만 조직·프로젝트·사용자·자원·정책 생성에 사용할 수 있습니다.

## Tenant 구성원 등록\n\n플랫폼 관리자는 외부 인증 주체의 `sub`를 Tenant에 연결할 수 있습니다. 한 사용자는 여러 Tenant에 각각 소속될 수 있으며, 요청마다 Tenant를 선택합니다.\n\n`POST /api/platform/tenants/{tenantId}/memberships`\n\n```json\n{\n  "subjectId": "identity-provider-user-123",\n  "role": "Administrator"\n}\n```\n\n지원 역할은 `Member`와 `Administrator`입니다. 현재는 두 역할 모두 Tenant 기본 API에 접근할 수 있으며, 조직·프로젝트별 세부 권한은 후속 구현 대상입니다.\n\nTenant 구성원 API 요청에는 다음 헤더가 필요합니다.\n\n```http\nAuthorization: Bearer {access-token}\nX-Tenant-Id: {tenant-guid}\n```\n\n선택한 Tenant에 활성 소속이 없으면 HTTP 403을 반환합니다.\n\n## 조직 생성

`POST /api/platform/tenants/{tenantId}/organizations`

```json
{
  "code": "OPS",
  "name": "운영 조직"
}
```

## 프로젝트 생성

`POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/projects`

```json
{
  "code": "CORE",
  "name": "핵심 프로젝트"
}
```

요청 경로의 테넌트와 조직 소속이 일치해야 합니다.

## 사용자 초대

`POST /api/platform/tenants/{tenantId}/users/invitations`

```json
{
  "email": "user@example.com",
  "displayName": "사용자"
}
```

이메일 중복은 테넌트 범위에서 검사합니다.

## 자원 등록

`POST /api/platform/resources`

```json
{
  "tenantId": "테넌트 GUID",
  "organizationId": null,
  "projectId": null,
  "resourceType": "DataSet",
  "name": "기본 데이터"
}
```

조직 또는 프로젝트 범위를 지정할 수 있습니다. 프로젝트 범위를 지정할 때는 조직 범위도 함께 지정해야 합니다.

## 정책 등록

`POST /api/platform/policies`

```json
{
  "tenantId": "테넌트 GUID",
  "organizationId": null,
  "projectId": null,
  "key": "retention.days",
  "value": "30"
}
```

## 오류 응답

업무 오류는 다음 공통 JSON 형식으로 반환합니다.

```json
{
  "code": "platform.tenant.unavailable",
  "description": "활성 상태의 테넌트만 조직을 생성할 수 있습니다."
}
```

- 400: 요청 값 또는 도메인 규칙 위반
- 404: 대상 리소스 없음
- 409: 중복·상태·소유권 충돌
- 503: 데이터베이스 상태 확인 실패

## 인증 및 Tenant 권한

모든 Platform 관리 API는 JWT Bearer 인증을 사용합니다.

- 인증 헤더: `Authorization: Bearer {access-token}`
- 필수 공통 클레임: `sub` (인증 주체 식별자)
- 현재 Tenant 선택 헤더: `X-Tenant-Id: {tenant-guid}`\n- Tenant 구성원 권한은 JWT의 `sub`와 데이터베이스의 활성 소속 레코드로 확인
- 플랫폼 관리자 역할: `role=platform_admin`
- JWT 발급자·대상·서명은 API 설정과 모두 일치해야 합니다.
- Tenant 경로 API는 경로의 `tenantId`와 `X-Tenant-Id` 헤더가 일치해야 합니다.
- 자원 및 정책 API는 요청 본문의 Tenant 식별자를 받지 않고 `X-Tenant-Id` 선택값으로 범위를 결정합니다.

플랫폼 관리자만 테넌트를 생성하고 최초 활성화할 수 있습니다. 조직·프로젝트·사용자·자원·정책 API는 Tenant 구성원 인증을 요구합니다.

### 필수 설정

다음 설정은 환경 변수 또는 비밀 저장소로 제공해야 합니다. 서명 키를 저장소에 커밋하지 마십시오.

- `Authentication__Issuer`: 신뢰하는 인증 발급자
- `Authentication__Audience`: EDAN Tensor API 대상 식별자
- `Authentication__SigningKey`: UTF-8 기준 32바이트 이상의 비밀 서명 키

서명 키 또는 발급자·대상 설정이 없으면 JWT 검증이 성공하지 않으므로 보호 API에 접근할 수 없습니다. 상태 확인 경로는 유지되며, 운영 환경에서는 전용 비밀 저장소와 키 교체 절차를 사용해야 합니다.

### 인증 토큰 예시 클레임

```json
{
  "sub": "사용자 GUID 또는 인증 공급자 사용자 식별자",
  "tenant_id": "테넌트 GUID",
  "role": "tenant_member",
  "iss": "설정된 발급자",
  "aud": "설정된 API 대상",
  "exp": 1794000000
}
```

플랫폼 관리자 토큰은 `role`을 `platform_admin`으로 발급합니다. 실제 계정·토큰 발급은 외부 Identity Provider에서 수행하며 API Host는 토큰 검증을 담당합니다.

### 현재 보안 경계의 범위

현재 구현은 외부 주체의 다중 Tenant 소속을 저장하고 `X-Tenant-Id`로 요청 Tenant를 선택합니다. 사용자 초대 수락과 외부 계정 연결, 조직·프로젝트별 세부 권한, 토큰 발급·갱신·폐기, 키 회전 및 JWKS 기반 비대칭 키 검증은 후속 단계에서 구현해야 합니다.

## 보안 및 운영 주의

현재 API는 JWT 인증과 기본 Tenant 권한 검사를 적용했습니다. 다만 세부 권한, 다중 Tenant 전환, 인증 공급자 운영 및 키 관리 기능은 아직 구현되지 않았으므로 완전한 운영 보안 검증 전까지 외부 공개를 제한해야 합니다.

개발 환경에서는 설정에 따라 API Host 시작 시 Migration을 적용합니다. 운영 환경에서는 배포 작업에서 승인된 Migration을 별도로 적용해야 합니다.
