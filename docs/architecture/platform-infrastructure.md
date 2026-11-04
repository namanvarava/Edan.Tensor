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

Resource와 Policy의 Scope는 TenantId, 선택적 OrganizationId, 선택적 ProjectId의 개별 열로 저장하며, 도메인에서는 ResourceScope 값 객체로 제공합니다.

## 테넌트 격리

Organization 및 Project 코드 중복 검사는 각각 TenantId 및 OrganizationId 조건을 포함합니다. User 이메일 중복 검사는 TenantId 조건을 반드시 포함합니다. 프로젝트에는 TenantId와 OrganizationId를 함께 보존하여 상위 범위 일치 여부를 확인할 수 있게 합니다.

## 공급자 독립성

Infrastructure 프로젝트는 EF Core 핵심 패키지만 참조합니다. 실제 공급자는 Host에서 구성합니다. 따라서 메타데이터 저장소 선택과 DataSet의 DuckDB·Parquet 기반 분석 저장 구조를 서로 독립적으로 결정할 수 있습니다.

## 트랜잭션 경계

요청 범위에서 하나의 PlatformDbContext를 공유합니다. 저장소는 변경을 추적하고 PlatformUnitOfWork가 SaveChangesAsync를 호출합니다. 관계형 공급자 선택 후 마이그레이션과 복합 외래 키 제약을 추가합니다.

## 검증 한계

InMemory 공급자는 모델 구성과 저장소 동작을 빠르게 확인하기 위한 테스트 수단입니다. 관계형 인덱스, 외래 키, 격리 수준 및 트랜잭션 보장은 실제 선택 공급자로 별도 통합 테스트해야 합니다.
