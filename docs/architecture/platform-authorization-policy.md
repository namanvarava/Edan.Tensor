# Policy 기반 권한 평가 및 감사 설계

## 목적

역할 기반 소속 권한 위에 명시적 접근 정책을 적용합니다. 요청의 Tenant·Organization·Project 범위, 인증 주체, 작업(Action), 자원 유형(Resource Type)을 함께 평가합니다.

## 정책 모델

AuthorizationPolicy는 다음 정보를 저장합니다.

- TenantId 및 선택적 OrganizationId·ProjectId
- SubjectId: 외부 인증 주체의 sub
- Action: resource:create, policy:create 등
- ResourceType: 구체적인 자원 유형 또는 *
- Effect: Allow 또는 Deny
- Priority: 0~10000
- IsEnabled 및 CreatedAtUtc

프로젝트 범위 정책에는 조직 범위가 반드시 필요합니다. 정책을 생성할 때 Tenant·Organization·Project의 실제 소유권을 검증합니다.

## 정책 적용 범위 상속

Tenant 요청에는 Tenant 정책을 적용합니다. Organization 요청에는 Tenant 및 해당 Organization 정책을 적용합니다. Project 요청에는 Tenant, 상위 Organization 및 해당 Project 정책을 함께 적용합니다.

## 평가 순서

1. 외부 인증 주체가 선택한 Tenant의 활성 구성원인지 확인합니다.
2. Tenant·Organization·Project 소속 역할에 따른 기본 권한을 평가합니다.
3. 요청 범위에 적용되는 활성 정책을 조회합니다.
4. 명시적 Deny가 있으면 역할 및 Allow 정책보다 우선하여 거부합니다.
5. Deny가 없고 일치하는 Allow가 있으면 허용합니다.
6. 일치하는 정책이 없으면 기존 역할 기반 권한을 적용합니다.
7. 역할 권한과 Allow 정책이 모두 없으면 기본 거부합니다.
8. 모든 평가 결과를 감사 테이블에 기록하고 저장합니다.

활성 Tenant 소속은 정책 Allow로 우회할 수 없는 필수 경계입니다. 소속이 없거나 폐기된 주체는 항상 거부합니다.

## API

Tenant Administrator만 접근 정책을 관리할 수 있습니다.

| Method | Endpoint | 설명 |
|---|---|---|
| POST | /api/platform/tenants/{tenantId}/authorization-policies | 정책 등록 |
| GET | /api/platform/tenants/{tenantId}/authorization-policies | Tenant 정책 목록 조회 |
| PUT | /api/platform/tenants/{tenantId}/authorization-policies/{policyId} | 효과와 우선순위 변경 |
| PATCH | /api/platform/tenants/{tenantId}/authorization-policies/{policyId}/enabled | 정책 활성 상태 변경 |
| GET | /api/platform/tenants/{tenantId}/authorization-audit | Tenant 권한 평가 감사 조회 |
| GET | /api/platform/tenants/{tenantId}/authorization-audit/integrity | 기간별 감사 체크섬 생성 |
| POST | /api/platform/tenants/{tenantId}/authorization-audit/integrity/verify | 외부 보관 체크섬 검증 |

정책 등록 예시:

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

정책 수정 예시:

```json
{
  "effect": "Deny",
  "priority": 200
}
```

활성 상태 변경 예시:

```json
{
  "isEnabled": false
}
```

## 감사 기록

AuthorizationAuditEntry는 Tenant, 외부 주체, Action, ResourceType, 평가 범위, 허용 여부, 사유 코드, 일치 정책 식별자 및 UTC 평가 시각을 저장합니다.

AuthorizationPolicyChangeAuditEntry는 Tenant, 정책 식별자, 변경자 SubjectId, 변경 유형, 변경 전 JSON, 변경 후 JSON 및 UTC 변경 시각을 추가 전용으로 보관합니다. 정책 변경과 감사 기록은 동일한 Unit of Work로 저장하므로 감사 저장이 실패하면 정책 변경도 확정되지 않습니다.

## 감사 조회 경계

권한 평가 감사 기록은 Tenant Administrator만 조회할 수 있습니다. 조회 조건은 SubjectId, Action, 허용 여부, UTC 기간이며 페이지 번호와 페이지 크기를 제한합니다. 페이지 크기는 최대 100건입니다. 쿼리는 항상 TenantId 조건으로 시작하므로 선택 Tenant 밖의 기록을 조회할 수 없습니다. 감사 기록은 읽기 전용으로 제공하며 수정·삭제 API는 제공하지 않습니다.

## 저장 구조

- platform_authorization_policies: 계층형 접근 정책
- platform_authorization_audit: 권한 평가 감사 기록
- platform_authorization_policy_changes: 정책 변경 전후 감사 기록

정책 테이블은 Tenant·Organization·Project의 복합 소유권 외래 키를 사용합니다. 변경 감사 테이블은 Tenant 경계로 보호하고 정책·시각 및 Tenant·시각 조회 인덱스를 제공합니다.

## 현재 범위와 한계

현재 정책은 외부 SubjectId에 직접 연결되며 정책 관리는 Tenant Administrator에게만 허용합니다. 역할 기반 정책 주체, 조건식 언어, 시간·네트워크·환경 속성 평가, 캐시 및 무효화, 외부 로그 전송은 아직 구현하지 않았습니다. 명시적 Deny는 우선순위와 무관하게 Allow보다 우선합니다.


## 감사 무결성 및 보존 경계

감사 기록의 무결성 API는 최대 31일, 최대 100,000건 범위의 Tenant 감사 데이터를 정규화해 SHA-256 체크섬을 계산합니다. 관리자는 체크섬을 별도 보관 매체에 저장하고 이후 검증 요청에 기준값으로 제출할 수 있습니다. 비교는 상수 시간 방식으로 수행합니다.

체크섬은 계산 시점의 데이터 상태를 식별하는 외부 기준값이며, 데이터베이스 내부에 기준값을 함께 저장하지 않습니다. 따라서 데이터와 체크섬을 서로 독립된 보관 경계에 유지해야 변조 탐지 효과를 얻을 수 있습니다. 현재 단계에서는 감사 데이터 자동 삭제, 보존 기간 만료 처리 및 외부 아카이브 저장소 연결을 수행하지 않습니다. 감사 증거의 무단 삭제를 방지하기 위해 보존 정책과 아카이브 승인 절차가 확정되기 전까지 원본 감사 기록을 유지합니다.
