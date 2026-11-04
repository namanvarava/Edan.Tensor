# Platform Infrastructure 설계

## 목적

Platform Infrastructure는 Application이 정의한 저장소 계약을 EF Core로 구현합니다. Domain과 Application은 데이터베이스 공급자에 종속되지 않습니다.

## 의존성 방향

```text
Platform.Application ──> Platform.Domain + SharedKernel
Platform.Infrastructure ──> Platform.Application + Platform.Domain + SharedKernel + EF Core
Host ──> Platform.Infrastructure (공급자 설정 주입)
```

Infrastructure는 Application의 추상화를 구현하며 Domain에서 Infrastructure를 참조하지 않습니다.

## 영속성 모델

| 집합 | 저장 테이블 | 고유성·동시성 |
|---|---|---|
| Tenant | platform_tenants | Code 전역 고유 |
| Organization | platform_organizations | TenantId + Code 고유 |
| Project | platform_projects | OrganizationId + Code 고유 |
| User | platform_users | TenantId + Email 고유 |
| Resource | platform_resources | Tenant 범위 인덱스 |
| Policy | platform_policies | Version 동시성 토큰 |
| TenantMembership | platform_tenant_memberships | TenantId + SubjectId 고유 |
| OrganizationMembership | platform_organization_memberships | TenantId + OrganizationId + SubjectId 고유 |
| ProjectMembership | platform_project_memberships | TenantId + OrganizationId + ProjectId + SubjectId 고유 |
| AuthorizationPolicy | platform_authorization_policies | Tenant·Subject·Action·ResourceType 인덱스 |
| AuthorizationAuditEntry | platform_authorization_audit | Tenant·발생 시각 및 Subject·발생 시각 인덱스 |

Resource와 Policy의 Scope는 TenantId, 선택적 OrganizationId, 선택적 ProjectId의 개별 열로 저장하며, 도메인에서는 ResourceScope 값 객체로 제공합니다.

## 테넌트 격리

Organization 및 Project 코드 중복 검사는 각각 TenantId 및 OrganizationId 조건을 포함합니다. User 이메일 중복 검사는 TenantId 조건을 반드시 포함합니다. 프로젝트에는 TenantId와 OrganizationId를 함께 보존하여 상위 범위 일치 여부를 확인할 수 있게 합니다.

## 공급자 독립성 및 현재 연결

Infrastructure 프로젝트는 EF Core 핵심 패키지만 참조하고 실제 공급자는 Host에서 구성합니다. 현재 API Host는 로컬 개발 기본 공급자로 SQLite를 선택하며 기본 파일은 `App_Data/edan-tensor-platform.db`입니다. `ConnectionStrings:PlatformDatabase` 또는 `ConnectionStrings__PlatformDatabase` 환경 변수로 연결 문자열을 교체할 수 있습니다. DataSet의 DuckDB·Parquet 분석 저장 구조와 Platform 메타데이터 저장 구조는 서로 독립적입니다.

`/health/database`는 DB 연결 가능 여부를 확인하고 `Green` 또는 `Red` 신호등 상태를 반환합니다. 개발 환경의 `Database:ApplyMigrationsInDevelopment=true` 설정은 API Host 시작 과정에서 EF Core Migration을 적용합니다. 운영 환경에서는 승인된 배포 작업에서 Migration을 적용합니다.

## 관계형 무결성

- Organization.TenantId는 Tenant.Id를 참조합니다.
- Project는 TenantId와 OrganizationId를 함께 사용하여 동일 Tenant의 Organization만 참조합니다.
- User.TenantId는 Tenant.Id를 참조합니다.
- Resource와 Policy는 Tenant, 선택적 Organization, 선택적 Project를 복합 외래 키로 연결합니다.
- Project 범위는 Organization 범위를 반드시 포함하도록 CHECK 제약을 둡니다.
- 상위 데이터 삭제는 `RESTRICT`로 제한하여 소유 데이터의 우발적 연쇄 삭제를 방지합니다.

## Migration 운영

초기 스키마는 `20261104100000_InitialPlatformSchema` Migration으로 관리합니다. Tenant 소속은 `20261105100000_AddTenantMemberships`, 조직·프로젝트 소속은 `20261105120000_AddScopedMemberships`, 접근 정책·감사 기록은 `20261105140000_AddAuthorizationPoliciesAndAudit` Migration으로 관리합니다. 개발 환경에서는 `Database:ApplyMigrationsInDevelopment=true`일 때 API Host 시작 과정에서 `MigrateAsync`를 실행합니다. 운영 환경에서는 배포 파이프라인의 승인된 마이그레이션 작업으로 스키마를 변경하며 API 인스턴스가 임의로 스키마를 변경하지 않도록 합니다.

## 트랜잭션 경계

요청 범위에서 하나의 PlatformDbContext를 공유합니다. 저장소는 변경을 추적하고 PlatformUnitOfWork가 SaveChangesAsync를 호출합니다. 권한 평가 감사 기록도 같은 작업 단위로 저장하며 감사 저장에 실패하면 해당 요청을 성공 처리하지 않습니다.

## SQLite 공급망 보안

SQLite 네이티브 라이브러리의 취약 버전 경고를 해결하기 위해 `SQLitePCLRaw.lib.e_sqlite3`를 2.1.14로 고정합니다. 의존성 업데이트 후 CI의 NuGet 취약성 경고를 확인합니다.

## 검증 한계

SQLite Migration 기반 통합 테스트에서 저장·조회, 고유 인덱스, Tenant 소속 복합 외래 키를 검증합니다. 운영 환경의 백업·복구, 부하, 장애 조치 및 실제 배포 마이그레이션은 별도 운영 검증이 필요합니다.
