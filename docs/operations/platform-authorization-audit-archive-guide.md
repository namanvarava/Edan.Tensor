# 권한 감사 아카이브 운영 안내

## 아카이브 생성

`POST /api/platform/tenants/{tenantId}/authorization-audit/archives`

요청 본문:

```json
{
  "fromUtc": "2026-11-01T00:00:00Z",
  "toUtc": "2026-11-02T00:00:00Z"
}
```

응답의 `archiveId`, `tenantId`, `fromUtc`, `toUtc`, `recordCount`, `sha256` 및 `sizeBytes`를 별도 보관 매체에 기록합니다. 기간은 최대 31일, 레코드는 최대 100,000건입니다.

## 아카이브 검증

`POST /api/platform/tenants/{tenantId}/authorization-audit/archives/{archiveId}/verify`

요청 본문:

```json
{
  "expectedSha256": "외부에 별도 보관한 64자리 SHA-256 값"
}
```

외부 기준값을 전달하면 아카이브 내부 체크섬과 외부 기준 체크섬을 모두 비교합니다. 기준값을 생략하면 내부 체크섬만 확인합니다.

## 저장 경로

기본 파일 시스템 경로는 API Host의 `App_Data/AuthorizationAuditArchives`입니다. 운영 환경에서는 `AuthorizationAuditArchive:RootPath` 설정을 통해 접근 제어와 백업 정책이 적용된 전용 경로를 지정합니다. 여러 API 인스턴스가 실행되는 환경에서는 모든 인스턴스가 동일하게 접근할 수 있는 내구성 있는 공유 저장소 Adapter가 필요합니다.

## 보존 및 삭제 원칙

- 아카이브 생성은 원본을 삭제하지 않습니다.
- 검증 실패 시 아카이브를 정상 보관본으로 취급하지 않습니다.
- 외부 기준 체크섬은 원본 데이터베이스 및 아카이브 저장 경로와 분리해 보관합니다.
- 현재 원본 삭제 API와 자동 보존 만료 작업은 제공하지 않습니다.
- 파일 시스템 저장 경로는 `AuthorizationAuditArchive:RootPath`로 설정합니다. 기본값은 API Host의 `App_Data/AuthorizationAuditArchives`입니다.

## Tenant별 감사 보존 정책

정책 조회:

`GET /api/platform/tenants/{tenantId}/authorization-audit/retention`

정책 변경:

`PUT /api/platform/tenants/{tenantId}/authorization-audit/retention`

요청 본문 예시:

```json
{
  "retentionDays": 730,
  "isLegalHold": true,
  "legalHoldReason": "보안 사고 조사 보존"
}
```

- 보존 기간은 30~3,650일로 제한합니다. 미설정 Tenant의 기본 정책은 365일입니다.
- 법적 보존 활성화 시 사유를 반드시 입력해야 합니다.
- 정책 변경은 Tenant 관리자만 수행할 수 있으며 변경 전·후 상태와 변경 주체가 이력으로 저장됩니다.
- 법적 보존 해제도 동일한 변경 이력으로 기록됩니다.
- 현재 정책은 보존 기준만 정의하며 감사 기록 삭제·정리 작업을 자동 실행하지 않습니다.

## 아카이브 작업 관리

아카이브 작업 생성:

`POST /api/platform/tenants/{tenantId}/authorization-audit/archive-jobs`

```json
{
  "fromUtc": "2026-11-01T00:00:00Z",
  "toUtc": "2026-11-02T00:00:00Z"
}
```

작업과 실행 이력 조회:

`GET /api/platform/tenants/{tenantId}/authorization-audit/archive-jobs/{jobId}`

실패 작업 재시도:

`POST /api/platform/tenants/{tenantId}/authorization-audit/archive-jobs/{jobId}/retry`

아카이브 검증:

`POST /api/platform/tenants/{tenantId}/authorization-audit/archive-jobs/{jobId}/verify`

- 작업 상태는 Pending, Completed, Verified, Failed로 관리합니다.
- 생성 실패 작업만 재시도할 수 있습니다.
- 각 생성·재시도·검증 실행은 시작 시각, 완료 시각, 성공 여부, 결과 코드로 기록합니다.
- 예약 실행은 아직 제공하지 않으며 Tenant 관리자가 API로 작업을 시작합니다.
- 원본 감사 기록은 이 작업에서 삭제되지 않습니다.
