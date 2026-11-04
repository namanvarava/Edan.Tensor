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

## 조직 생성

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

## 보안 및 운영 주의

현재 단계의 Platform API는 인증·인가 연결 전의 개발 기반입니다. 외부 네트워크나 운영 환경에 공개하지 마십시오. 다음 보안 단계에서 인증, Tenant 컨텍스트 확정, 권한 정책을 연결한 후 운영 공개를 검토해야 합니다.

개발 환경에서는 설정에 따라 API Host 시작 시 Migration을 적용합니다. 운영 환경에서는 배포 작업에서 승인된 Migration을 별도로 적용해야 합니다.
