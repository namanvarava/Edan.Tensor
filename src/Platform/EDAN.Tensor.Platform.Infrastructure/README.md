# 플랫폼 인프라 계층

플랫폼 인프라 계층은 Platform Application의 저장소 계약을 EF Core 기반 영속성 구현으로 연결합니다.

## 구성
- `PlatformDbContext`: 플랫폼 메타데이터 테이블과 매핑을 관리합니다.
- `PlatformRepositories`: Tenant, Organization, Project, User, TenantMembership, OrganizationMembership, ProjectMembership, Resource, Policy 저장소와 Unit of Work를 제공합니다.
- `PlatformInfrastructureExtensions`: 호스트가 선택한 데이터베이스 공급자 설정으로 의존성을 등록합니다.

## 데이터 경계와 제약
- Tenant 코드는 전역 고유 인덱스를 적용합니다.
- Organization 코드는 Tenant 범위에서 고유합니다.
- Project 코드는 Organization 범위에서 고유합니다.
- User 이메일은 Tenant 범위에서 고유합니다.
- TenantMembership은 (TenantId, SubjectId) 조합을 고유하게 유지합니다.
- OrganizationMembership은 (TenantId, OrganizationId, SubjectId) 조합을 고유하게 유지합니다.
- ProjectMembership은 (TenantId, OrganizationId, ProjectId, SubjectId) 조합을 고유하게 유지합니다.
- 조직·프로젝트 소속은 부모 집합체의 복합 소유권 키를 참조합니다.
- Resource와 Policy는 Tenant 및 선택적 Organization·Project 범위를 저장합니다.
- Policy Version은 동시성 토큰으로 사용합니다.

## Migration
- 초기 스키마: `20261104100000_InitialPlatformSchema`
- Tenant 소속 스키마: `20261105100000_AddTenantMemberships`
- 조직·프로젝트 소속 스키마: `20261105120000_AddScopedMemberships`

## 데이터베이스 연결
API Host의 기본 개발 공급자는 SQLite이며, 연결 문자열은 `ConnectionStrings:PlatformDatabase`에서 관리합니다. 개발 환경에서는 `Database:ApplyMigrationsInDevelopment=true` 설정에 따라 EF Core Migration을 적용합니다. 운영 환경에서는 승인된 배포 단계에서 Migration을 적용해야 합니다.

Infrastructure는 특정 데이터베이스 공급자를 강제하지 않습니다. 운영 환경에서는 연결 문자열과 EF Core 공급자를 운영 인프라 정책에 맞게 교체합니다.
