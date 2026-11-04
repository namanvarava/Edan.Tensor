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

## 공급자 선택

Infrastructure는 특정 데이터베이스 공급자를 강제하지 않습니다. API Host 또는 Worker Host에서 `AddPlatformInfrastructure`에 SQLite, PostgreSQL, SQL Server 등 선택한 EF Core 공급자 설정을 전달합니다.

## 트랜잭션

같은 요청 범위에서 등록된 저장소와 Unit of Work는 같은 DbContext를 사용합니다. Application은 `SaveChangesAsync`를 호출하여 변경을 확정합니다.

## 운영 유의사항

EF Core 인덱스 정의는 관계형 공급자에서 고유성 제약으로 동작합니다. InMemory 공급자는 관계형 제약과 트랜잭션 동작을 완전히 재현하지 않으므로 통합 검증에는 실제 선택 공급자를 사용해야 합니다. Tenant·Organization·Project 간 교차 행 소속 일관성은 현재 Application 검증으로 보장하며, 실제 관계형 공급자를 선택한 뒤 복합 외래 키 제약을 추가 검토합니다.
