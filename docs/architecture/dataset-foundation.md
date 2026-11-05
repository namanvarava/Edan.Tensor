# DataSet 기반 도메인

## 첫 단계 범위

- DataSet은 Tenant·Organization·Project 식별자를 모두 보유하며, Platform 도메인 객체를 참조하지 않습니다.
- DataSet 메타데이터와 실제 데이터 파일의 수명 주기는 분리합니다.
- 저장 위치는 물리 경로나 연결 문자열이 아닌 불투명한 Storage Reference로만 표현합니다.
- 저장소 엔진은 `StorageAdapter` 식별자로 선택하며 Domain은 DuckDB·Parquet·SQLite 구현체에 의존하지 않습니다.
- Schema는 JSON 객체 문서로 검증하고 순차 버전으로 추가합니다. 기존 버전은 수정하지 않습니다.
- DataSet은 Draft에서 스키마를 등록한 후 Active가 되며, Active에서 Archived로 전환합니다.

## 다음 구현 단계

Application 계층에 저장소 어댑터 계약, DataSet 조회·등록 유스케이스, 소유 범위 검증 포트를 추가합니다. 이후 별도 DataSet 메타데이터 DbContext와 마이그레이션을 구성하고, 실제 어댑터를 연결한 다음 Worker 검증 Handler를 등록합니다.
