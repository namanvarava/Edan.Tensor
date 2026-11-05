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

## 보존 및 삭제 원칙

- 아카이브 생성은 원본을 삭제하지 않습니다.
- 검증 실패 시 아카이브를 정상 보관본으로 취급하지 않습니다.
- 외부 기준 체크섬은 원본 데이터베이스 및 아카이브 저장 경로와 분리해 보관합니다.
- 현재 원본 삭제 API와 자동 보존 만료 작업은 제공하지 않습니다.
- 파일 시스템 저장 경로는 `AuthorizationAuditArchive:RootPath`로 설정합니다. 기본값은 API Host의 `App_Data/AuthorizationAuditArchives`입니다.
