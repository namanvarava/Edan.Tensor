# 플랫폼 도메인

테넌트, 조직, 프로젝트, 사용자 및 각 범위의 소속·역할·정책 규칙을 관리합니다.

## 주요 구성
- `Model/PlatformDomainModel.cs`: 식별자, 소유권 범위, 집합체, 상태 전이 및 정책 버전 규칙
- `Model/TenantMembership.cs`: 외부 인증 주체의 Tenant 소속, 역할 및 접근 상태
- `Model/ScopedMemberships.cs`: Organization·Project 소속, 역할 및 폐기 규칙

## 권한 범위
- Tenant는 데이터 격리의 최상위 경계입니다.
- OrganizationMembership은 Tenant와 Organization에 속한 외부 주체의 역할을 관리합니다.
- ProjectMembership은 Tenant·Organization·Project에 속한 외부 주체의 역할을 관리합니다.
- Reader는 생성할 수 없고 Contributor는 자신이 속한 범위에서 생성할 수 있습니다.
- Organization Manager는 하위 Project 생성 권한을 상속합니다.
- Project Contributor와 Manager는 지정 Project에서 생성할 수 있습니다.
- Tenant Administrator는 선택 Tenant의 하위 범위에 전체 생성 권한을 가집니다.

도메인 계층은 저장소, ASP.NET Core, JWT 구현에 의존하지 않습니다.
