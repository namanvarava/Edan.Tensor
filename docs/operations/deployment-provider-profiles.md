# 배포 환경별 데이터베이스 Provider 프로파일

EDAN.Tensor는 배포 환경에 따라 기본 저장 Provider를 선택할 수 있습니다.

## 프로파일

| 프로파일 | 대상 | 기본 Provider | 저장 위치 |
|---|---|---|---|
| Online | Edan 운영 서비스 또는 중앙 운영 환경 | PostgreSQL | Platform·Worker·DataSet 메타데이터를 PostgreSQL에 저장 |
| OfflineSingleCustomer | 폐쇄망 또는 고객사 전용 단일 설치 | SQLite | 고객사 내부의 App_Data 파일에 저장 |

두 프로파일 모두 API와 Worker가 같은 Provider 계열을 사용해야 합니다. Provider만 바꾸고 한 호스트의 연결 문자열을 다른 Provider로 남겨 두면 안 됩니다.

## Online 프로파일

운영 배포에서는 다음 값을 환경 변수 또는 비밀 저장소로 주입합니다.

```text
Database__Provider=PostgreSQL
ConnectionStrings__PlatformDatabase=Host=<host>;Port=5432;Database=edan_platform;Username=<user>;Password=<secret>;SSL Mode=Require
ConnectionStrings__WorkerDatabase=Host=<host>;Port=5432;Database=edan_worker;Username=<user>;Password=<secret>;SSL Mode=Require
ConnectionStrings__DataSetDatabase=Host=<host>;Port=5432;Database=edan_dataset;Username=<user>;Password=<secret>;SSL Mode=Require
ConnectionStrings__DataSetContentDatabase=Data Source=/app/App_Data/edan-tensor-dataset-content.db;Cache=Shared
```

Migration은 애플리케이션 시작 시 자동 실행하지 않고 배포 단계에서 별도로 적용합니다.

## OfflineSingleCustomer 프로파일

폐쇄망 고객사 전용 설치에서는 DB 서버 없이 SQLite 파일을 사용합니다.

```text
Database__Provider=Sqlite
ConnectionStrings__PlatformDatabase=Data Source=/app/App_Data/edan-platform.db;Cache=Shared
ConnectionStrings__WorkerDatabase=Data Source=/app/App_Data/edan-worker.db;Cache=Shared
ConnectionStrings__DataSetDatabase=Data Source=/app/App_Data/edan-dataset.db;Cache=Shared
ConnectionStrings__DataSetContentDatabase=Data Source=/app/App_Data/edan-dataset-content.db;Cache=Shared
```

파일은 API와 Worker가 함께 접근할 수 있는 영속 볼륨에 둡니다. Docker Compose에서는 `./app-data:/app/App_Data` 바인드 마운트를 사용하고, 백업 시 네 파일을 일관된 시점에 함께 백업합니다.

SQLite를 사용하더라도 인증을 자동으로 우회하지 않습니다. 폐쇄망에서 Keycloak 등 내부 IdP를 사용할 경우 `Authentication__Mode=External`과 내부 Metadata 주소를 사용하고, 별도 인증을 구성하지 않는 개발 전용 환경에서만 `Bypass`를 사용합니다.

## 테넌트 및 라이선스

`OfflineSingleCustomer`는 설치 시 고객사 Tenant 하나를 생성하는 배포 모드입니다. 사용자 추가는 기존 Tenant의 Membership으로 처리하며, 사용자마다 새 Tenant를 만들지 않습니다. 오프라인 라이선스 파일과 사용량 리포트는 Framework 저장소 또는 보호된 `App_Data` 하위 경로에 보관하고, 리포트 파일만 외부로 반출합니다.

## 전환 규칙

1. Provider를 바꾸기 전에 대상 Migration을 적용합니다.
2. 기존 DB 파일과 PostgreSQL 백업을 보존합니다.
3. API와 Worker를 동시에 같은 프로파일로 재배포합니다.
4. `/health`와 `/health/database`를 확인합니다.
5. 플랫폼·Worker·Dataset Migration History와 라이선스 검증을 확인합니다.

SQLite는 폐쇄망·단일 고객사에 적합하지만, 여러 인스턴스의 동시 쓰기와 대규모 운영에는 PostgreSQL 프로파일을 사용합니다.
