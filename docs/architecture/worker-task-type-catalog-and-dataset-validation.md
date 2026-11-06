# Worker 작업 유형 카탈로그와 DataSet 검증 구현 기준

## 목적

Worker는 등록되지 않은 작업 유형을 영구 대기 상태로 만들지 않으며, DataSet 검증은 Tenant·DataSet·프로필 revision 경계를 유지하면서 Lease와 체크포인트를 사용합니다.

## 작업 유형 카탈로그

- Worker 전용 데이터베이스에 정규화된 `TaskType` 카탈로그를 보관합니다.
- WorkerHost의 단일 조합 지점이 Handler를 등록하고 시작 시 카탈로그를 멱등적으로 upsert합니다.
- API Enqueue는 활성 카탈로그 유형만 허용합니다. 카탈로그 조회 장애는 fail-closed로 처리합니다.
- WorkerHost가 등록하지 않은 기존 카탈로그 항목은 롤링 배포 중 자동 삭제·비활성화하지 않습니다.
- 작업 데이터와 실행 결과는 TenantId와 Lease/Fencing 조건을 함께 적용합니다.

## DataSet Validation v1

승인된 상세 계약은 [DataSet 검증 v1 계약](dataset-validation-v1-proposal.md)에 있습니다. 현재 구현은 다음 범위를 제공합니다.

- 기존 SQLite 콘텐츠 어댑터의 UTF-8 CSV와 JSON 객체 배열을 읽습니다.
- 프로필은 Tenant별 ID와 증가하는 revision으로 불변 관리합니다. 수정은 새 revision을 생성합니다.
- 필수값, 문자열·정수·소수·불리언 형식, 문자열 길이, 수치 범위 및 필드 조합 중복을 검사합니다.
- v1은 최상위 필드만 읽으며 중첩 경로·임의 코드·SQL을 허용하지 않습니다.
- 실행에는 DataSet 버전, Schema revision, 프로필 revision을 고정하고 진행 위치와 상세 결과 상한을 저장합니다.
- 결과는 원문 셀 값·연결 문자열·물리 경로를 기록하지 않습니다. 자동 삭제하지 않으며 Tenant 관리자만 명시적으로 삭제할 수 있습니다.
- `dataset.validation.v1` 작업은 불투명 실행 참조를 사용하고 Worker가 전용 DbContext와 DataSet 저장소를 통해 실행합니다.

## 장애와 보안 검증

단위·인프라·API 통합 테스트는 정상/잘못된 입력, Error·Warning 집계, UTF-8 오류, 최대 행 제한, Tenant 격리, 멱등 Enqueue, 실행 상태 및 명시적 삭제를 확인합니다. WorkerHost는 실제 Windows 프로세스로 기동하여 Health 경계를 확인합니다.

Lease 인계·취소 시 DataSet 검증 Handler는 자체 취소 토큰을 취소하고 실행 중인 저장 작업의 종료를 기다립니다. 오래된 실행이 공개 결과를 갱신하지 않도록 저장소의 Tenant·실행 상태·Fencing 조건을 유지합니다.

## 이번 릴리스에서 제외하는 항목

DuckDB·Parquet 어댑터, Commercial·Licensing 업무 흐름·결제·권리·만료, WPF 화면은 이번 릴리스의 승인 범위에 포함하지 않습니다. 해당 기능은 다음 릴리스의 계약과 수용 기준을 별도로 승인한 뒤 구현합니다.
