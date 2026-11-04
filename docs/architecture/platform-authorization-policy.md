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

Tenant Administrator만 다음 API로 정책을 등록할 수 있습니다.

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

## 감사 기록

AuthorizationAuditEntry는 Tenant, 외부 주체, Action, ResourceType, 평가 범위, 허용 여부, 사유 코드, 일치 정책 식별자 및 UTC 평가 시각을 저장합니다.

감사 저장에 실패하면 요청도 성공 처리하지 않습니다. 운영 환경에서는 보존 기간, 접근 통제, 무결성 보호 및 외부 로그 전송 정책이 추가로 필요합니다.

## 저장 구조

- platform_authorization_policies: 계층형 접근 정책
- platform_authorization_audit: 권한 평가 감사 기록

정책 테이블은 Tenant·Organization·Project의 복합 소유권 외래 키를 사용합니다.

## 현재 범위와 한계

현재 정책은 외부 SubjectId에 직접 연결되며 정책 등록은 Tenant Administrator에게만 허용합니다. 정책 수정·비활성화 API, 역할 기반 정책 주체, 조건식 언어, 시간·네트워크·환경 속성 평가, 캐시 및 무효화는 아직 구현하지 않았습니다. 명시적 Deny는 우선순위와 무관하게 Allow보다 우선합니다.
