# EDAN Tensor

**Universal Data Tensor Platform (범용 데이터 텐서 플랫폼)**

EDAN Tensor는 .NET 10 / C# 14 기반의 Modular Monolith로 개발 중인 데이터 플랫폼입니다.
개발자는 아래의 구현 현황과 미정 요구사항을 구분해야 하며, CI 성공을 제품 전체의 운영 완료로 해석해서는 안 됩니다.

## 구현 현황

| 영역 | 현재 구현 | 남은 주요 조건 |
| --- | --- | --- |
| Platform | Tenant·Organization·Project, 소속·권한 Policy, 감사와 아카이브, API 인증 경계 | 실제 Identity Provider와 키 회전 등 운영 보안 검증 |
| DataSet | Tenant 소유 메타데이터, 스키마 revision, 전용 DbContext·Migration, SQLite 콘텐츠 어댑터, 계층형 저장량 정책 | 검증 프로필·규칙·결과 계약과 Validation Handler, DuckDB·Parquet 어댑터 |
| Worker | 전용 DB, 작업 유형 카탈로그, Lease/Fencing, 체크포인트, 재시도·DLQ·재처리, 종료 인계, Tenant 커서 LRU | 실제 운영 Host 및 외부 전달 응답 유실의 종단 간 장애 시험 |
| Commercial / Licensing | 독립된 Contracts·Domain·Application·Infrastructure 프로젝트 | 업무 흐름, 권리·만료·결제 정책과 수용 기준 |
| 클라이언트 | 현재 저장소에는 WPF 클라이언트가 없습니다. | 화면과 업무 흐름의 범위 확정 및 구현 |

[검증 현황 문서](docs/validation/2026-11-06-main-ci-and-scope.md)는 커밋별 Actions 증거와 미완료 조건을 기록합니다.
[DataSet 검증 v1 제안](docs/architecture/dataset-validation-v1-proposal.md)은 미정 계약의 검토안이며 승인 또는 구현 완료 기록이 아닙니다.

## 아키텍처 규칙

- SharedKernel은 Platform·Commercial·Licensing 모듈에 의존하지 않습니다.
- Domain은 업무 규칙을 담당하며 Application·Infrastructure·Host를 참조하지 않습니다.
- Application은 같은 모듈의 Domain·Contracts와 허용된 추상화에 의존합니다.
- Infrastructure는 저장소와 외부 연동을 구현하며 Host는 의존성을 조합합니다.
- Commercial과 Licensing은 서로의 내부 구현이나 저장소를 직접 참조하지 않습니다.
- DataSet 메타데이터와 콘텐츠 저장소는 별도로 관리하며 물리 경로는 불투명 Storage Reference로 감춥니다.
- Worker는 Platform과 분리된 DbContext를 사용하며 작업 접근과 변경에 Tenant 및 Lease/Fencing 조건을 적용합니다.
- 개발자는 운영 인증 정보와 서명 키를 저장소에 기록해서는 안 됩니다.

## 빌드와 테스트

.NET 10 SDK를 설치한 후 저장소 루트에서 다음 명령을 실행합니다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore
dotnet test EDAN.Tensor.sln --no-build
```

로컬 실행은 개발자 점검용입니다. 공식 CI 판정은 해당 main 커밋의 [GitHub Actions](https://github.com/namanvarava/Edan.Tensor/actions) 결과를 사용합니다.
프로세스 장애 시험은 빌드가 준비한 별도 검증 프로그램을 시작하고 강제 종료하므로 .NET 실행 파일을 찾을 수 있어야 합니다.

## 호스트와 개발 가이드

- ApiHost는 Platform·DataSet·Worker API를 조합합니다.
- WorkerHost는 등록된 Handler를 실행하고 작업 카탈로그와 저장량 스냅샷을 관리합니다.
- HostServices는 API와 Worker의 공통 조합 서비스를 제공합니다.
- LicenseAuthority.Host는 라이선스 권한의 독립 호스트 경계이며 업무 기능은 아직 구현되지 않았습니다.

[로컬 DB 설정](docs/operations/platform-database-local-setup.md), [API 사용 가이드](docs/operations/platform-api-guide.md),
[Worker 수명주기](docs/architecture/worker-task-lifecycle.md), [의존성 규칙](docs/architecture/dependency-rules.md)을 함께 확인하세요.
