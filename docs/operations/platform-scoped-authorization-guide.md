# 조직·프로젝트 권한 API 사용 안내

## 사전 조건

- JWT Bearer 인증이 완료되어야 합니다.
- `sub` 클레임에 외부 인증 주체 식별자가 있어야 합니다.
- 요청에 `X-Tenant-Id`를 지정해야 합니다.
- 선택한 Tenant에 활성 소속이 있어야 합니다.
- 조직·프로젝트 소속을 배정하려면 Tenant Administrator 역할이 필요합니다.

## 조직 구성원 등록

`POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/memberships`

```json
{
  "subjectId": "identity-provider-user-123",
  "role": "Contributor"
}
```

지원 역할:
- `Reader`: 읽기 전용 역할이며 현재 자원·정책 생성은 허용하지 않습니다.
- `Contributor`: 해당 조직 범위의 자원·정책 생성을 허용합니다.
- `Manager`: 조직 범위와 하위 프로젝트 범위의 생성을 허용합니다.

## 프로젝트 구성원 등록

`POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/projects/{projectId}/memberships`

```json
{
  "subjectId": "identity-provider-user-123",
  "role": "Reader"
}
```

프로젝트 역할은 `Reader`, `Contributor`, `Manager`입니다. Contributor와 Manager는 해당 프로젝트 범위의 자원·정책 생성을 허용합니다.

## 권한 상속

- Tenant Administrator는 선택 Tenant 내 모든 조직·프로젝트 범위에서 생성할 수 있습니다.
- Organization Contributor는 조직 범위에서만 생성할 수 있습니다.
- Organization Manager는 조직과 하위 프로젝트에서 생성할 수 있습니다.
- Project Contributor와 Manager는 지정 프로젝트에서만 생성할 수 있습니다.
- Project Reader는 생성할 수 없습니다.
- Organization Contributor는 프로젝트 권한을 자동으로 상속하지 않습니다.

## 오류

- 400: 지원하지 않는 역할 또는 요청 값 오류
- 403: Tenant 관리자 권한 또는 대상 범위 권한 부족
- 409: 상위 조직·프로젝트 소속 불일치 또는 중복 등록

## 보안 유의사항

현재 소속 배정은 Tenant Administrator만 수행할 수 있습니다. 역할 변경·폐기 API 및 조회·수정·삭제 권한은 아직 제공하지 않습니다. 운영 배포 전에는 실제 Identity Provider, 감사 이벤트 및 세부 권한 테스트를 추가로 검증해야 합니다.
