# PostgreSQL 개발 환경 검증 안내

이 문서는 EDAN.Tensor의 Platform, DataSet 메타데이터, Worker 데이터베이스를 개발용 PostgreSQL 인스턴스에 연결하는 방법을 설명합니다.

## 적용 범위

개발용 PostgreSQL 연결은 다음 세 EF Core 컨텍스트에 적용됩니다.

- `PlatformDatabase`
- `DataSetDatabase`
- `WorkerDatabase`

애플리케이션은 컨텍스트별 연결 문자열을 사용할 수 있습니다. `DataSetContentDatabase`는 현재 전용 SQLite 저장소이므로 PostgreSQL로 전환되지 않습니다.

같은 PostgreSQL 데이터베이스를 공유하는 구성도 가능하지만, 기본 적용 스크립트는 영역별 데이터베이스를 사용합니다. Migration 이력은 컨텍스트별로 분리합니다.

- `__EFMigrationsHistory_Platform`: Platform 컨텍스트의 적용 이력입니다.
- `__EFMigrationsHistory_Worker`: Worker 컨텍스트의 적용 이력입니다.
- `__EFMigrationsHistory_DataSet`: DataSet 메타데이터 컨텍스트의 적용 이력입니다.

## 연결 설정

비밀번호는 저장소, 설정 파일, 명령 기록에 기록하지 않습니다. 다음 값은 개발 PC의 프로세스 환경 변수로 주입합니다.

- Provider: `PostgreSQL`
- Host: `192.168.56.101`
- Port: `5432`
- Framework Database: `edan_framework` (Platform·Worker·DataSet 통합)
- User: `edan`
- TLS: 개발 환경에서만 `Ssl Mode=Disable`

예시 연결 문자열은 다음 형식입니다.

`Host=192.168.56.101;Port=5432;Database=edan_framework;Username=edan;Password=<프로세스에서만 주입>;Ssl Mode=Disable`

PowerShell에서는 다음처럼 비밀번호를 입력한 뒤 연결 문자열을 현재 프로세스에만 설정합니다. 입력한 비밀번호는 화면에 표시되지 않으며 저장소에 기록되지 않습니다.

```powershell
$pgPassword = Read-Host "PostgreSQL 비밀번호" -AsSecureString
$pgCredential = [System.Net.NetworkCredential]::new("edan", $pgPassword).Password
$env:Database__Provider = "PostgreSQL"
$env:ConnectionStrings__PlatformDatabase = "Host=192.168.56.101;Port=5432;Database=edan_framework;Username=edan;Password=$pgCredential;Ssl Mode=Disable"
$env:ConnectionStrings__WorkerDatabase = $env:ConnectionStrings__PlatformDatabase
$env:ConnectionStrings__DataSetDatabase = $env:ConnectionStrings__PlatformDatabase
$env:Database__ApplyMigrationsInDevelopment = "true"
```

API Host와 Worker Host를 별도 PowerShell 창에서 실행하면 각 창에 같은 환경 변수를 설정해야 합니다. 실행이 끝나면 다음처럼 프로세스 환경 변수에서 제거합니다.

```powershell
Remove-Item Env:ConnectionStrings__PlatformDatabase, Env:ConnectionStrings__WorkerDatabase, Env:ConnectionStrings__DataSetDatabase, Env:Database__Provider, Env:Database__ApplyMigrationsInDevelopment -ErrorAction SilentlyContinue
Remove-Variable pgPassword, pgCredential -ErrorAction SilentlyContinue
```

운영 환경에서는 TLS를 활성화하고 인증 정보를 비밀 저장소에서 주입해야 합니다.

## 마이그레이션 실행 전 확인

1. PostgreSQL 서버가 실행 중이고 TCP 5432를 허용하는지 확인합니다.
2. 대상 데이터베이스(`edan_framework`)와 `edan` 사용자에게 스키마 변경 권한이 있는지 확인합니다. Platform·Worker·DataSet 세 Migration은 같은 DB의 별도 Migration History 테이블을 사용합니다.
3. 애플리케이션 프로세스에만 연결 문자열을 주입합니다.
4. 개발 환경에서만 `Database:ApplyMigrationsInDevelopment=true`를 사용합니다.
5. API Host를 실행한 뒤 `/health/database`에서 Platform, Worker, DataSet 연결 상태를 확인합니다.
6. Swagger에서 인증 토큰을 넣고 Platform, DataSet, Worker API를 실제로 호출합니다.

## Migration 스크립트 생성과 적용 순서

저장소는 SQLite와 PostgreSQL의 Migration 집합을 분리합니다. PostgreSQL Migration은 `EDAN.Tensor.PostgreSqlMigrations` 공용 assembly에 있으며, PostgreSQL에서 처음부터 `uuid`, `timestamp with time zone`, `character varying` 형식으로 테이블을 생성합니다. 따라서 SQLite용 `TEXT` 컬럼을 PostgreSQL에서 사후 변환하지 않습니다. 실제 PostgreSQL에 접속하지 않고도 Release Migration 스크립트를 생성할 수 있으며, 비밀번호는 명령행에 직접 기록하지 말고 현재 PowerShell 프로세스의 환경 변수로만 주입합니다.

1. `Database__Provider=PostgreSQL`과 세 연결 문자열 환경 변수를 설정합니다.
2. `PlatformDbContext` 스크립트를 생성합니다.
3. `WorkerDbContext` 스크립트를 생성합니다.
4. `DataSetDbContext` 스크립트를 생성합니다.
5. PostgreSQL 관리 도구에서 각 스크립트를 해당 데이터베이스에 적용합니다.
6. 각 컨텍스트의 `__EFMigrationsHistory_*` 테이블과 대상 테이블이 생성되었는지 확인합니다.
7. API Host와 Worker Host를 같은 Provider·연결 설정으로 실행합니다.
8. `/health/database`, Swagger API, Worker Lease·재시도 흐름을 순서대로 확인합니다.

스크립트 생성 명령은 다음 형식을 사용합니다.

`dotnet ef migrations script --project src/Hosts/EDAN.Tensor.PostgreSqlMigrations/EDAN.Tensor.PostgreSqlMigrations.csproj --startup-project src/Hosts/EDAN.Tensor.ApiHost/EDAN.Tensor.ApiHost.csproj --context PlatformDbContext --configuration Release --no-build --output .\artifacts\postgresql-platform.sql`

컨텍스트 이름만 `WorkerDbContext`, `DataSetDbContext`로 바꾸어 각각 생성합니다. 스크립트 생성 성공은 실제 DB 적용 성공을 의미하지 않으며, 적용 후 PostgreSQL의 Migration 이력과 테이블 조회 결과를 별도로 기록해야 합니다.

저장소의 `scripts/postgresql/Apply-Migrations.ps1`는 공용 PostgreSQL Migration assembly와 컨텍스트별 데이터베이스를 자동으로 선택합니다. 매개변수 없이 실행하면 임시 SQL 파일을 생성하는 확인 모드이고, `-Apply`를 지정한 경우에만 실제 PostgreSQL에 Migration을 적용합니다. `-Contexts PlatformDbContext`처럼 대상 컨텍스트를 제한할 수 있으며, `-PlatformDatabase`, `-WorkerDatabase`, `-DataSetDatabase`로 DB 이름을 지정할 수 있습니다. 두 경우 모두 비밀번호는 입력 즉시 프로세스에서 제거되며 파일에 저장되지 않습니다.

## 검증 한계

현재 저장소 작업 환경에서 `192.168.56.101:5432`로의 연결이 허용되지 않거나 PostgreSQL 비밀번호가 제공되지 않으면 실제 Migration 적용과 API 호출은 완료로 판정하지 않습니다. 이 경우 코드 빌드와 SQLite 회귀만 성공으로 기록하고, 실제 PostgreSQL 검증은 해당 VM 네트워크와 비밀번호 주입이 가능한 환경에서 재개합니다.

## 보안 주의사항

`Ssl Mode=Disable`은 개발용 격리 네트워크에서만 사용합니다. 운영 구성에는 복사하지 않습니다. 비밀번호와 토큰은 로그, Git, 설정 파일에 기록하지 않습니다.
