# Platform 관리 API 사용 안내

## 기본 주소

API Host를 실행한 뒤 다음 경로를 사용합니다.

- 기본 상태: `/`
- 서비스 상태: `/health` (API와 네 개 로컬 데이터베이스)
- 데이터베이스 상태: `/health/database` (Platform·Worker·DataSet·DataSet 콘텐츠)
- Platform API: `/api/platform`

## OpenAPI 문서

.NET 10 기본 OpenAPI 생성 기능과 Swagger UI를 사용하며, Development 환경에서 `/openapi/v1.json`과 `/swagger/index.html`을 제공합니다. Swagger UI는 API 문서와 Bearer 인증·Tenant 헤더 입력을 함께 표시합니다. 운영 환경에서는 두 개발용 문서 경로를 제공하지 않습니다.

Development Host는 사용자 프로필 권한과 분리된 `App_Data/DataProtection-Keys`에 DataProtection 키를 저장합니다. 이 경로는 `.gitignore`로 저장소에서 제외하며, 운영 환경에는 적용하지 않습니다. 운영 배포는 플랫폼이 승인한 영속 키 저장소와 접근 권한을 별도로 구성해야 합니다.

## 인증 및 Tenant 선택

모든 Platform 관리 API는 기본적으로 JWT Bearer 인증을 사용합니다. 개인 개발의 Development `Authentication:Mode=Bypass`에서는 외부 IdP와 Bearer 토큰이 없어도 모든 요청을 고정된 개발 주체로 인증합니다. 이 경우에도 Tenant 소속과 리소스 권한 검사는 수행합니다. Bypass는 `auth-test.example.com`을 사용하는 고정 시뮬레이션 프로필을 기록하지만 실제 외부 IdP에 연결하거나 외부 JWT를 검증하지 않습니다.

기본 Development 설정은 시작할 때 `EDAN-NONPROD` Tenant와 `edan-nonprod-test-subject`의 Administrator 소속을 멱등적으로 준비합니다. 이미 같은 Tenant나 소속이 있으면 기존 역할과 상태를 덮어쓰지 않습니다. 이 동작은 `DevelopmentData:SeedBypassTenant=true`와 `Authentication:Mode=Bypass`가 함께 설정된 Development 환경에서만 실행됩니다.

- 인증 헤더: `Authorization: Bearer {access-token}`
- 필수 JWT 클레임: `sub` (외부 인증 주체 식별자)
- 플랫폼 관리자 역할: `role=platform_admin`
- Tenant 선택 헤더: `X-Tenant-Id: {tenant-guid}`
- Tenant 소속 여부는 JWT의 Tenant 클레임이 아니라 데이터베이스의 활성 소속 레코드로 확인합니다.

요청마다 `X-Tenant-Id`로 Tenant를 선택합니다. 한 외부 인증 주체는 여러 Tenant에 각각 소속될 수 있습니다. `GET /api/platform/tenants/mine`은 현재 인증 주체가 소속된 활성 Tenant와 역할 목록을 반환합니다. 선택한 Tenant에 활성 소속이 없으면 HTTP 403을 반환합니다.

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

지원 역할은 `Member`와 `Administrator`입니다. 두 역할 모두 허용된 Tenant 구성원 API와 읽기 작업에 접근할 수 있지만, 조직·프로젝트 생성과 사용자 초대·소속 배정·정책 관리는 `Administrator`만 수행할 수 있습니다. 동일 Tenant에서 동일 주체의 중복 소속은 HTTP 409로 거부합니다.

## 조직 생성

`POST /api/platform/tenants/{tenantId}/organizations`

이 API는 선택한 Tenant의 `Administrator`만 호출할 수 있습니다. 일반 `Member`는 활성 소속이 있어도 Tenant 구조를 변경할 수 없습니다.

```json
{
  "code": "OPS",
  "name": "운영 조직"
}
```

요청 경로의 Tenant와 `X-Tenant-Id`가 일치하고, 호출 주체가 해당 Tenant의 활성 구성원이어야 합니다.

## 프로젝트 생성

`POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/projects`

이 API는 선택한 Tenant의 `Administrator`만 호출할 수 있습니다. 프로젝트가 소속될 조직은 선택한 Tenant에 속해야 합니다.

```json
{
  "code": "CORE",
  "name": "핵심 프로젝트"
}
```

## 사용자 초대

`POST /api/platform/tenants/{tenantId}/users/invitations`

이 API는 선택한 Tenant의 `Administrator`만 호출할 수 있습니다.

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

## 조직·프로젝트 소속 및 권한

Tenant Administrator는 조직·프로젝트 소속을 부여할 수 있습니다.

조직 소속 등록:

`POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/memberships`

```json
{
  "subjectId": "identity-provider-user-123",
  "role": "Contributor"
}
```

프로젝트 소속 등록:

`POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/projects/{projectId}/memberships`

```json
{
  "subjectId": "identity-provider-user-123",
  "role": "Reader"
}
```

조직·프로젝트 역할은 `Reader`, `Contributor`, `Manager`입니다. Reader는 생성할 수 없고 Contributor는 자신이 속한 범위에서 생성할 수 있습니다. Organization Manager는 하위 Project 범위까지 생성 권한을 상속합니다. Organization Contributor는 프로젝트 권한을 자동으로 상속하지 않습니다. Tenant Administrator는 선택한 Tenant의 모든 하위 범위에서 생성할 수 있습니다.

자원 및 정책 생성 API는 모두 동일한 범위 권한 평가를 수행합니다. Tenant 소속만 가진 Member는 조직·프로젝트 범위 자원을 생성할 수 없습니다.

상세 권한 모델은 [조직·프로젝트 권한 API 안내](platform-scoped-authorization-guide.md)를 참고하십시오.

## DataSet 검증 예약

`POST /api/platform/tenants/{tenantId}/datasets/{dataSetId}/validations`는 Tenant Administrator가 호출하며, 요청에는 `expectedDataSetVersion`, `schemaVersion`, `profileId`, `profileRevision`을 지정하고 `Idempotency-Key` 헤더를 함께 보냅니다.

같은 Tenant에서 같은 키와 같은 DataSet·버전·스키마·프로필 revision을 다시 보내면 기존 검증 실행과 Worker 작업으로 수렴하며 HTTP 202를 반환합니다. 다른 요청에 이미 사용한 키를 보내면 HTTP 409와 `dataset.validation.idempotency_conflict`를 반환합니다. 키는 200자를 초과할 수 없습니다.

## DataSet 메타데이터와 콘텐츠

DataSet API는 선택한 Tenant의 활성 Organization·Project에 소유 범위를 고정합니다. 생성·스키마 등록·콘텐츠 기록·활성화는 Tenant Administrator가 수행하고, 메타데이터·콘텐츠 조회와 저장량 조회는 활성 Tenant 구성원이 수행합니다. `tenantId` 경로, `X-Tenant-Id` 헤더와 활성 소속은 모든 요청에서 함께 확인합니다.

DataSet 초안 생성:

`POST /api/platform/tenants/{tenantId}/datasets`

```json
{
  "organizationId": "00000000-0000-0000-0000-000000000000",
  "projectId": "00000000-0000-0000-0000-000000000000",
  "name": "매출 데이터",
  "storageAdapter": "sqlite",
  "contentFormat": "json-array"
}
```

`organizationId`와 `projectId`는 생략할 수 없으며 선택한 Tenant에 속한 활성 범위여야 합니다. 현재 승인된 저장 엔진은 `sqlite`이고 콘텐츠 형식은 `csv` 또는 `json-array`입니다. DuckDB·Parquet 어댑터는 승인된 별도 포맷·복구·동시성 계약이 제공될 때까지 사용할 수 없습니다.

생성한 DataSet에 JSON Schema revision을 등록합니다.

`POST /api/platform/tenants/{tenantId}/datasets/{dataSetId}/schemas`

```json
{
  "schemaJson": "{\"type\":\"object\",\"properties\":{\"id\":{\"type\":\"string\"}}}"
}
```

스키마가 등록된 초안만 다음 경로에서 활성화할 수 있습니다.

`POST /api/platform/tenants/{tenantId}/datasets/{dataSetId}/activate`

콘텐츠는 요청 본문에 원문 스트림으로 기록하고 별도 JSON 래퍼를 사용하지 않습니다.

`POST /api/platform/tenants/{tenantId}/datasets/{dataSetId}/content`

현재 개발 기본 최대 크기는 64MiB이며, 한도 초과는 HTTP 413과 `dataset.content_too_large`, 보관된 DataSet 또는 quota 초과는 HTTP 409로 반환합니다. 콘텐츠는 전용 SQLite 콘텐츠 저장소에 기록되고 API 응답이나 로그에 내부 Storage Reference를 노출하지 않습니다.

다음 경로는 Tenant 범위 메타데이터·콘텐츠·저장량을 조회합니다.

- `GET /api/platform/tenants/{tenantId}/datasets/{dataSetId}`는 DataSet 상태, 버전과 스키마 revision을 반환합니다.
- `GET /api/platform/tenants/{tenantId}/datasets/{dataSetId}/content`는 콘텐츠를 `application/octet-stream` 스트림으로 반환합니다.
- `GET /api/platform/tenants/{tenantId}/datasets/{dataSetId}/usage`는 Tenant·Organization·Project·DataSet별 사용량, 한도와 경고를 반환합니다.
- `GET /api/platform/tenants/{tenantId}/storage-usage`는 Tenant 전체의 현재 사용량을 반환합니다.
- `GET /api/platform/tenants/{tenantId}/storage-usage/history?take=168`은 저장량 스냅샷을 반환합니다. `take`는 1~744 범위이며 기본값은 168입니다. 범위를 벗어나면 HTTP 400과 `api.invalid_request`를 반환합니다.
- `GET /api/platform/tenants/{tenantId}/storage-usage/quota-events?take=100`은 quota 임계치 이벤트를 Tenant 관리자에게 반환합니다. `take`는 1~500 범위이며 기본값은 100입니다. 범위를 벗어나면 HTTP 400과 `api.invalid_request`를 반환합니다.

## DataSet Validation 프로필과 결과

Tenant 관리자는 검증 프로필을 생성하고 불변 revision을 추가합니다.

- `POST /api/platform/tenants/{tenantId}/datasets/validation-profiles`
- `POST /api/platform/tenants/{tenantId}/datasets/validation-profiles/{profileId}/revisions`

프로필의 최상위 객체는 `fields` 배열을 포함해야 하며, 필드 형식은 `string`, `integer`, `decimal`, `boolean`입니다. `duplicateSets`와 `limits`는 승인된 v1 규칙만 사용합니다. 원문 셀 값과 내부 저장 경로는 결과에 기록하지 않습니다.

검증 실행 예약 요청은 다음과 같습니다.

```json
{
  "expectedDataSetVersion": 2,
  "schemaVersion": 1,
  "profileId": "00000000-0000-0000-0000-000000000000",
  "profileRevision": 1
}
```

`POST /api/platform/tenants/{tenantId}/datasets/{dataSetId}/validations`에 `Idempotency-Key`를 함께 보내면 HTTP 202와 검증 실행·Worker 작업 식별자를 반환합니다. 실행 상태는 `GET /api/platform/tenants/{tenantId}/datasets/validations/{executionId}`, 상세 항목은 `GET /api/platform/tenants/{tenantId}/datasets/validations/{executionId}/details?skip=0&take=100`으로 조회합니다. 결과 삭제는 `DELETE /api/platform/tenants/{tenantId}/datasets/validations/{executionId}`로 수행하며 실행 중인 결과는 HTTP 409로 거부합니다.

## Worker 작업과 장애 복구

Tenant 관리자는 등록된 작업 유형만 불투명 Payload Reference로 예약할 수 있습니다. 요청 본문에 작업 Payload 자체를 넣지 않고 `Idempotency-Key`를 반드시 사용합니다.

```json
{
  "taskType": "dataset.validation.v1",
  "payloadReference": "dataset-validation:v1:00000000000000000000000000000000",
  "maxAttempts": 5
}
```

`POST /api/platform/tenants/{tenantId}/worker/tasks`는 새 작업이면 HTTP 201, 동일 Tenant·Idempotency Key로 같은 내용을 재요청하면 HTTP 200, 다른 내용을 재사용하면 HTTP 409를 반환합니다. `GET /api/platform/tenants/{tenantId}/worker/tasks/{taskId}`로 작업 상태를 조회합니다. 작업은 Lease·Fencing Token·체크포인트를 사용하고, 만료 Lease는 재시도 또는 DLQ 경로로 복구합니다.

DLQ 작업은 다음 관리자 API로 조회·재처리합니다.

- `GET /api/platform/tenants/{tenantId}/worker/dead-letters?page=1&pageSize=50`
- `POST /api/platform/tenants/{tenantId}/worker/dead-letters/{taskId}/replay`

재처리 요청에는 새로운 GUID 형식의 `Replay-Request-Id` 헤더가 필요합니다. 같은 재처리 식별자를 반복 사용해도 동일한 재처리 작업으로 수렴하며, 원본 작업과 Tenant 범위는 유지됩니다.

## 외부 전달

Tenant 관리자는 허용 목록에 등록된 대상 URI로 외부 전달을 예약할 수 있습니다. 요청 본문은 저장되지만 상태 응답에는 Payload를 반환하지 않으며, `Idempotency-Key`는 Tenant 범위에서 전달 저장과 Worker 작업에 함께 적용됩니다.

`POST /api/platform/tenants/{tenantId}/worker/deliveries`

```json
{
  "targetUri": "https://receiver.example.test/events",
  "payload": { "event": "dataset.updated" },
  "maxAttempts": 5
}
```

전달 상태는 구성원도 `GET /api/platform/tenants/{tenantId}/worker/deliveries/{deliveryId}`로 조회할 수 있습니다. 대상 Host가 허용되지 않거나 loopback 허용 설정이 없으면 예약이 거부됩니다. 응답 유실·5xx·timeout은 재시도 상태로 남기고, 동일 Tenant·Idempotency Key의 동시 생성은 하나의 커밋된 전달과 작업으로 수렴합니다.

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

## 저장 용량 정책

Tenant Administrator는 Tenant·Organization·Project·DataSet 범위의 저장 용량 정책을 관리할 수 있습니다. 정책의 `targetId`는 선택한 Tenant에 속해야 하며 Tenant 범위 정책은 경로의 Tenant 식별자와 같아야 합니다. 하위 범위의 한도는 상위 범위의 한도를 완화하지 않습니다.

정책 목록 조회:

`GET /api/platform/tenants/{tenantId}/storage-quota-policies?page=1&pageSize=50`

정책 생성:

`POST /api/platform/tenants/{tenantId}/storage-quota-policies`

```json
{
  "scope": "Tenant",
  "targetId": "00000000-0000-0000-0000-000000000000",
  "tenantLimitBytes": 1073741824,
  "organizationLimitBytes": null,
  "projectLimitBytes": null,
  "dataSetLimitBytes": null,
  "warningThresholdPercent": 80,
  "planCode": "Community"
}
```

정책 수정은 응답의 `version`을 `expectedVersion`으로 보내야 합니다.

`PUT /api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}`

활성 상태 변경은 다음 경로에서 동일하게 `expectedVersion`을 사용합니다.

`PATCH /api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}/enabled`

`GET /api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}/history?page=1&pageSize=50`은 생성·수정·활성 상태 변경의 전후 JSON과 수행 주체를 반환합니다. 정책과 이력은 항상 요청 Tenant 조건으로 조회하며, `pageSize`는 1~100으로 제한합니다. 대상이 없으면 404, 중복 정책은 `quota_policy.conflict`와 409, 오래된 `expectedVersion`은 `quota_policy.version_conflict`와 409를 반환합니다.

## 필수 설정

다음 설정은 환경 변수 또는 비밀 저장소로 제공해야 합니다. 서명 키를 저장소에 커밋하지 마십시오.

- `Authentication__Mode`: `External` 또는 `Bypass`를 선택합니다. 기본값은 `External`입니다.
- `Authentication__Issuer`: 신뢰하는 인증 발급자
- `Authentication__Audience`: EDAN Tensor API 대상 식별자
- `Authentication__SigningKey`: UTF-8 기준 32바이트 이상의 비밀 서명 키
- `Authentication__MetadataAddress`: 개발·자동 검증용 OpenID Connect 메타데이터 주소입니다. 운영에서는 HTTPS 주소를 사용합니다.
- `Authentication__RequireHttpsMetadata`: 로컬 HTTP IdP를 사용할 때만 `false`로 지정합니다. 기본값은 `true`입니다.

`MetadataAddress`를 지정하면 JWKS 기반 비대칭 서명을 사용하며 `Issuer`와 `Audience` 검증은 계속 적용됩니다. 설정이 없거나 유효하지 않으면 JWT 검증이 성공하지 않으므로 보호 API에 접근할 수 없습니다. 상태 확인 경로는 유지됩니다. 운영 환경에서는 전용 비밀 저장소와 키 교체 절차를 사용해야 합니다.

### 개인 개발용 Bypass 모드

개인 PC에서 도메인이나 외부 IdP를 아직 준비하지 못한 경우 개발 환경에서만 Bypass 모드를 사용할 수 있습니다. 저장소의 `appsettings.Development.json`은 이 모드를 명시적으로 선택하며, 고정된 `sub`와 기본 `platform_admin` 역할을 가진 로컬 개발 주체를 사용합니다. Bypass에서는 Metadata/JWKS, Issuer, Audience와 키 회전 설정을 입력하지 않아도 되며, 해당 값이 있더라도 외부 인증 검증에 사용하지 않습니다.

Bypass 프로필의 Metadata 주소, JWKS 주소, Issuer, Audience, 테스트 Tenant, 테스트 사용자, 안정적인 `sub`와 키 회전 권한 식별자는 코드에 고정되어 설정값으로 변경할 수 없습니다. 현재 고정값은 `https://auth-test.example.com/.well-known/openid-configuration`, `https://auth-test.example.com/jwks`, `https://auth-test.example.com`, `edan-tensor-api`, `EDAN-NONPROD`, `edan-nonprod-test-user`, `edan-nonprod-test-subject`, `nonprod-idp-test-key-rotation`입니다. 이 값은 실제 외부 IdP의 Metadata·JWKS·Issuer·Audience·권한을 검증했다는 뜻이 아니라, 외부 IdP가 없는 Development 테스트에서 사용할 시뮬레이션 식별자입니다.

```text
Authentication__Mode=Bypass
Authentication__BypassRole=platform_admin
```

`Authentication__BypassRole`은 Development 테스트 Claim의 역할만 선택하며 고정 Tenant·사용자·`sub`·외부 IdP 시뮬레이션 프로필을 변경하지 않습니다. `EDAN-NONPROD` Tenant와 `edan-nonprod-test-subject`의 Administrator 소속은 Development 초기화가 멱등적으로 준비합니다.

Bypass는 외부 JWT 검증만 생략하고 Tenant 소속·조직 범위·관리자 권한 검사는 생략하지 않습니다. Bypass 주체를 Tenant Administrator로 등록해야 Tenant 업무 API를 사용할 수 있습니다. 운영 또는 비개발 환경에서 Bypass를 선택하면 API가 시작되지 않습니다.

개발 연속성 정책에 따라 실제 비운영 IdP의 Metadata·JWKS·Issuer·Audience·테스트 Tenant·opaque `sub`·키 회전 권한이 아직 없더라도 개인 개발을 중지하지 않습니다. Development 환경에서는 이 Bypass 설정과 로컬 IdP/JWKS 테스트를 사용하고, 외부 경계 검증만 미완료로 기록합니다. GitHub main 보호 규칙 API의 HTTP 403도 저장소 요금제에 따른 형상 관리 제약으로 취급하며 제품 개발이나 자체 호스팅 CI를 중지하는 원인으로 사용하지 않습니다.

이 연속성 정책은 `External` 또는 운영 환경의 인증 우회를 허용하지 않습니다. 해당 환경에서는 실제 Metadata·Issuer·Audience·JWKS·HTTPS 검증을 계속 적용하고, 외부 정보가 없으면 보호 API를 닫힌 상태로 유지합니다.

### 비운영 외부 IdP 권장 설정

유지보수자는 외부 인증 경계를 시험할 때 운영 IdP 대신 API와 분리된 `Edan.Identity` 비운영 서비스를 사용해야 합니다. 실제 소유 도메인이 `example.com`인 경우 권장 환경 변수는 다음과 같습니다.

```text
Authentication__MetadataAddress=https://auth-test.example.com/.well-known/openid-configuration
Authentication__Issuer=https://auth-test.example.com
Authentication__Audience=edan-tensor-api
Authentication__RequireHttpsMetadata=true
```

`example.com`은 설명용 값이므로 실제 소유 도메인으로 바꿉니다. JWKS 주소는 Metadata 응답의 `jwks_uri`를 사용하며, RS256 비대칭 키와 기존 키·새 키의 겹침 회전을 유지합니다. 테스트 Tenant는 `edan-nonprod`처럼 운영과 분리하고, 테스트 사용자의 이메일이 아니라 안정적인 `sub` 값을 Tenant Member 또는 Administrator 멤버십에 등록합니다. 개인키, 클라이언트 비밀값, 비밀번호와 액세스 토큰은 환경 변수 또는 비밀 저장소로만 주입하고 저장소·문서·로그에는 기록하지 않습니다.

개인 개발에서 별도 장치의 IdP를 연결할 때는 `MetadataAddress`와 `Issuer`를 `http://192.168.0.10:5001`처럼 IP 주소가 포함된 실제 주소로 바꿀 수 있습니다. 이때도 `Authentication__Mode=External`을 사용하고, HTTP Metadata는 Development에서만 허용합니다. 운영 전환 시에는 인증서가 있는 HTTPS 주소와 `Authentication__RequireHttpsMetadata=true`를 사용합니다.

## 현재 보안 경계와 후속 작업

현재 구현은 외부 주체의 다중 Tenant 소속 저장, 요청별 Tenant 선택, 데이터베이스 기반 활성 소속 확인, 로컬 JWKS 키 조회·회전 검증을 제공합니다. Tenant 내부의 Member와 Administrator는 허용된 구성원·읽기 API를 함께 사용할 수 있지만, Tenant 구조 변경·사용자 초대·소속 배정·정책 관리는 Administrator로 제한합니다. 사용자 초대 수락과 외부 계정 연결, 토큰 발급·갱신·폐기 및 실제 비운영 외부 IdP 경계 검증은 별도 운영 계약이 필요합니다.

완전한 운영 보안 검증 전까지 API의 외부 공개를 제한해야 합니다. 운영 환경에서는 배포 작업에서 승인된 EF Core Migration을 별도로 적용해야 합니다.


## 접근 정책 및 권한 감사

Tenant Administrator는 접근 정책을 등록할 수 있습니다.

POST /api/platform/tenants/{tenantId}/authorization-policies

```json
{
  "organizationId": null,
  "projectId": null,
  "subjectId": "identity-provider-user-123",
  "action": "resource:create",
  "resourceType": "DataSet",
  "effect": "Deny",
  "priority": 100
}
```

정책 효과는 Allow 또는 Deny이며, 자원 유형은 특정 유형 또는 *를 지정합니다. Tenant 정책은 모든 하위 범위에 적용되고 Organization 정책은 해당 조직과 프로젝트에, Project 정책은 지정 프로젝트에 적용됩니다. 명시적 Deny는 항상 우선하며, 정책에 일치하지 않으면 기존 역할 권한을 평가합니다. 활성 Tenant 소속은 필수이며 정책 Allow로 우회할 수 없습니다.

자원·정책 생성 권한 평가 결과는 platform_authorization_audit 테이블에 자동 기록됩니다. 감사 저장이 실패하면 권한 평가 요청도 성공 처리하지 않습니다. 상세 규칙은 [접근 정책 및 감사 안내](platform-authorization-policy-guide.md)를 참고하십시오.
