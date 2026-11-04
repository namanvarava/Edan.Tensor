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

- `GET http://localhost:5000/health`: API 프로세스 상태
- `GET http://localhost:5000/health/database`: 데이터베이스 연결 상태

실제 포트는 실행 시 출력되는 ASP.NET Core 주소를 사용합니다. 연결 성공 시 `Connected`와 `Green`을 반환하고 연결 실패 시 HTTP 503과 `Red` 상태를 반환합니다.

## 연결 문자열 변경

환경 변수로 기본 연결 문자열을 변경할 수 있습니다.

```powershell
$env:ConnectionStrings__PlatformDatabase = "Data Source=D:\EDAN\Data\edan-platform.db"
dotnet run --project src/Hosts/EDAN.Tensor.ApiHost
```

## 운영 환경 주의사항

- SQLite 파일 경로에 API 프로세스의 읽기·쓰기 권한이 필요합니다.
- 개발 환경 Migration 적용은 개발 편의 설정입니다. 운영 환경에서는 배포 파이프라인에서 Migration을 별도 승인·적용해야 합니다.
- 운영 환경에 배포하기 전 EF Core Migration과 백업·복구 정책을 확정해야 합니다.
- DataSet용 DuckDB·Parquet 저장소는 Platform 메타데이터 SQLite와 별도 수명주기로 관리합니다.
