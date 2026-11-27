# 배포 환경별 데이터베이스 Provider 프로파일

EDAN.Tensor는 배포 환경에 따라 기본 저장 Provider를 선택할 수 있습니다.

## 프로파일

| 프로파일 | 대상 | 기본 Provider | 저장 위치 |
|---|---|---|---|
| Online | Edan 운영 서비스 또는 중앙 운영 환경 | PostgreSQL | Platform·Worker·DataSet 메타데이터를 PostgreSQL에 저장 |
| OfflineSingleCustomer | 폐쇄망 또는 고객사 전용 단일 설치 | SQLite | 고객사 내부의 App_Data 파일에 저장 |
| Enterprise | 다중 인스턴스·고가용성·대규모 고객 환경 | PostgreSQL | Framework 메타데이터를 HA PostgreSQL 클러스터에 저장 |

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

## Enterprise 프로파일

대기업 설치에서는 다음 원칙을 적용합니다.

- API와 Worker를 최소 2개 인스턴스로 운영하고, 무상태 API 앞에 고객사 표준 Load Balancer를 둡니다.
- Framework의 Platform·Worker·DataSet은 PostgreSQL HA 클러스터 또는 관리형 PostgreSQL에 저장합니다. API와 Worker는 동일한 스키마 계약과 Migration 버전을 사용합니다.
- 고객 업무 데이터(Edan)는 고객사가 선택한 PostgreSQL·SQL Server·Oracle 중 하나를 Tenant Database Binding으로 연결합니다. Framework DB와 업무 DB를 하나의 분산 트랜잭션으로 묶지 않고, 복수 DB DML은 작업 오케스트레이션·멱등키·재시도·보상·감사상태로 처리합니다.
- 외부 IdP는 고객사 표준 OIDC/SAML 연계 정책을 따르며, API는 HTTPS Metadata/Issuer와 Audience를 검증합니다. Keycloak은 고객사가 제공하지 않을 때의 참조 구현입니다.
- DB 비밀번호, IdP Client Secret, 라이선스 서명키는 Vault·클라우드 Secret Manager·고객사 HSM 연계 저장소 중 하나에서 런타임 주입합니다. 환경 파일에는 Secret URI와 비밀 식별자만 둡니다.
- 백업은 PostgreSQL PITR/WAL과 고객 업무 DB의 공식 백업 기능을 함께 사용합니다. 기본 목표는 RPO 15분, RTO 1시간이며 계약 SLA가 우선합니다. 월 1회 복구 리허설과 분기 1회 장애 전환 리허설을 기록합니다.
- 로그·감사·메트릭은 고객 SIEM/관제 플랫폼으로 전달하고, API·Worker·DB·백업·라이선스 만료를 알림 규칙으로 연결합니다.
- 기본 용량 기준은 100명 동시 사용자, 25 RPS, p95 1초 이하·p99 3초 이하입니다. 실제 SLA와 부하시험 결과로 조정합니다.

적용 순서는 다음과 같습니다.

1. PostgreSQL HA, 백업 저장소, Secret Manager, IdP, Load Balancer의 고객 표준을 확정합니다.
2. `enterprise-default-profile.env.example`을 외부 운영 경로에 복사하고 Secret URI만 입력합니다.
3. 배포 파이프라인에서 Framework Migration을 단일 실행하고, API·Worker를 동일 릴리스로 롤링 배포합니다.
4. Tenant, Membership, License, 고객 업무 DB Binding과 Routine Allowlist를 등록합니다.
5. 정상·실패·재시도·보상·멱등성, 장애 전환, 백업 복구, 보안 및 부하 검증을 수행하고 증적을 보관합니다.
