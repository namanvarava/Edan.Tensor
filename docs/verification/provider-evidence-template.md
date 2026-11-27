# Provider 운영 검증 증적 양식

이 문서는 VM에서 실제 검증한 결과를 Provider별로 기록하는 양식이다. 비밀번호·토큰·Client Secret은 기록하지 않는다.

## 공통 정보

| 항목 | 값 |
|---|---|
| 실행 시각(UTC) | |
| Git commit | |
| VM/OS | |
| Docker Compose 상태 | |
| API 이미지/버전 | |
| Worker 이미지/버전 | |

## Provider별 결과

| Provider | Database | Schema | BindingId | Migration 적용 | Health | Table Metadata | Routine Metadata | Consistency |
|---|---|---|---|---|---|---|---|---|
| SQLite | | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| PostgreSQL | | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| SQL Server | | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| Oracle | | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |

## 실제 스키마 대조

| Provider | 예상 테이블 수 | 실제 테이블 수 | PK 일치 | FK 일치 | Index 일치 | Migration History 일치 |
|---|---:|---:|---|---|---|---|
| SQLite | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| PostgreSQL | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| SQL Server | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| Oracle | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |

## 복수 DB DML·정합성

| 시나리오 | 결과 | 증적 경로 |
|---|---|---|
| 정상 복수 Provider DML | PASS/FAIL | |
| 중간 Provider 실패 | PASS/FAIL | |
| 자동 재시도 | PASS/FAIL | |
| 보상 처리 | PASS/FAIL | |
| 멱등성 재실행 | PASS/FAIL | |
| 정합성 불일치 감지 | PASS/FAIL | |

## 백업·복구

| Provider | 백업 파일/ID | 복구 시각 | 복구 후 Health | 복구 후 Metadata | 복구 후 Routine | 결과 |
|---|---|---|---|---|---|---|
| SQLite | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| PostgreSQL | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| SQL Server | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| Oracle | | | PASS/FAIL | PASS/FAIL | PASS/FAIL | PASS/FAIL |

## 병합 판정

- [ ] 모든 Provider 행이 PASS
- [ ] 복수 DB DML·재시도·보상·정합성 모두 PASS
- [ ] 백업 복구 후 검증 모두 PASS
- [ ] 보안·라이선스·Secret 점검 증적 저장
- [ ] CI와 코드 리뷰 승인 확인
- [ ] 위 항목을 확인하기 전에는 `main` 병합 및 브랜치 삭제 금지

최종 증적은 다음과 같은 JSON으로 저장한 뒤 게이트를 실행한다.

```json
{
  "migrations": "PASS",
  "schema": "PASS",
  "health": "PASS",
  "routines": "PASS",
  "consistency": "PASS",
  "backupRestore": "PASS",
  "securityLicense": "PASS"
}
```

```bash
pwsh -File ./scripts/verification/Assert-FinalMergeEvidence.ps1 \
  -EvidenceJson ./artifacts/provider-acceptance-evidence.json
```
