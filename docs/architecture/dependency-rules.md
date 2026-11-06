# 의존성 규칙

1. Domain은 SharedKernel과 .NET 기본 라이브러리만 참조합니다.
2. Application은 같은 모듈의 Domain과 필요한 Contracts, SharedKernel을 참조합니다.
3. Infrastructure는 같은 모듈의 추상화를 구현합니다.
4. Host는 의존성을 조합하는 진입점입니다.
5. Commercial과 Licensing은 서로의 내부 구현을 참조하지 않으며 버전 관리 계약과 비동기 메시지로 통합합니다.
6. SharedKernel은 업무 모듈을 참조하지 않습니다.
7. 어떤 프로젝트도 Host를 참조하지 않습니다.
8. 운영 비밀 값과 개인 서명 키를 커밋하지 않습니다.
