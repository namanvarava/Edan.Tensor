# SQLite 데이터베이스 백업·복구 운영 런북

## 문서 상태와 적용 범위

이 런북은 현재 개발·검증 환경의 SQLite 데이터베이스를 안전하게 백업하고 별도 경로로 복구하는 절차를 정의합니다. 운영 DB 공급자, 백업 보존 기간, RPO·RTO와 외부 Object Storage는 아직 결정되지 않았으므로 이 문서만으로 운영 백업 완료를 판정하지 않습니다.

API Host와 Worker Host는 다음 네 개의 SQLite 저장소를 사용합니다.

| 저장소 | 기본 파일 | 포함 데이터 |
| --- | --- | --- |
| Platform | `App_Data/edan-tensor-platform.db` | Tenant, 소속, 조직·프로젝트, 정책과 감사 |
| Worker | `App_Data/edan-tensor-worker.db` | 작업, Lease, Fencing, 재시도와 DLQ |
| DataSet 메타데이터 | `App_Data/edan-tensor-dataset.db` | DataSet, schema revision, 검증 프로필·실행 결과 |
| DataSet 콘텐츠 | `App_Data/edan-tensor-dataset-content.db` | 불투명 저장 참조에 연결된 콘텐츠 BLOB |

네 파일은 서로 다른 연결을 사용하지만 동일한 논리 시점의 제품 상태를 구성합니다. 일부 파일만 복구하면 메타데이터와 콘텐츠 또는 작업 상태가 서로 다른 시점을 가리킬 수 있습니다.

## 백업 전 조건

1. 백업 시각과 대상 파일의 절대 경로를 기록합니다.
2. API Host와 Worker Host의 신규 요청·작업 처리를 중지하고, 실행 중인 Worker 작업이 없거나 중지 시점과 복구 영향을 기록합니다.
3. 두 Host가 같은 네 개의 파일을 가리키는지 `ConnectionStrings` 설정으로 확인합니다. 상대 경로는 Host별 Content Root가 달라질 수 있으므로 운영에서는 사용하지 않습니다.
4. 각 파일의 `.db-wal`과 `.db-shm` 파일을 포함한 SQLite 상태를 확인합니다. 단순히 `.db` 파일 하나만 복사하여 WAL에 남은 커밋을 누락하지 않습니다.
5. 백업 경로는 원본 데이터베이스 디렉터리와 분리하고, 원본 파일을 덮어쓰지 않습니다. 백업 파일에는 서비스 계정 권한과 암호화·보존 정책을 적용합니다.

## 일관된 백업 절차

모든 쓰기를 중지한 뒤 각 데이터베이스에 같은 순서로 다음 검사를 수행합니다.

```sql
PRAGMA wal_checkpoint(TRUNCATE);
PRAGMA integrity_check;
```

`integrity_check` 결과가 `ok`가 아니면 백업을 성공으로 기록하지 않고 원인을 조사합니다. 검사 결과가 정상인 파일은 승인된 SQLite 도구에서 `VACUUM INTO` 또는 동일한 의미의 온라인 백업 API로 별도 백업 파일을 생성합니다.

```sql
VACUUM INTO 'D:/EDAN/backup/<timestamp>/edan-tensor-platform.db';
```

실제 경로는 백업 운영자가 결정하며, SQL 문자열에 비밀번호·토큰·개인키를 포함하지 않습니다. 네 파일을 모두 생성한 뒤 백업 디렉터리의 파일 목록·크기·해시·생성 시각을 별도 기록합니다. 백업 파일 생성 전에 원본을 삭제하거나 이동하지 않습니다.

## 복구 절차

1. 복구 대상 백업 세트가 네 파일을 모두 포함하고 해시·생성 시각 기록과 일치하는지 확인합니다.
2. API Host와 Worker Host를 중지하고, 복구 대상의 신규 데이터 디렉터리를 생성합니다. 기존 운영 또는 개발 파일을 직접 덮어쓰지 않습니다.
3. 백업 파일을 새 디렉터리에 복사한 뒤 네 파일 각각에 `PRAGMA integrity_check;`를 실행합니다.
4. 새 절대 경로를 `ConnectionStrings:PlatformDatabase`, `ConnectionStrings:WorkerDatabase`, `ConnectionStrings:DataSetDatabase`, `ConnectionStrings:DataSetContentDatabase`에 설정합니다.
5. 운영에서는 자동 Migration을 비활성화하고 승인된 배포 절차로 현재 Migration 상태를 확인합니다. 복구된 스키마를 임의로 최신화하거나 하위 버전으로 되돌리지 않습니다.
6. API Host를 먼저 기동하여 `/health`와 `/health/database`가 네 저장소를 모두 정상으로 보고하는지 확인합니다.
7. Swagger 또는 승인된 API 검증 절차로 복구한 Tenant, DataSet 메타데이터, 콘텐츠, Worker 작업 상태를 읽기 전용으로 확인합니다. 다른 Tenant의 데이터가 노출되지 않는지도 확인합니다.
8. Worker Host를 기동하고 만료 Lease 복구, Fencing Token 증가, DLQ와 체크포인트 정합성을 확인합니다. 복구 전 시점의 외부 작업을 자동으로 재전송하기 전에 Idempotency Key와 수신 시스템 상태를 확인합니다.
9. 검증이 끝난 뒤에만 트래픽과 작업 처리를 재개하고, 복구 시각·사용한 백업 세트·검증 결과·미복구 항목을 기록합니다.

## 장애와 판정 기준

- 네 파일 중 하나라도 누락되면 전체 복구는 실패이며 부분 복구를 성공으로 기록하지 않습니다.
- `integrity_check` 실패, Migration 불일치, Tenant 격리 실패, 콘텐츠 참조 불일치, Worker Lease 정합성 실패가 발생하면 서비스를 공개하지 않고 복구 세트를 교체하거나 원인을 조사합니다.
- 복구 후 외부 전달 작업은 요청 수락 여부가 저장되기 전에 프로세스가 종료됐을 가능성을 고려합니다. 동일 Idempotency Key를 유지하고 수신 시스템의 중복 처리 계약 없이 강제 재전송하지 않습니다.
- 실제 운영 DB 백업·복구 시간, 장애 시점 데이터 손실량과 복구 시간은 운영 DB 공급자와 인프라가 결정된 뒤 별도 측정합니다. 현재 로컬 SQLite 검증은 운영 백업 성공의 증거가 아닙니다.

## 검증 기록

이 런북을 실제 실행할 때는 다음 증거를 남깁니다.

- 대상 환경과 네 데이터베이스의 절대 경로
- 백업·복구 시작 및 종료 시각
- 백업 세트 파일 목록과 해시
- 각 파일의 `integrity_check` 결과
- Migration 상태와 `/health`, `/health/database` 응답
- Tenant·DataSet 콘텐츠·Worker Lease/DLQ 확인 결과
- 외부 전달 재처리 여부와 Idempotency 판단
- 실패 원인, 영향 범위, 복구 대안과 재발 방지 조치
