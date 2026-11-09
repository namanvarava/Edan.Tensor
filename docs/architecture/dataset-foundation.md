# DataSet 기반 도메인 및 응용 계층

## 확정된 기반 규칙

- DataSet은 Tenant·Organization·Project 식별자를 모두 보유하며 Platform 도메인 객체를 참조하지 않습니다.
- DataSet 메타데이터와 실제 데이터 파일의 수명 주기는 분리합니다.
- 저장 위치는 물리 경로나 연결 문자열이 아닌 불투명한 Storage Reference로만 표현합니다.
- 저장소 엔진은 `StorageAdapter` 식별자로 선택하며 Domain은 DuckDB·Parquet·SQLite 구현체에 의존하지 않습니다.
- Schema는 JSON 객체 문서로 검증하고 순차 버전으로 추가합니다.
- DataSet은 Draft에서 스키마를 등록한 후 Active가 되며, Active에서 Archived로 전환합니다.

## Application 계층

- 저장소 조회는 DataSet ID와 Tenant ID를 함께 요구하여 Tenant 범위가 누락된 조회 계약을 방지합니다.
- Tenant·Organization·Project의 실제 소속 검증은 IDataSetScopeValidator 포트에 위임합니다.
- IDataSetStorageAdapter는 논리 할당 요청만 받고 물리 경로·연결 문자열을 노출하지 않습니다.
- 저장소 할당 후 메타데이터 저장에 실패하면 ReleaseAsync로 보상 해제를 시도합니다.
- 실제 DB 구현, 트랜잭션형 Outbox, 어댑터별 삭제 재시도 정책은 Infrastructure 단계에서 구현합니다.

## 현재 구현 상태와 다음 결정

DataSet 전용 메타데이터 DbContext와 Migration, Tenant 조건 조회, 스키마 revision 영속화와 SQLite 콘텐츠 어댑터가 구현되어 있습니다. API Host는 메타데이터와 콘텐츠를 별도 SQLite 데이터베이스로 연결하고, 저장소 할당 후 메타데이터 저장이 실패하면 콘텐츠 할당을 보상 해제합니다.

현재 승인된 검증 v1 계약은 SQLite 콘텐츠 어댑터와 UTF-8 CSV·JSON 객체 배열만 포함합니다. DuckDB·Parquet 어댑터는 파일 포맷, 스키마 매핑, 용량 계산, 동시 쓰기, 복구와 Migration 호환성에 대한 별도 계약이 승인된 뒤 구현합니다. 계약이 없는 상태에서는 저장소 식별자만 허용하는 가짜 구현을 추가하지 않습니다.
