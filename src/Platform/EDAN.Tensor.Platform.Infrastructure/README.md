# Platform Infrastructure

Platform Infrastructure는 Platform Application의 저장소 계약을 EF Core 기반 영속성 구현으로 연결합니다.

## 구성

- `PlatformDbContext`: 플랫폼 메타데이터 테이블과 매핑을 관리합니다.
- `PlatformRepositories`: Tenant, Organization, Project, User, Resource, Policy 저장소와 Unit of Work를 제공합니다.
- `PlatformInfrastructureExtensions`: 호스트가 선택한 데이터베이스 공급자 설정으로 의존성을 등록합니다.

## 데이터 경계와 제약

- Tenant 코드에는 전역 고유 인덱스를 적용합니다.
- Organization 코드는 Tenant 범위에서 고유합니다.
- Project 코드는 Organization 범위에서 고유합니다.
- User 이메일은 Tenant 범위에서 고유합니다.
- Project에는 TenantId와 OrganizationId를 함께 저장하여 소속 범위 조회를 지원합니다.
- Resource와 Policy는 Tenant 및 선택적 Organization·Project 범위를 저장합니다.
- Policy Version은 동시성 토큰으로 사용합니다.

## 데이터베이스 연결

- API Host의 기본 개발 공급자는 SQLite입니다.
- 기본 파일 경로는 `src/Hosts/EDAN.Tensor.ApiHost/App_Data/edan-tensor-platform.db`입니다.
- 연결 문자열은 `ConnectionStrings:PlatformDatabase`에서 관리하며 환경 변수 `ConnectionStrings__PlatformDatabase`로 덮어쓸 수 있습니다.
- `/health/database`는 연결 성공 시 `Green`, 실패 시 `Red` 상태를 반환합니다.
- 개발 환경에서는 `Database:EnsureCreatedInDevelopment=true`일 때 최초 실행 시 데이터베이스 파일과 테이블을 생성합니다.

## 공급자 선택

Infrastructure는 특정 데이터베이스 공급자를 강제하지 않습니다. 현재 API Host는 로컬 실행과 관계형 통합 테스트를 위해 SQLite를 선택했습니다. 운영 환경에서는 연결 문자열과 EF Core 공급자를 운영 인프라 정책에 맞게 교체합니다.

## 트랜잭션

같은 요청 범위에서 등록된 저장소와 Unit of Work는 같은 DbContext를 사용합니다. Application은 `SaveChangesAsync`를 호출하여 변경을 확정합니다.

## 운영 유의사항

EF Core 인덱스 정의는 관계형 공급자에서 고유성 제약으로 동작합니다. InMemory 공급자는 관계형 제약과 트랜잭션 동작을 완전히 재현하지 않으므로 SQLite 기반 통합 테스트를 별도로 수행합니다. 개발 환경의 `EnsureCreated`는 초기 검증 편의 기능이며, 운영 데이터베이스의 버전 관리에는 EF Core Migration이 필요합니다. Tenant·Organization·Project 간 교차 행 소속 일관성은 현재 Application 검증으로 보장하며, 실제 관계형 공급자를 선택한 뒤 복합 외래 키 제약을 추가 검토합니다.


## Migration 및 관계형 무결성

초기 스키마는 `Persistence/Migrations/20261104100000_InitialPlatformSchema.cs`에서 관리합니다. 개발 환경에서는 API Host가 `Database:ApplyMigrationsInDevelopment=true` 설정에 따라 Migration을 적용합니다. 운영 환경에서는 배포 파이프라인의 승인된 Migration 단계로 적용해야 합니다.

Tenant → Organization → Project 소속 관계와 Resource·Policy 범위는 복합 외래 키 및 CHECK 제약으로 보호합니다. SQLite 통합 테스트는 Migration 적용 후 실제 관계형 제약을 검증합니다.

SQLite 네이티브 라이브러리는 알려진 취약 버전을 피하기 위해 `SQLitePCLRaw.lib.e_sqlite3` 2.1.14를 사용합니다.
