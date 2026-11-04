# 플랫폼 응용 계층

플랫폼 응용 계층은 도메인 객체를 업무 흐름으로 연결하고 저장소 경계를 정의합니다.

## 주요 구성
- `Abstractions/PlatformRepositories.cs`: 플랫폼 도메인 저장소와 작업 단위 인터페이스
- `Abstractions/ITenantMembershipRepository.cs`: Tenant 구성원 조회 및 권한 확인 계약
- `UseCases/PlatformProvisioningService.cs`: 테넌트·조직·프로젝트·사용자·자원·정책 생성 업무
- `UseCases/TenantMembershipService.cs`: 외부 인증 주체의 Tenant 소속 부여 업무

## 핵심 검증
- 조직 생성은 활성 테넌트에서만 허용합니다.
- 프로젝트 생성은 조직의 테넌트 소속을 확인합니다.
- 사용자 이메일 중복은 테넌트 범위에서 확인합니다.
- 자원과 정책 생성은 테넌트·조직·프로젝트의 소속 관계를 확인합니다.
- Tenant 소속 부여는 활성 Tenant에서만 허용하며 중복 소속을 차단합니다.
- 저장 작업은 작업 단위 인터페이스를 통해 완료합니다.

## 구현 경계
실제 데이터베이스 연결과 트랜잭션 구현은 Platform.Infrastructure 단계에서 담당합니다. 응용 계층은 구체적인 저장 기술에 의존하지 않습니다.
