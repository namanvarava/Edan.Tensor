# EDAN Tensor

**Universal Data Tensor Platform (범용 데이터 텐서 플랫폼)**

EDAN Tensor는 **사용자의 편의성, 모듈형 아키텍처, 확장 가능한 데이터 통합**을 핵심 가치로 설계된 .NET 기반의 범용 데이터 플랫폼입니다.

## 초기 저장소 기반 구조

이 저장소는 .NET 10 기반의 모듈형 모놀리스(Modular Monolith) 아키텍처를 기반으로 시작합니다.

초기 솔루션은 Platform, Commercial, Licensing, Host의 책임 영역을 분리하고, 각 모듈 간의 의존성을 명확하게 관리할 수 있도록 구성합니다.

### 아키텍처 설계 원칙

- **Domain 프로젝트**는 비즈니스 규칙을 포함하며, 인프라 및 UI 프레임워크에 독립적으로 유지합니다.
- **Application 프로젝트**는 유스케이스(Use Case)를 담당하며, 자체 Domain 계층에 의존합니다.
- **Infrastructure 프로젝트**는 데이터 영속성(Persistence)과 외부 시스템 연동을 구현합니다.
- **Contracts 프로젝트**는 전송 및 시스템 통합을 위한 DTO를 포함하며, Domain 엔티티를 포함하지 않습니다.
- **Commercial과 Licensing 모듈**은 버전이 관리되는 통합 계약(Integration Contracts)과 비동기 메시징 경계를 통해 통신합니다. 어느 모듈도 상대 모듈의 데이터 영속성 구현에 직접 접근하지 않습니다.
- **API 및 Worker Host**는 애플리케이션의 구성과 의존성 주입을 담당하는 컴포지션 루트(Composition Root)입니다.
- 운영 환경에서 사용하는 서명 키, 인증 정보 및 비밀 정보(Secrets)는 이 저장소에 저장하지 않습니다.

### 초기 기술 기반

- .NET 10 / C# 14
- API 호스트 구현을 위한 ASP.NET Core
- SQLite: 초기 Platform / Commercial / Licensing 메타데이터 저장소로 도입할 예정이며, 데이터베이스 어댑터는 이후 구현 단계에서 추가합니다.
- DuckDB 및 Parquet: 향후 데이터 엔진 구성 요소로 도입할 예정이며, 현재 초기 기반 구조에서는 설치하거나 구성하지 않습니다.

### 빌드

.NET 10 SDK를 설치한 후 다음 명령을 실행합니다.

```powershell
dotnet --info
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore
```

초기 기반 구조에서는 의도적으로 서드파티 NuGet 패키지에 대한 의존성을 최소화합니다.

데이터베이스 공급자 SDK, 데이터베이스 패키지, API 문서화 패키지 및 테스트 프레임워크는 호환성과 버전을 검증한 후 별도의 구현 단계에서 추가합니다.

### 초기 호스트 구성

- `EDAN.Tensor.ApiHost`: 제품 및 상용 기능 API의 구성 진입점입니다.
- `EDAN.Tensor.LicenseAuthority.Host`: 독립적인 라이선스 발급 및 관리 권한을 담당하는 호스트 경계입니다.
- `EDAN.Tensor.WorkerHost`: 백그라운드 작업 처리를 위한 호스트 기반 구조입니다.

### 개발 진행 상태

현재 저장소는 완성된 제품이 아니라 **아키텍처 기반 구조를 구축하는 초기 단계**입니다.

다음 기능은 단계적으로 추가하고 구현 결과를 검증해야 합니다.

- 비즈니스 업무 흐름 및 기능 구현
- 데이터베이스 마이그레이션
- 사용자 인증 및 권한 관리
- 암호화 키 및 서명 키 관리
- 결제 시스템 연동
- 이메일 발송 기능
- WPF 애플리케이션
- 자동화 테스트 프레임워크

모든 기능은 단계적으로 구현하고 충분한 검증을 거친 후 운영 환경에 적용해야 합니다.
