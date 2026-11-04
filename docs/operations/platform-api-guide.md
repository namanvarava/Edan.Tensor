# Platform 관리 API 사용 안내

## 기본 주소

API Host를 실행한 뒤 다음 경로를 사용합니다.

- 기본 상태: `/`
- 서비스 상태: `/health`
- 데이터베이스 상태: `/health/database`
- Platform API: `/api/platform`

## OpenAPI 문서

.NET 10 기본 OpenAPI 생성 기능을 사용하며, 개발 환경에서 `/openapi/v1.json` 문서를 제공합니다. 현재 API Host에는 Swagger UI 화면을 내장하지 않았으며, OpenAPI JSON을 Swagger UI 또는 호환 도구에 연결해 사용할 수 있습니다.

## 인증 및 Tenant 선택

모든 Platform 관리 API는 JWT Bearer 인증을 사용합니다.

- 인증 헤더: `Authorization: Bearer {access-token}`
- 필수 JWT 클레임: `sub` (외부 인증 주체 식별자)
- 플랫폼 관리자 역할: `role=platform_admin`
- Tenant 선택 헤더: `X-Tenant-Id: {tenant-guid}`
- Tenant 소속 여부는 JWT의 Tenant 클레임이 아니라 데이터베이스의 활성 소속 레코드로 확인합니다.

요청마다 `X-Tenant-Id`로 Tenant를 선택합니다. 한 외부 인증 주체는 여러 Tenant에 각각 소속될 수 있습니다. 선택한 Tenant에 활성 소속이 없으면 HTTP 403을 반환합니다.

## Tenant 생성 및 활성화

`POST /api/platform/tenants`

```json
{
  "code": "ACME",
  "name": "에이씨미"
}
```

성공하면 HTTP 201과 생성된 Tenant 식별자를 반환합니다. 생성 직후 상태는 Provisioning입니다.

`POST /api/platform/tenants/{tenantId}/activate`

활성화 성공 시 HTTP 204를 반환합니다. 활성화된 Tenant에만 구성원과 조직을 등록할 수 있습니다.

## Tenant 구성원 등록

플랫폼 관리자는 외부 인증 주체의 `sub`를 Tenant에 연결합니다.

`POST /api/platform/tenants/{tenantId}/memberships`

```json
{
  "subjectId": "identity-provider-user-123",
  "role": "Administrator"
}
```

지원 역할은 `Member`와 `Administrator`입니다. 현재 두 역할 모두 Tenant 기본 API에 접근할 수 있으며, 조직·프로젝트별 세부 권한은 후속 구현 대상입니다. 동일 Tenant에서 동일 주체의 중복 소속은 HTTP 409로 거부합니다.

## 조직 생성

`POST /api/platform/tenants/{tenantId}/organizations`

```json
{
  "code": "OPS",
  "name": "운영 조직"
}
```

요청 경로의 Tenant와 `X-Tenant-Id`가 일치하고, 호출 주체가 해당 Tenant의 활성 구성원이어야 합니다.

## 프로젝트 생성

`POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/projects`

```json
{
  "code": "CORE",
  "name": "핵심 프로젝트"
}
```

프로젝트가 소속될 조직은 선택한 Tenant에 속해야 합니다.

## 사용자 초대

`POST /api/platform/tenants/{tenantId}/users/invitations`

```json
{
  "email": "user@example.com",
  "displayName": "사용자"
}
```

이메일 중복은 Tenant 범위에서 검사합니다. 현재 사용자 초대와 외부 Identity Provider 계정 연결 및 초대 수락 흐름은 분리되어 있으며, 계정 연결은 후속 단계 대상입니다.

## 자원 등록

`POST /api/platform/resources`

```json
{
  "organizationId": null,
  "projectId": null,
  "resourceType": "DataSet",
  "name": "기본 데이터"
}
```

Tenant 범위는 요청 본문이 아니라 `X-Tenant-Id`에서 결정합니다. 조직 또는 프로젝트 범위를 지정할 수 있으며, 프로젝트 범위를 지정할 때는 조직 범위도 함께 지정해야 합니다.

## 정책 등록

`POST /api/platform/policies`

```json
{
  "organizationId": null,
  "projectId": null,
  "key": "retention.days",
  "value": "30"
}
```

Tenant 범위는 요청 본문이 아니라 `X-Tenant-Id`에서 결정합니다.

## 오류 응답

업무 오류는 다음 공통 JSON 형식으로 반환합니다.

```json
{
  "code": "platform.tenant.unavailable",
  "description": "활성 상태의 테넌트만 조직을 생성할 수 있습니다."
}
```

- 400: 요청 값 또는 도메인 규칙 위반
- 401: 인증 누락 또는 유효하지 않은 토큰
- 403: 관리자 역할 부족, Tenant 선택 불일치 또는 활성 소속 없음
- 404: 대상 리소스 없음
- 409: 중복·상태·소유권 충돌
- 503: 데이터베이스 상태 확인 실패

## 필수 설정

다음 설정은 환경 변수 또는 비밀 저장소로 제공해야 합니다. 서명 키를 저장소에 커밋하지 마십시오.

- `Authentication__Issuer`: 신뢰하는 인증 발급자
- `Authentication__Audience`: EDAN Tensor API 대상 식별자
- `Authentication__SigningKey`: UTF-8 기준 32바이트 이상의 비밀 서명 키

설정이 없거나 유효하지 않으면 JWT 검증이 성공하지 않으므로 보호 API에 접근할 수 없습니다. 상태 확인 경로는 유지됩니다. 운영 환경에서는 전용 비밀 저장소와 키 교체 절차를 사용해야 합니다.

## 현재 보안 경계와 후속 작업

현재 구현은 외부 주체의 다중 Tenant 소속 저장, 요청별 Tenant 선택, 데이터베이스 기반 활성 소속 확인을 제공합니다. Tenant 내부의 Member와 Administrator 역할은 아직 기본 API 접근에서 동일하게 취급합니다. 사용자 초대 수락과 외부 계정 연결, 조직·프로젝트별 세부 권한, 토큰 발급·갱신·폐기, 키 회전 및 JWKS 기반 비대칭 키 검증은 후속 단계에서 구현해야 합니다.

완전한 운영 보안 검증 전까지 API의 외부 공개를 제한해야 합니다. 운영 환경에서는 배포 작업에서 승인된 EF Core Migration을 별도로 적용해야 합니다.
