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

## 감사 무결성 체크섬

기간별 체크섬 생성은 다음 API를 사용합니다.

GET /api/platform/tenants/{tenantId}/authorization-audit/integrity?fromUtc=2026-11-01T00:00:00Z&toUtc=2026-11-02T00:00:00Z

- Tenant Administrator만 실행할 수 있습니다.
- 조회 기간은 최대 31일입니다.
- 결과 건수는 최대 100,000건으로 제한합니다.
- 응답의 sha256 값과 조회 기간, Tenant 식별자를 별도 보관 매체에 함께 저장합니다.

기준 체크섬 검증은 다음 API를 사용합니다.

POST /api/platform/tenants/{tenantId}/authorization-audit/integrity/verify

요청 본문:

```json
{
  "fromUtc": "2026-11-01T00:00:00Z",
  "toUtc": "2026-11-02T00:00:00Z",
  "expectedSha256": "외부에 보관한 64자리 SHA-256 값"
}
```

응답의 isValid가 true이면 현재 조회 결과가 기준 체크섬과 일치합니다. false이면 감사 데이터가 변경되었거나 기준값이 일치하지 않는 것이므로 원본 로그와 별도 보관본을 조사해야 합니다. 체크섬은 외부에 독립적으로 보관해야 하며, 동일 데이터베이스에만 보관하면 독립적인 변조 탐지 기준이 되지 않습니다.

현재 체크섬은 동일한 감사 데이터 집합에 대해 결정적으로 계산됩니다. 조회 범위, Tenant 식별자, 레코드 내용이 달라지면 체크섬도 달라집니다. 최대 건수 또는 기간 제한을 초과하는 경우 요청을 거부합니다.

## 보존 및 아카이빙 운영 원칙

- 감사 원본은 보존 정책, 법적 보존 요청 및 아카이브 검증 절차가 확정되기 전까지 자동 삭제하지 않습니다.
- 아카이브 작업은 원본과 별도 저장소에 체크섬 및 조회 범위 메타데이터를 함께 보관해야 합니다.
- 아카이브 검증이 완료되기 전에는 원본을 제거하지 않습니다.
- 보존 기간 만료와 실제 삭제는 별도 승인·감사 절차를 거쳐야 합니다.

## 현재 미지원 기능

역할 그룹 기반 주체, 복합 조건식, 시간·IP·장치 조건, 캐시 및 캐시 무효화, 외부 로그 전송은 후속 단계입니다. 운영 적용 전 감사 데이터 보존 기간과 외부 보관 정책을 확정해야 합니다.
