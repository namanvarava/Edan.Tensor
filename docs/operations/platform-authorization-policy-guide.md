# 접근 정책 및 감사 API 안내

## 사전 조건

- JWT Bearer 인증이 필요합니다.
- 요청에 X-Tenant-Id를 지정해야 합니다.
- 선택 Tenant의 활성 Administrator 소속이 있어야 정책을 관리할 수 있습니다.
- 정책 대상 주체는 해당 Tenant의 활성 구성원이어야 권한을 사용할 수 있습니다.

## 정책 등록

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

## 정책 목록 조회

GET /api/platform/tenants/{tenantId}/authorization-policies

선택한 Tenant에 속한 정책을 생성 시각 역순으로 반환합니다. 비활성 정책도 함께 반환하여 관리자가 전체 정책 상태를 확인할 수 있습니다.

## 정책 효과 및 우선순위 변경

PUT /api/platform/tenants/{tenantId}/authorization-policies/{policyId}

```json
{
  "effect": "Deny",
  "priority": 200
}
```

- effect: Allow 또는 Deny
- priority: 0~10000

정책 식별자가 다른 Tenant에 속하거나 존재하지 않으면 404를 반환합니다. 입력 범위를 벗어난 우선순위는 400으로 거부합니다.

## 정책 활성 상태 변경

PATCH /api/platform/tenants/{tenantId}/authorization-policies/{policyId}/enabled

```json
{
  "isEnabled": false
}
```

비활성 정책은 권한 평가에 사용되지 않지만 목록 조회에는 표시됩니다. 삭제 대신 비활성화를 사용하여 정책의 관리 이력을 유지합니다.

## 정책 상속과 평가

- Tenant 정책은 Tenant 및 하위 범위 요청에 적용됩니다.
- Organization 정책은 해당 조직과 하위 프로젝트에 적용됩니다.
- Project 정책은 지정 프로젝트에 적용됩니다.
- 하위 범위 요청에서는 해당 범위에 적용되는 모든 상위 정책을 함께 평가합니다.
- 명시적 Deny는 역할 기반 권한과 Allow 정책보다 항상 우선합니다.
- Deny가 없으면 일치하는 Allow 정책을 적용합니다.
- 일치 정책이 없으면 기존 역할 권한을 적용하며, 역할 권한도 없으면 기본 거부합니다.
- 정책 Allow는 활성 Tenant 소속을 대체하지 않습니다.

## 정책 변경 감사

정책 수정과 활성 상태 변경은 다음 정보를 추가 전용 테이블에 기록합니다.

- TenantId 및 PolicyId
- ActorSubjectId: 변경을 요청한 인증 주체
- ChangeType: Updated, Enabled 또는 Disabled
- BeforeJson 및 AfterJson
- OccurredAtUtc

정책 변경과 감사 기록은 같은 저장 작업으로 확정합니다. 감사 기록 저장에 실패하면 정책 변경도 확정되지 않습니다. 정책 삭제 API는 제공하지 않습니다.

## 권한 평가 감사

자원 및 정책 생성 권한을 평가할 때마다 Tenant, SubjectId, 작업, 자원 유형, 평가 범위, 허용·거부 결과, 사유 코드, 일치 정책 식별자 및 UTC 시각을 기록합니다.

## 권한 평가 감사 조회

GET /api/platform/tenants/{tenantId}/authorization-audit

Tenant Administrator만 조회할 수 있습니다. X-Tenant-Id와 경로의 tenantId가 일치해야 하며, 다른 Tenant의 감사 기록은 조회할 수 없습니다.

지원 쿼리 매개변수:

| 매개변수 | 설명 |
|---|---|
| subjectId | 인증 주체 식별자 |
| action | 권한 평가 작업 |
| isAllowed | 허용 여부 (true 또는 false) |
| fromUtc | 조회 시작 UTC 시각 |
| toUtc | 조회 종료 UTC 시각 |
| page | 1부터 시작하는 페이지 번호 |
| pageSize | 페이지 크기, 1~100 (기본값 50) |

응답은 totalCount, page, pageSize, items를 포함합니다. 감사 기록은 평가 시각 내림차순으로 반환합니다. 조회 API는 읽기 전용이며 수정·삭제 기능을 제공하지 않습니다.

## 현재 미지원 기능

역할 그룹 기반 주체, 복합 조건식, 시간·IP·장치 조건, 캐시 및 캐시 무효화, 외부 로그 전송은 후속 단계입니다. 운영 적용 전 감사 데이터 보존 기간과 외부 보관 정책을 확정해야 합니다.
