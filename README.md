# EDAN Tensor

**Universal Data Tensor Platform (범용 데이터 텐서 플랫폼)**

EDAN Tensor는 .NET 10 / C# 14 기반의 Modular Monolith 데이터 플랫폼입니다.
아래 표는 승인된 이번 릴리스 범위의 구현 현황과 다음 릴리스 또는 외부 환경에 남은 조건을 구분합니다.

## 구현 현황

| 영역 | 현재 구현 | 남은 주요 조건 |
| --- | --- | --- |
| Platform | Tenant·Organization·Project, 소속·권한 Policy, 감사와 아카이브, 닫힌 JWT 인증 경계, 로컬 IdP/JWKS·키 회전 검증 | 비운영 외부 IdP 테스트 테넌트 경계 검증은 승인된 후속 Milestone #1에서 수행합니다. |
| DataSet | Tenant 소유 메타데이터, 스키마 revision, SQLite 콘텐츠 어댑터, 계층형 저장 용량 quota·관리 API·임계치 이력, UTF-8 CSV·JSON 객체 배열 검증, 불변 프로필 revision, 결과 보존·명시적 삭제 | DuckDB·Parquet 어댑터는 다음 릴리스 범위 |
| Worker | 전용 DB, 작업 유형 카탈로그, Lease/Fencing, 체크포인트, 재시도·DLQ·재처리, 종료 인계, DataSet Validation Handler, 외부 전달 Idempotency Key와 재시도, 로컬 수신기 장애 검증 | 실제 운영 수신 시스템은 이번 범위에서 제외하고, 비운영 외부 IdP는 외부 환경 제공 후 검증 |
| Commercial / Licensing | 독립된 모듈 경계를 유지하며, 승인된 개발 라이선스 1개를 발급·검증하는 별도 LicenseAuthority를 제공합니다. RBAC·기본 거부·최소 권한·감사 경계를 유지합니다. | 결제·상품·유료 권리와 운영 라이선스는 공급자·업무 계약 승인 후 구현합니다. |
| 클라이언트 | WPF 화면은 제외하지만 `EDAN.Tensor.Client.Contracts`와 `EDAN.Tensor.Client` typed API 연동 기반을 제공합니다. | 화면과 업무 흐름 승인 후 WPF를 추가합니다. |

[검증 현황 문서](docs/validation/2026-11-10-licensing-implementation.md)는 승인된 LicenseAuthority 구현과 현재 HTTP 환경 미검증 조건을 기록합니다.
[전체 구현 진행 방향](docs/implementation-roadmap.md)은 이후 단계의 순서, 의존성, 수용 기준과 운영 검증 조건을 고정합니다.
[DataSet 검증 v1 계약](docs/architecture/dataset-validation-v1-proposal.md)은 2026-11-07 승인된 구현 기준입니다.
[외부 전달 설계](docs/architecture/external-delivery.md)는 로컬 수신기 장애 재현과 Idempotency 계약을 설명합니다.
[라이선스 승인 계약](docs/architecture/licensing-v1-approved.md)은 개발 라이선스·RBAC·결제 경계를 기록하며, [결제 공급자 권고](docs/operations/commercial-payment-provider-recommendation.md)는 공급자 미확정 상태를 유지합니다.

## 아키텍처 규칙

- SharedKernel은 Platform·Commercial·Licensing 모듈에 의존하지 않습니다.
- Domain은 업무 규칙을 담당하며 Application·Infrastructure·Host를 참조하지 않습니다.
- Application은 같은 모듈의 Domain·Contracts와 허용된 추상화에 의존합니다.
- Infrastructure는 저장소와 외부 연동을 구현하며 Host는 의존성을 조합합니다.
- Commercial과 Licensing은 서로의 내부 구현이나 저장소를 직접 참조하지 않습니다.
- DataSet 메타데이터와 콘텐츠 저장소는 별도로 관리하며 물리 경로는 불투명 Storage Reference로 감춥니다.
- Worker는 Platform과 분리된 DbContext를 사용하며 작업 접근과 변경에 Tenant 및 Lease/Fencing 조건을 적용합니다.
- DataSet 검증 결과는 원문 셀 값과 내부 저장 위치를 기록하지 않으며 Tenant 관리자의 명시적 삭제 전까지 보존합니다.
- 외부 전달은 허용된 대상만 호출하고 opaque Worker 참조와 Idempotency Key를 사용하며 timeout·5xx·응답 유실을 재시도 상태로 기록합니다.
- 개발자는 운영 인증 정보와 서명 키를 저장소에 기록해서는 안 됩니다.

## 빌드와 테스트

.NET 10 SDK를 설치한 후 저장소 루트에서 다음 명령을 실행합니다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

로컬 실행은 개발자 점검용입니다. 공식 CI는 동일한 Release 명령을 Windows 자체 호스팅 Runner에서 실행하며, 해당 main 커밋의 [GitHub Actions](https://github.com/namanvarava/Edan.Tensor/actions) 결과를 사용합니다.
프로세스 장애 시험은 빌드가 준비한 별도 검증 프로그램을 시작하고 강제 종료하므로 .NET 실행 파일을 찾을 수 있어야 합니다.

## 호스트와 개발 가이드

- ApiHost는 Platform·DataSet·Worker API를 조합합니다.
- WorkerHost는 등록된 Handler를 실행하고 작업 카탈로그와 저장량 스냅샷을 관리합니다.
- HostServices는 API와 Worker의 공통 조합 서비스를 제공합니다.
- LicenseAuthority.Host는 Development 환경의 명시적 플래그에서만 개발 라이선스를 발급·폐기하며, 공개키·검증 API를 제공합니다. 개인키와 개발 라이선스 저장 파일은 저장소·로그·API 응답의 비밀 경계를 지킵니다.
- 개인 개발은 Development 환경의 `Authentication:Mode=Bypass`로 외부 IdP 없이 실행할 수 있습니다. Bypass에서는 실제 Metadata/JWKS·Issuer·Audience 검증을 수행하지 않고 고정된 개발용 비운영 IdP 시뮬레이션 프로필과 `edan-nonprod-test-subject`를 사용하지만, Tenant 소속·조직 범위·관리자 권한 검사는 유지합니다. 서비스 연결 시에는 `External` 모드와 실제 Metadata·Issuer·Audience·HTTPS 설정을 사용합니다.
- `appsettings.Development.json`은 Development Bypass의 초기화와 개발 역할만 활성화합니다. Bypass가 사용하는 `EDAN-NONPROD` Tenant, `edan-nonprod-test-user`, 안정적인 `sub`인 `edan-nonprod-test-subject`는 코드의 고정 비운영 프로필에서 제공하며 설정값으로 바꿀 수 없습니다. 같은 Tenant·소속이 있으면 역할이나 상태를 덮어쓰지 않으며, 초기화는 `Authentication:Mode=Bypass`와 Development 환경에서만 실행됩니다.
- 개발 연속성 정책에 따라 외부 IdP Metadata·JWKS·Tenant·키 회전 정보가 아직 없거나 GitHub main 보호 규칙 API가 비공개 저장소 요금제 제한으로 HTTP 403을 반환해도 개발 작업을 중지하지 않습니다. Development Bypass, 로컬 IdP/JWKS 테스트, Windows 자체 호스팅 CI를 계속 사용하고 해당 외부 경계만 미검증으로 기록합니다.
- `External` 및 운영 환경에서는 위 조건을 Bypass하지 않습니다. 외부 정보가 없으면 Issuer·Audience·Metadata·JWKS·HTTPS 검증을 유지한 채 보호 API를 닫힌 상태로 두며, 실제 외부 검증이 끝나기 전에는 외부 경계 검증 완료로 판정하지 않습니다. 현재 개발 범위의 구현·문서·Release·CI 검증과 소스 병합은 후속 Milestone #1과 별도로 종료할 수 있습니다.

[로컬 DB 설정](docs/operations/platform-database-local-setup.md), [SQLite 백업·복구 런북](docs/operations/platform-database-backup-restore.md), [API 사용 가이드](docs/operations/platform-api-guide.md),
[Worker 수명주기](docs/architecture/worker-task-lifecycle.md), [작업 카탈로그와 DataSet 검증](docs/architecture/worker-task-type-catalog-and-dataset-validation.md),
[API 보안](docs/architecture/platform-api-security.md), [의존성 규칙](docs/architecture/dependency-rules.md)을 함께 확인하세요.
[Client API 연동 계약](docs/architecture/client-api-contract.md)도 확인하세요.


