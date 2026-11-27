# 데이터베이스 기준 Provider 결정

## 결정

EDAN.Tensor는 배포 환경에 따라 두 가지 기준 Provider 프로파일을 제공한다.

- Online 운영: **PostgreSQL**
- OfflineSingleCustomer(폐쇄망·고객사 전용 단일 설치): **SQLite**

- API Host와 WorkerHost의 Production 구성은 `Database:Provider=PostgreSQL`을 사용한다.
- Platform, Worker, DataSet 메타데이터는 PostgreSQL Migration assembly를 사용한다.
- `DataSetContentDatabase`는 현재 콘텐츠 저장소 구현이 SQLite 전용이므로 PostgreSQL 기준 Provider 확정 범위와 분리하여 SQLite를 유지한다.
- SQLite는 폐쇄망·단일 고객사 운영과 로컬 개발·회귀 테스트용 호환 Provider로 유지한다.
- MSSQL과 Oracle Provider는 PostgreSQL 운영 안정화와 공통 모델 확정 이후 별도 프로젝트로 진행한다.

## 운영 설정 원칙

Production 설정에는 Provider만 선언하고 연결 문자열과 비밀번호는 환경 변수 또는 비밀 저장소로 주입한다. Compose 기본값도 PostgreSQL로 고정하여 운영 환경변수 누락 시 SQLite로 잘못 기동되지 않게 하며, SQLite는 폐쇄망·독립형 프로파일에서 `Database__Provider=Sqlite`를 명시적으로 선택한다.

필수 환경 변수:

- `Database__Provider=PostgreSQL`
- `ConnectionStrings__PlatformDatabase`
- `ConnectionStrings__WorkerDatabase`
- `ConnectionStrings__DataSetDatabase`
- `ConnectionStrings__DataSetContentDatabase` (현재 SQLite 콘텐츠 저장소)

운영에서는 TLS를 활성화하고 Migration은 배포 단계에서 별도로 적용한다. 애플리케이션 시작 시 자동 Migration은 운영 설정에서 사용하지 않는다.

## 확정 근거

- PostgreSQL Platform/Worker/DataSet Migration 생성 및 외부 DB 적용 완료
- 세 컨텍스트의 Migration History와 테이블 생성 확인 완료
- API PostgreSQL 연결·인증 검증 완료
- WorkerHost `/health` 및 `/health/database` HTTP 200 확인 완료
- SQLite Provider 모델 변경 대기 없음 및 Release 빌드 성공 확인 완료

## 배포별 운영 원칙

- Online 프로파일은 PostgreSQL TLS 연결과 별도 Migration 적용을 사용한다.
- OfflineSingleCustomer 프로파일은 API와 Worker가 동일한 SQLite 파일 세트를 사용하고 `App_Data`를 영속 볼륨에 둔다.
- 폐쇄망이라고 해서 인증을 자동으로 우회하지 않는다. 내부 IdP가 있으면 External 인증을 사용하고, Bypass는 개발 전용으로 제한한다.
- SQLite에서 PostgreSQL로 전환할 때는 파일 백업, 데이터 이전, Provider별 Migration, Health 검증을 순서대로 수행한다.

## 향후 Provider 추가 순서

PostgreSQL을 기준으로 운영 안정화한 후 MSSQL, Oracle 순서로 추가한다. 각 Provider는 별도 Migration, 자료형 매핑, 예약어·식별자, 인덱스, 날짜·UUID·JSON 동작과 API/Worker 외부 DB 검증을 통과해야 한다.
