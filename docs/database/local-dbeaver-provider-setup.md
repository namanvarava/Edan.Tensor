# 로컬 Provider 통합 DB와 DBeaver 확인 절차

## 목표 구조

개발·검증 환경의 Tensor 내부 Framework DB는 Provider별로 하나만 사용한다.

```text
MSSQL       edan_framework  ── Platform + Worker + DataSet 테이블
PostgreSQL  edan_framework  ── Platform + Worker + DataSet 테이블
Oracle      EDAN_FRAMEWORK  ── Platform + Worker + DataSet 테이블

MSSQL       edan            ── Edan SYS/BAS 테이블
PostgreSQL  edan            ── Edan SYS/BAS 테이블
Oracle      EDAN             ── Edan SYS/BAS 테이블
```

`DataSetContent` 파일 저장소는 Framework 메타데이터 DB와 별도이며, 개발 기본값은 `App_Data/edan-tensor-dataset-content.db`다.

## 적용 순서

1. 각 Provider에서 `edan_framework`와 `Edan` 데이터베이스를 생성한다.
2. `scripts/sqlserver/Apply-Migrations.ps1`, `scripts/postgresql/Apply-Migrations.ps1`, `scripts/oracle/Apply-Migrations.ps1`를 실행한다. 기본값은 Framework 통합 DB다.
3. `scripts/edan/Apply-Migrations.ps1`를 같은 Provider의 Edan DB 연결 문자열로 실행한다.
4. 아래 설명 SQL을 각 DB에서 실행한다.
   - PostgreSQL: `scripts/database/postgresql/Apply-DatabaseComments.sql`
   - SQL Server: `scripts/database/sqlserver/Apply-DatabaseComments.sql`
   - Oracle: `scripts/database/oracle/Apply-DatabaseComments.sql`
5. DBeaver에서 새 연결을 열고 테이블의 Properties/Comments와 컬럼 Comments를 확인한다.

## 연결 문자열 예시

```text
PostgreSQL Framework: Host=<host>;Port=5432;Database=edan_framework;Username=edan;Password=<secret>
PostgreSQL Edan:     Host=<host>;Port=5432;Database=edan;Username=edan;Password=<secret>
SQL Server Framework: Server=<host>,1433;Database=edan_framework;User Id=edan;Password=<secret>;TrustServerCertificate=True
SQL Server Edan:     Server=<host>,1433;Database=edan;User Id=edan;Password=<secret>;TrustServerCertificate=True
Oracle Framework:    User Id=EDAN;Password=<secret>;Data Source=<host>:1521/FREEPDB1
Oracle Edan:         User Id=EDAN;Password=<secret>;Data Source=<host>:1521/EDAN
```

실제 비밀번호와 고객사 접속정보는 저장소에 기록하지 않는다. VBox에 직접 적용할 때는 VM에서 실행하고, DBeaver는 같은 VM의 포트로 접속한다.

## 기존 3개 DB를 통합할 때

기존 `edan_platform`, `edan_worker`, `edan_dataset`을 바로 삭제하지 않는다. 먼저 각 DB를 백업하고, `edan_framework`에 세 Context의 마이그레이션을 적용한 뒤 데이터 이관·건수 비교·Health 검증을 수행한다. 검증 완료 후에만 기존 DB를 보존 또는 폐기한다.
