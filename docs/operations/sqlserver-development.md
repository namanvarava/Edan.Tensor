# SQL Server Provider 개발·검증 안내

EDAN.Tensor의 SQL Server Provider는 PostgreSQL 기준 구조를 유지하면서 Platform, Worker, DataSet 메타데이터를 SQL Server에 저장하기 위한 Provider입니다.

## 적용 범위

- Provider 이름: `SqlServer` (별칭 `MSSQL`, `SQL Server`)
- 전용 Migration assembly: `EDAN.Tensor.SqlServerMigrations`
- 대상 컨텍스트: `PlatformDbContext`, `WorkerDbContext`, `DataSetDbContext`
- 콘텐츠 저장소: 현재 구현상 `DataSetContentDatabase`의 SQLite 저장소를 계속 사용

## Migration 적용

비밀번호는 명령행·파일·로그에 기록하지 않습니다.

```powershell
.\scripts\sqlserver\Apply-Migrations.ps1 -Apply
```

미리보기만 실행하려면 `-Apply`를 생략합니다. 대상 DB 이름과 서버는 매개변수로 바꿀 수 있습니다.

```powershell
.\scripts\sqlserver\Apply-Migrations.ps1 `
  -Server 192.168.56.101 `
  -Port 1433 `
  -PlatformDatabase edan_platform `
  -WorkerDatabase edan_worker `
  -DataSetDatabase edan_dataset
```

## 운영 원칙

- 운영 연결 문자열은 환경 변수 또는 비밀 저장소로 주입합니다.
- 운영에서는 `Encrypt=True`와 검증 가능한 서버 인증서를 사용합니다.
- Migration은 배포 단계에서 적용하고 애플리케이션 시작 시 자동 적용하지 않습니다.
- SQL Server Migration History는 컨텍스트별로 분리합니다.
- SQLite와 PostgreSQL 회귀 검증을 MSSQL 변경 후에도 계속 실행합니다.

## 검증 기준

1. SQL Server 1433 포트 연결 확인
2. 세 컨텍스트의 전용 Migration 생성 확인
3. Migration History와 테이블 생성 확인
4. API `/health/database` 확인
5. WorkerHost `/health/database` 확인
6. 인증 API와 주요 Platform/DataSet/Worker API 호출 확인
7. SQLite 및 PostgreSQL 회귀 테스트 통과 확인
