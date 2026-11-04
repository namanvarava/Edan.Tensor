# 플랫폼 응용 계층

플랫폼 응용 계층은 도메인 객체를 업무 흐름으로 연결하고 저장소 경계를 정의합니다.

## 주요 구성
- `Abstractions/PlatformRepositories.cs`: 플랫폼 도메인 저장소와 작업 단위 인터페이스
- `Abstractions/ITenantMembershipRepository.cs`: Tenant 소속 조회 및 권한 확인 계약
- `Abstractions/IScopedMembershipRepository.cs`: 조직·프로젝트 소속 조회와 범위 권한 확인 계약
- `UseCases/PlatformProvisioningService.cs`: Tenant·조직·프로젝트·사용자·자원·정책 생성 업무
- `UseCases/TenantMembershipService.cs`: 외부 인증 주체의 Tenant 소속 관리
- `UseCases/ScopedMembershipService.cs`: 조직·프로젝트 소속 부여와 상위 소유권 검증
- `UseCases/PlatformAuthorizationService.cs`: 역할과 계층형 정책을 결합한 권한 평가 및 감사 저장
- `UseCases/AuthorizationPolicyService.cs`: 소유권 검증을 포함한 접근 정책 등록
- `Abstractions/IAuthorizationPolicyRepository.cs`: 적용 가능한 정책 조회 및 감사 기록 저장 계약

## 권한 평가
- Tenant 소속이 활성 상태인지 확인합니다.
- Tenant Administrator는 선택 Tenant의 하위 범위에 전체 생성 권한을 가집니다.
- Organization Contributor와 Manager는 조직 범위에서 생성할 수 있습니다.
- Project Contributor와 Manager는 프로젝트 범위에서 생성할 수 있습니다.
- Organization Manager만 하위 프로젝트에 생성 권한을 상속합니다.
- Tenant 소속만 가진 Member는 조직·프로젝트 범위 자원을 생성할 수 없습니다.

저장소와 Unit of Work는 인터페이스로 추상화하며, 실제 데이터베이스 구현은 Infrastructure 계층에서 담당합니다.
