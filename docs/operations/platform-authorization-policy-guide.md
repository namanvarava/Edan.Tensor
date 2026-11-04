# 접근 정책 및 감사 API 안내

## 사전 조건

- JWT Bearer 인증이 필요합니다.
- 요청에 X-Tenant-Id를 지정해야 합니다.
- 선택 Tenant의 활성 Administrator 소속이 있어야 정책을 등록할 수 있습니다.
- 정책 대상 주체는 해당 Tenant의 활성 구성원이어야 권한을 사용할 수 있습니다.

## 접근 정책 등록

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

| 필드 | 설명 |
|---|---|
| organizationId | 선택적 조직 범위 |
| projectId | 선택적 프로젝트 범위 |
| subjectId | 정책 대상 외부 인증 주체 |
| action | 보호할 작업 식별자 |
| resourceType | 구체적인 자원 유형 또는 * |
| effect | Allow 또는 Deny |
| priority | 0~10000, 동일 효과 정책의 정렬 기준 |

프로젝트 범위를 지정할 때 조직 범위도 지정해야 합니다. 상위 소유권은 서버에서 검증합니다.

## 정책 상속

- Tenant 정책은 Tenant 및 그 하위 범위 요청에 적용됩니다.
- Organization 정책은 해당 조직과 그 하위 프로젝트에 적용됩니다.
- Project 정책은 지정 프로젝트에 적용됩니다.
- 하위 범위 요청에서는 해당 범위에 적용되는 모든 상위 정책을 함께 평가합니다.

## 우선순위와 기본 권한

명시적 Deny는 역할 기반 권한과 Allow 정책보다 항상 우선합니다. Deny가 없으면 일치하는 Allow 정책을 적용합니다. 일치 정책이 없으면 기존 Tenant·Organization·Project 역할 권한을 적용합니다. 역할 권한도 없으면 기본 거부합니다.

정책 Allow는 활성 Tenant 소속을 대체하지 않습니다. Tenant 소속이 없거나 폐기된 주체는 정책과 관계없이 거부됩니다.

## 감사 기록

자원 및 정책 생성 권한을 평가할 때마다 Tenant, SubjectId, 작업, 자원 유형, 평가 범위, 허용·거부 결과, 사유 코드, 일치 정책 식별자 및 UTC 시각을 감사 테이블에 기록합니다.

감사 저장이 실패하면 권한 평가 요청은 성공으로 처리되지 않습니다. 현재 감사 조회 API는 제공하지 않으며, 데이터베이스 운영 권한으로 별도 조회해야 합니다.

## 현재 미지원 기능

정책 수정·삭제·비활성화 API, 역할 그룹 기반 주체, 복합 조건식, 시간·IP·장치 조건, 캐시 및 캐시 무효화는 후속 단계입니다. 운영 적용 전 감사 데이터 보존·접근 통제와 정책 변경 이력 관리 방안을 확정해야 합니다.
