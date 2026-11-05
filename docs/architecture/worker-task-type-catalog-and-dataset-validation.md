# Worker 작업 유형 카탈로그 및 DataSet 검증 Handler 설계

## 목적

Worker 작업 등록 API가 실행할 수 없는 임의의 `TaskType`을 받아 영구 대기 작업을 만들지 않도록 한다. 동시에 DataSet 검증 작업은 아직 구현되지 않은 DataSet 저장·메타데이터 계약을 추측하지 않고 실제 모듈 계약에 연결한다.

## 현재 기준선

- Worker 작업은 별도 WorkerDbContext와 저장소에 보관한다.
- Handler는 WorkerHost 조합 지점에서 `AddWorkerTaskHandler<THandler>(taskType)`로 등록한다.
- API Host는 Worker 작업을 등록하지만 WorkerHost의 메모리 Handler Registry를 공유하지 않는다.
- 따라서 API가 현재 Handler 존재 여부를 확인할 방법이 없다.
- 저장소에는 DataSet 도메인·애플리케이션 프로젝트, 저장 어댑터, 스키마 계약 및 검증 서비스가 아직 없다. Platform의 `ResourceType=DataSet`은 자원 분류 메타데이터이며 실제 DataSet 저장 모델을 대체하지 않는다.

## 결정 1: 영속 작업 유형 카탈로그

Worker 전용 데이터베이스에 작업 유형 카탈로그를 둔다.

- 키: 정규화된 `TaskType` (최대 120자, ordinal 비교)
- 속성: 설명, 활성화 여부, 등록/갱신 시각, 낙관적 버전
- Handler 등록은 WorkerHost의 단일 조합 지점에서 선언한다.
- WorkerHost 시작 시 등록된 Handler 유형을 카탈로그에 upsert한다.
- 카탈로그에 없는 유형 또는 비활성 유형은 API Enqueue에서 거부한다.
- 중복 Handler 유형은 현재처럼 시작 시 구성 오류로 처리한다.
- WorkerHost가 등록하지 않은 카탈로그 항목을 자동 삭제하거나 비활성화하지 않는다. 롤링 배포 중 구버전 인스턴스가 신버전 작업 유형을 비활성화하는 일을 방지한다.
- Handler가 일시적으로 없는 작업은 Dispatcher가 계속 건너뛰므로, 운영에서는 카탈로그 비활성화와 대기 작업 처리 정책을 함께 제공해야 한다.
- 카탈로그 조회 장애 시 Enqueue는 fail-closed로 처리한다. 카탈로그 장애를 미등록 유형으로 오인해 작업을 생성하지 않는다.

### 데이터 경계

카탈로그와 WorkerTask는 WorkerDbContext에 둔다. PlatformDbContext와 Tenant 도메인에는 추가하지 않는다. 카탈로그는 전역 실행 기능 메타데이터이며 작업 데이터 자체는 기존과 같이 TenantId로 격리한다.

## 결정 2: DataSet Validation Handler의 선행 계약

실제 Handler 구현 전 다음 DataSet 계약이 저장소에 존재해야 한다.

1. DataSet 식별자와 Tenant·Organization·Project 소유 범위
2. 저장 위치를 직접 경로로 노출하지 않는 불투명한 Storage Reference
3. 버전이 있는 Schema/Format 계약과 DuckDB·Parquet·SQLite 등 어댑터 경계
4. 검증 프로필 및 규칙 버전
5. 검증 결과 요약, 오류/경고 코드, 상세 결과 저장 참조
6. 검증 실행 중 진행률·체크포인트·취소 및 Lease/Fencing Token 연계

Validation Handler의 입력은 사용자 제공 파일 경로나 임의 SQL이 아니라 Tenant 범위로 해석 가능한 DataSet ID, 기대 버전, 검증 프로필 참조로 제한한다. Handler는 저장소의 불투명 참조를 통해 승인된 어댑터를 호출하고, 결과는 Worker의 ResultReference에 저장 참조만 기록한다. API는 결과 존재 여부와 안전한 요약만 반환하고 내부 경로·연결 문자열·원본 페이로드를 노출하지 않는다.

## 구현 순서

1. WorkerTaskType 카탈로그 엔터티, 마이그레이션, 조회/등록 저장소 계약 구현
2. Handler Registry가 등록 유형을 노출하고 WorkerHost 시작 시 카탈로그와 동기화
3. API Enqueue가 활성 카탈로그 유형만 허용하도록 변경하고 미등록·비활성·카탈로그 장애 테스트 추가
4. DataSet Domain/Application/Infrastructure의 실제 저장 및 스키마 계약 확인·구현
5. 그 계약에 맞춘 DataSet Validation Handler와 진행률·결과·Tenant 경계 통합 테스트 구현

## 완료 기준

- API와 WorkerHost가 동일 Worker DB 카탈로그를 사용한다.
- 미등록/비활성 유형은 작업을 생성하지 않는다.
- 카탈로그 DB 장애 시 Enqueue는 실패 폐쇄한다.
- Handler 등록/카탈로그 동기화는 멱등적이며 롤링 배포에서 기존 유형을 임의 비활성화하지 않는다.
- DataSet Validation Handler는 실제 DataSet 어댑터 및 스키마 계약에 대해 통합 테스트를 통과한다.
