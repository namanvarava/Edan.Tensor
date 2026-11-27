# 개발자용 기본 Tensor API

고객사 정보와 운영 Secret 없이도 개발자가 API 계약과 SYS/BAS 기준 스키마를 검증할 수 있도록 개발 프로필을 제공한다.

## 기본 원칙

- 개발용 기본 데이터베이스는 SQLite를 사용한다.
- API와 Worker는 별도의 로컬 SQLite 파일을 사용한다.
- 개발 환경에서만 `Authentication__Mode=Bypass`를 선택할 수 있다.
- 외부 IdP를 사용할 수 있는 개발 환경에서는 `Authentication__Mode=External`을 우선한다.
- Bypass는 운영·스테이징·고객사 환경에서 사용할 수 없다.
- 개발 Seed는 `Development` 환경에서만 실행되고, 고객 업무 데이터나 운영 Secret을 포함하지 않는다.

## 오프라인 개발 설정 예시

```text
ASPNETCORE_ENVIRONMENT=Development
Operations__Profile=SME
Database__Provider=Sqlite
ConnectionStrings__PlatformDatabase=Data Source=./app-data/edan-platform.db;Cache=Shared
ConnectionStrings__WorkerDatabase=Data Source=./app-data/edan-worker.db;Cache=Shared
ConnectionStrings__DataSetDatabase=Data Source=./app-data/edan-dataset.db;Cache=Shared
ConnectionStrings__DataSetContentDatabase=Data Source=./app-data/edan-dataset-content.db;Cache=Shared
Authentication__Mode=Bypass
DevelopmentData__SeedBypassTenant=true
Database__ApplyMigrationsInDevelopment=true
```

## 외부 IdP를 사용하는 개발 설정

```text
ASPNETCORE_ENVIRONMENT=Development
Operations__Profile=SME
Database__Provider=Sqlite
Authentication__Mode=External
Authentication__MetadataAddress=https://idp.example.test/realms/EDAN-NONPROD/.well-known/openid-configuration
Authentication__Issuer=https://idp.example.test/realms/EDAN-NONPROD
Authentication__Audience=edan-tensor-api
DevelopmentData__SeedExternalTenant=true
```

외부 IdP 주소와 테스트 주체 식별자는 개발 환경에서만 주입하며 저장소에 실제 고객 Secret을 기록하지 않는다.

## 확인 순서

1. `app-data` 디렉터리를 만든다.
2. 개발 환경 변수 또는 개발용 `.env`를 주입한다.
3. API와 Worker를 시작한다.
4. `/health`와 `/health/database`를 확인한다.
5. 개발용 Tenant와 인증 주체가 준비되었는지 확인한다.
6. SYS/BAS 기준 테이블과 API 계약을 테스트한다.

개발 프로필은 제품 공통 기능을 빠르게 확인하기 위한 것이며, 고객사 DB 바인딩·라이선스·운영 Secret을 대신하지 않는다.
