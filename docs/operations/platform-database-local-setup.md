# Platform 데이터베이스 로컬 연결 안내

## 기본 구성

API Host는 Platform 메타데이터 저장소에 SQLite를 사용합니다. 기본 데이터베이스 파일은 `src/Hosts/EDAN.Tensor.ApiHost/App_Data/edan-tensor-platform.db`에 생성됩니다.

## 실행

저장소 루트에서 다음 명령을 실행합니다.

```powershell
dotnet run --project src/Hosts/EDAN.Tensor.ApiHost
```

개발 환경에서는 `Database:ApplyMigrationsInDevelopment=true` 설정에 따라 API 시작 시 EF Core Migration을 적용합니다. 초기 Migration은 `20261104100000_InitialPlatformSchema`입니다.

## 연결 상태 확인

브라우저 또는 HTTP 클라이언트에서 다음 주소를 호출합니다.

- `GET http://localhost:5000/health`: API 프로세스와 Platform·Worker·DataSet·DataSet 콘텐츠 SQLite 상태
- `GET http://localhost:5000/health/database`: 네 개 데이터베이스의 연결·기본 스키마 상태

실제 포트는 실행 시 출력되는 ASP.NET Core 주소를 사용합니다. 모든 데이터베이스가 연결되고 기본 테이블을 읽을 수 있으면 `/health`는 HTTP 200과 `Healthy`, `/health/database`는 `Connected`와 `Green`을 반환합니다. 하나라도 실패하면 두 경로 모두 HTTP 503을 반환하며, 응답에는 데이터베이스별 성공 여부만 포함하고 연결 문자열과 예외 상세는 포함하지 않습니다.

## 연결 문자열 변경

환경 변수로 기본 연결 문자열을 변경할 수 있습니다.

```powershell
$env:ConnectionStrings__PlatformDatabase = "Data Source=D:\EDAN\Data\edan-platform.db"
dotnet run --project src/Hosts/EDAN.Tensor.ApiHost
```

## 운영 환경 주의사항

- SQLite 파일 경로에 API 프로세스의 읽기·쓰기 권한이 필요합니다.
- 개발 환경 Migration 적용은 개발 편의 설정입니다. 운영 환경에서는 배포 파이프라인에서 Migration을 별도 승인·적용해야 합니다.
- 운영 환경에 배포하기 전 EF Core Migration과 백업·복구 정책을 확정해야 합니다. 네 SQLite 파일을 함께 복구하는 절차는 [SQLite 백업·복구 런북](platform-database-backup-restore.md)을 따릅니다.
- DataSet용 DuckDB·Parquet 저장소는 Platform 메타데이터 SQLite와 별도 수명주기로 관리합니다.

## API Host와 Worker Host의 공유 데이터베이스 구성

DataSet 저장량 스냅샷과 Quota 임계치 이벤트의 주기적 수집은 전용 Worker Host가 5분 간격으로 실행합니다. API Host는 저장량 및 이벤트 이력 조회 API만 제공합니다.

두 Host는 반드시 동일한 Platform·Worker·DataSet 메타데이터·DataSet 콘텐츠 DB를 사용해야 합니다. Worker Host는 잘못된 별도 DB 파일을 조용히 생성하지 않도록 아래 연결 문자열을 명시적으로 요구합니다. 두 프로세스의 상대 경로 기준 디렉터리가 다르므로 절대 경로 사용을 권장합니다.

PowerShell 예시(두 Host 실행 터미널에 동일하게 설정):

```powershell
$env:ConnectionStrings__PlatformDatabase = "Data Source=D:\EDAN\Data\edan-platform.db"
$env:ConnectionStrings__WorkerDatabase = "Data Source=D:\EDAN\Data\edan-worker.db"
$env:ConnectionStrings__DataSetDatabase = "Data Source=D:\EDAN\Data\edan-dataset.db;Cache=Shared"
$env:ConnectionStrings__DataSetContentDatabase = "Data Source=D:\EDAN\Data\edan-dataset-content.db;Cache=Shared"
```

API Host와 Worker Host를 각각 실행합니다.

```powershell
dotnet run --project src/Hosts/EDAN.Tensor.ApiHost
dotnet run --project src/Hosts/EDAN.Tensor.WorkerHost
```

개발 환경 Migration 자동 적용을 사용하는 경우 두 Host가 동시에 Migration을 실행하지 않도록 순차적으로 시작합니다. 운영 환경에서는 두 Host 모두 자동 Migration을 비활성화하고 배포 파이프라인에서 각 데이터베이스 Migration을 승인·적용합니다.

Worker Host의 `/health`는 Worker 프로세스 상태를 확인합니다. 실제 주기 작업 결과는 API의 Tenant 저장량 이력 및 Quota 이벤트 조회 API로 확인합니다.
Worker Host의 `/health/database`는 API Host와 같은 네 개 데이터베이스의 연결과 기본 스키마 읽기를 확인하고 `Connected/Green` 또는 `Disconnected/Red`를 반환합니다. 이 경로가 실패하면 Worker 프로세스가 살아 있어도 작업 처리를 재개하지 않고 연결 설정과 데이터베이스 상태를 조사해야 합니다.

## 재현 가능한 Release Host Smoke

Release 산출물이 준비된 개발 환경에서는 저장소 루트에서 `powershell -ExecutionPolicy Bypass -File tools/host-smoke.ps1`를 일회성으로 실행할 수 있습니다. 이 도구는 임시 SQLite 네 개, loopback 포트, `Development` 환경과 고정 Development Bypass만 사용하여 ApiHost·WorkerHost의 health와 DB health, OpenAPI, Swagger UI와 보호 Tenant API를 확인한 뒤 프로세스·stderr·임시 경로를 정리합니다.

이 도구는 예약 실행이나 CI·운영 배포 설정을 만들지 않으며, 실제 외부 IdP·운영 인증·운영 DB·외부 수신 시스템 검증을 대체하지 않습니다. `External` 또는 운영 환경에서는 이 도구의 Bypass 설정을 사용하지 말고 실제 인증·인가와 승인된 운영 DB 절차를 적용해야 합니다.
