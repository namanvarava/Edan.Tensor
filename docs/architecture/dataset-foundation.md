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

## 다음 구현 단계

DataSet 전용 메타데이터 DbContext와 Migration을 추가하고, Tenant 복합 필터·낙관적 동시성·스키마 revision 관계를 영속화합니다. 이후 DuckDB·Parquet·SQLite 어댑터를 별도 구현체로 연결합니다.
