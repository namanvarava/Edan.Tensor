# Organization·Project 범위 권한 설계

## 목적

Tenant 소속만으로 모든 조직과 프로젝트에 접근하는 문제를 방지합니다. 조직과 프로젝트의 소속을 별도 레코드로 관리하고, 요청한 자원의 범위에 맞춰 생성 권한을 평가합니다.

## 권한 계층

| 범위 | 소속 모델 | 역할 | 현재 생성 권한 |
|---|---|---|---|
| Tenant | TenantMembership | Member | 범위 자원 생성 불가 |
| Tenant | TenantMembership | Administrator | Tenant·조직·프로젝트 전체 생성 |
| Organization | OrganizationMembership | Reader | 생성 불가 |
| Organization | OrganizationMembership | Contributor | 조직 범위 생성 |
| Organization | OrganizationMembership | Manager | 조직 및 하위 프로젝트 범위 생성 |
| Project | ProjectMembership | Reader | 생성 불가 |
| Project | ProjectMembership | Contributor | 해당 프로젝트 범위 생성 |
| Project | ProjectMembership | Manager | 해당 프로젝트 범위 생성 |

Tenant Administrator는 선택한 Tenant의 모든 하위 범위에 대한 권한을 가집니다. Organization Contributor는 조직 범위에만 적용되며 프로젝트 범위로 자동 상속되지 않습니다. Organization Manager만 하위 프로젝트에 권한을 상속합니다. 프로젝트 소속 역할은 해당 프로젝트에만 적용합니다.

## 데이터 모델

- `OrganizationMembership`: TenantId, OrganizationId, SubjectId, Role, Status
- `ProjectMembership`: TenantId, OrganizationId, ProjectId, SubjectId, Role, Status

각 소속은 활성·폐기 상태를 가지며 역할 변경과 폐기는 도메인 규칙으로 통제합니다. 동일 범위와 외부 주체의 중복 소속은 복합 고유 인덱스로 차단합니다.

## API

Tenant Administrator가 소속을 배정합니다.

- `POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/memberships`
- `POST /api/platform/tenants/{tenantId}/organizations/{organizationId}/projects/{projectId}/memberships`

요청 본문 예시:

```json
{
  "subjectId": "identity-provider-user-123",
  "role": "Contributor"
}
```

조직 역할은 `Reader`, `Contributor`, `Manager`이며 프로젝트 역할도 동일한 세 가지 값을 사용합니다.

## 권한 평가 흐름

1. JWT에서 외부 인증 주체 `sub`를 확인합니다.
2. `X-Tenant-Id`로 선택한 Tenant의 활성 소속을 확인합니다.
3. Tenant Administrator이면 요청 범위를 허용합니다.
4. 조직 범위이면 활성 OrganizationMembership의 Contributor 또는 Manager를 확인합니다.
5. 프로젝트 범위이면 활성 ProjectMembership의 Contributor 또는 Manager를 확인합니다.
6. 프로젝트 소속이 없으면 Organization Manager만 하위 프로젝트 권한을 상속합니다.
7. 범위 권한이 없으면 HTTP 403으로 거부합니다.

자원 및 정책 생성 API는 모두 동일한 범위 권한 필터를 통과해야 합니다. 도메인 및 Infrastructure 계층의 Tenant·Organization·Project 외래 키 검증은 별도로 유지합니다.

## 영속성

Migration `20261105120000_AddScopedMemberships`가 다음 테이블을 추가합니다.

- `platform_organization_memberships`
- `platform_project_memberships`

조직 소속은 (TenantId, OrganizationId, SubjectId), 프로젝트 소속은 (TenantId, OrganizationId, ProjectId, SubjectId) 조합을 고유하게 유지합니다. 외래 키는 부모 조직·프로젝트의 복합 소유권 키를 참조하여 Tenant 경계를 보호합니다.

## 현재 범위와 후속 과제

현재는 Tenant Administrator가 조직·프로젝트 소속을 배정하며, 생성 작업에 대한 Reader·Contributor·Manager 권한을 적용합니다. 역할 변경·폐기 API, 멤버가 멤버를 초대하는 위임 정책, 읽기·수정·삭제 작업별 권한, 정책 기반 동적 조건, 권한 변경 감사 이벤트는 후속 단계에서 추가합니다.
