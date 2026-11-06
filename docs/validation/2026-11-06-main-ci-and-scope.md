# 2026-11-06 구현 및 CI 검증 현황

## 판정

CI 실패 복구와 Worker 종료·프로세스 장애 경계의 회귀 검증을 완료했다. 프로젝트 전체 완료는 미선언이다.
기능 문서의 후속 과제는 승인된 상세 완료 계약과 구분하며, 테스트 통과만으로 운영 적합성을 승인하지 않는다.

## main 최종 검증 기록

- PR #5 병합 직후 검증 대상 `main`은 커밋 `4a20a71844d2663b7be338907979dae1aee59446`이며, README에 Release 검증 명령과 인증 경계를 반영한 상태였다.
- [Actions Run #628](https://github.com/namanvarava/Edan.Tensor/actions/runs/42179454874)은 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 모두 성공시켰다.
- Run #628은 총 193개 테스트를 실패 0개·건너뜀 0개로 통과했으며, TRX 덮어쓰기·xUnit2031·NU1900 경고도 확인되지 않았다.
- 당시 열린 Issue와 Pull Request는 0개였으며, 필수 변경 PR #5는 `4a20a71844d2663b7be338907979dae1aee59446`으로 squash merge되었다.
- 문서 정리 PR #6은 `734ca9a94edb610e383b7656bb291e89b3b370b6`으로 squash merge되었고, 병합 후 [Actions Run #630](https://github.com/namanvarava/Edan.Tensor/actions/runs/42180471013)도 동일한 Release 검증을 성공시켰다. Run #630은 총 193개 테스트를 실패 0개·건너뜀 0개로 통과했다.
- 이 절의 판정은 각 커밋에 연결된 원격 Actions 실행 증거에 근거한다. 로컬 exact restore/test 실패와 로컬 GitHub 인증·동기화 차단은 아래 인수인계 절에 별도로 기록한다.

## 공식 검증 증거

- 코드 기준: `14db2e38ed5eba66f7737d0861b10595b7ff5362`
- [Actions Run #612](https://github.com/namanvarava/Edan.Tensor/actions/runs/42167385135)
- Restore / Build / Test 성공. 183개 통과, 실패 0, 건너뜀 0.
- SharedKernel 8, Platform Domain 27 / Application 30 / Infrastructure 12,
  DataSet Domain 6 / Application 8 / Infrastructure 8,
  Worker Domain 21 / Infrastructure 54, API 통합 9.
- API 통합 테스트는 이전 두 실패 및 호스트 간 실제 DbContext 경로 격리 회귀 검증을 포함한다.
- 이 문서와 함께 추가하는 아키텍처 테스트는 위 183개에 포함되지 않는다. 해당 커밋의 후속 Actions로 별도 검증한다.

## 실패 원인과 수정

Run #611은 Worker TaskType UNIQUE 충돌과 Tenant 소속 수 불일치(2 대신 6)로 실패했다.
Program은 CreateBuilder 직후 설정을 읽어 저장소를 등록하지만, 테스트는 ConfigureWebHost의
ConfigureAppConfiguration에서 임시 DB 설정을 주입했다. 기본 DB 사용과 이전 데이터 누적을 막기 위해
ApiHostFactory.CreateHost의 ConfigureHostConfiguration으로 설정을 앞당겼다.
Platform / Worker / DataSet 메타데이터 / 콘텐츠 DB를 각각 고유 임시 경로로 지정하고
Pooling=False로 호스트 종료 후 Windows 파일 삭제 시 연결 풀 잠금을 방지했다.
실제 Platform / Worker / DataSet DbContext가 두 호스트 간 경로를 공유하지 않는 회귀 테스트가 Run #612에서 통과했다.
기존 오염 DB는 삭제하지 않았으며 제품의 중복 제약이나 권한 검증을 완화하지 않았다.

## 구현과 요구사항 비교

| 영역 | 확인된 구현 | 남은 조건 |
| --- | --- | --- |
| Platform | Tenant·Organization·Project·소속·Policy·감사·아카이브 | 실제 Identity Provider, 키 회전, 외부 계정 연결 등 운영 보안 계약과 검증 |
| DataSet | 독립 DbContext·Migration, 불투명 저장 참조, SQLite 콘텐츠 어댑터, 계층형 quota | 검증 프로필·규칙 버전·상세 결과 저장 계약, 검증 Handler, DuckDB·Parquet 어댑터 |
| Worker | 전용 DB, 영속 카탈로그, Lease/Fencing, 체크포인트, 재시도·DLQ·재처리 | 별도 저장소 검증 프로세스의 강제 종료 복구는 검증 완료; 실제 WorkerHost와 외부 전달 응답 유실의 종단 간 시험은 잔여 |
| 종료·순회 | Registry/Dispatcher 단일 handoff 예산, Tenant cursor LRU 상한 | Dispatcher LRU 및 Registry/Dispatcher 종료 단일 예산의 직접 회귀 검증 완료 |
| Commercial / Licensing | 분리된 Contracts·Domain·Application·Infrastructure 기반 프로젝트 | 업무 규칙·라이선스·결제 상세 요구사항과 수용 기준 |
| 클라이언트 | 현재 소스 트리에 WPF 클라이언트 없음 | 화면·업무 흐름 및 완료 범위 확정 |

DataSet Validation 설계 문서의 선행 조건 중 식별자·소유 범위·저장 참조·스키마 revision은 구현되었다.
검증 프로필과 규칙 버전, 오류·경고/상세 결과 저장 계약은 확인되지 않았다.
이 부분을 임의의 JSON 구문 검사로 대체하여 완료 처리하지 않는다.
WorkerHost에 등록된 실제 업무 Handler는 DataSet 저장량 스냅샷이다.

## 아키텍처 검토

소스 csproj 참조를 대조하여 SharedKernel 독립성, Domain의 SharedKernel 의존,
Application의 동일 모듈 Domain/Contracts 및 DI 추상화 의존,
Commercial/Licensing 내부 구현 간 참조 부재를 확인했다.
DataSet은 Platform 도메인을 직접 참조하지 않고 Host의 scope validator로 경계를 연결한다.
WorkerDbContext는 PlatformDbContext와 분리되어 있다.
같은 커밋의 ArchitectureDependencyTests가 주요 프로젝트 참조 규칙을 자동 검사한다.
이는 런타임 Tenant 보안 시험과 운영 보안 검증을 대체하지 않는다.

## 잔여 우선순위와 재개 지점

1. 아키텍처·LRU·종료 예산·프로세스 장애 경계 테스트는 아래 후속 검증 증거에서 확인한다.
2. Dispatcher LRU 상한·퇴출·재접근·삭제와 Registry/Dispatcher 단일 종료 예산의 직접 회귀 검증은 완료했다.
3. DataSet 검증 프로필·지원 포맷·오류/경고·결과 보존 계약 확정 후 Validation Handler 구현.
4. 실제 프로세스 장애 복구·외부 호출 중복 전달 시험 환경과 증거 수립.
5. Identity Provider·Commercial·Licensing·WPF·분석 어댑터의 승인된 기능 범위와 수용 기준 확정.

열린 Issue가 없다는 사실은 위 제품 요구사항이 완료되었다는 증거가 아니다.
README와 일부 초기 설계 문서는 과거 기반 단계의 표현을 포함한다.
본 문서의 구현 현황은 지정 SHA 기준이며 미래 작업의 승인을 새로 부여하지 않는다.

## 2026-11-06 Worker 종료 및 프로세스 장애 검증 진행

- `0059f7a7369d145efaaa71069bedfb8902e40855`의 [Actions Run #615](https://github.com/namanvarava/Edan.Tensor/actions/runs/42169916191)는 190개 테스트를 통과했다.
- 이 수정은 인계 콜백이 Task를 반환하기 전에 동기 대기하는 경우에도 Registry의 종료 대기를 제한한다. 시간 초과가 콜백 자체를 강제 종료하거나 영속 Lease를 해제하지는 않는다.
- `313bb961b777e72ae3efdcfbc113801f6668a41e`의 [Actions Run #616](https://github.com/namanvarava/Edan.Tensor/actions/runs/42170176288)는 191개 테스트를 통과했다.
- Dispatcher의 실제 StopAsync 경로가 Registry에서 사용한 인계 예산을 다시 기다리지 않는 직접 회귀 검증을 포함한다.
- 별도 프로세스가 Lease와 체크포인트를 저장한 뒤 강제 종료되는 시험은 새로운 DbContext에서 복구, Fencing Token 증가, 오래된 쓰기 거부, Tenant 격리를 확인한다.
- 프로세스 시험은 실제 운영 WorkerHost 전체와 외부 전달 수신 시스템을 실행하는 종단 간 시험을 대체하지 않는다.
- 기존 인수인계 문서는 CI 복구·운영 경계 문서이며 전체 제품의 상세 기능 계약은 포함하지 않는다.
- Commercial/Licensing은 실제 소스에서도 AssemblyMarker 기반 상태다. 결제 정책·라이선스 권리·WPF 업무 화면의 완료 기준을 추정하여 완료 처리할 수 없다.
- DataSet의 미정 계약을 [검증 v1 상세 계약 제안](../architecture/dataset-validation-v1-proposal.md)에 정리했다. 이 문서는 구현 완료나 승인 기록이 아니다.

## 프로세스 장애 시험의 공식 결과

- [Run #617](https://github.com/namanvarava/Edan.Tensor/actions/runs/42170456891)은 검증 프로그램의 빌드 구성 불일치로 실패했다. 나머지 191개 테스트는 통과했으나 전체 실행은 실패로 판정했다.
- 실제 Build 로그는 검증 프로그램을 Debug에 출력했고 Release 테스트의 복사 대상에는 파일이 없었다.
- 수정 커밋 `5af866f0fdbbcb2c92008464636433da243bc6bf`은 검증 프로젝트를 솔루션에 등록하고 Debug/Release 매핑과 검증 프로그램 누락 시 Build 실패 조건을 추가했다.
- [Run #618](https://github.com/namanvarava/Edan.Tensor/actions/runs/42170720120)은 Restore / Build / Test를 통과했다. 총 192개 통과, 실패 0, 건너뜀 0이다.
- Build 로그는 검증 프로그램의 Release 출력을 확인했다. Worker Infrastructure의 60개 테스트에는 실제 자식 프로세스 강제 종료 및 재조회 복구 시험이 포함된다.
- Lease 만료 시험은 저장된 만료 시각 이후를 저장소의 입력 시각으로 전달한다. 실제 프로세스 강제 종료는 수행하지만 벽시계로 Lease 만료를 기다리는 운영 부하 시험은 아니다.
- 당시 변경은 README와 구조·검증 문서를 갱신했으며, 그 시점에는 최신 main 커밋의 전체 CI 후속 확인이 필요했다.
- 최신 main 커밋 `15ea31b1465bbb26e90eca9355ad1295aae59e30`에 대한 [Actions Run #619](https://github.com/namanvarava/Edan.Tensor/actions/runs/42170991678)은 2026년 11월 6일에 push 이벤트로 실행되었고, Restore, Release Build 및 Release Test 단계가 모두 성공했다. 해당 실행은 총 192개 테스트를 실패 0개와 건너뜀 0개로 통과시켰으며, `build-and-test` 작업의 최종 결과도 `success`였다.
- 최신 원격 Git 상태를 확인한 결과 `refs/heads/main`만 존재하며 해당 ref는 위 커밋을 가리킨다. 열린 Pull Request도 0개이므로 별도 브랜치와 Pull Request를 만들지 않고 원격 main에 승인된 구현과 검증 기록을 유지했다.
- 현재 완료로 판정할 수 있는 범위는 Platform의 Tenant·Organization·Project·소속·Policy·감사·아카이브, DataSet의 Tenant 소유 메타데이터·스키마 revision·SQLite 콘텐츠 저장·계층형 저장량 정책, Worker의 전용 DB·Lease/Fencing·체크포인트·재시도·DLQ·재처리·종료 인계·Tenant 커서 LRU 및 이에 대한 자동 검증이다. 이 범위의 구현과 수용 근거는 소스, 테스트 및 위 Actions 실행 결과로 확인한다.
- DataSet 검증 프로필·규칙 버전·상세 결과 저장 계약과 Validation Handler는 상세 계약이 승인되지 않았기 때문에 구현 완료 항목으로 확정하지 않는다. 지원 포맷, 최초 규칙, 오류·경고 및 결과 보존 정책을 승인하고 자동 검증 기준을 확정하면 해당 제안 문서를 구현 요구사항으로 전환하여 재개한다.
- 실제 Identity Provider·키 회전·외부 계정 연결, 실제 운영 WorkerHost와 외부 전달 수신 시스템의 응답 유실 종단 간 시험, Commercial·Licensing의 업무 규칙·결제 정책·라이선스 권리, WPF 클라이언트 및 분석용 DuckDB·Parquet 어댑터는 승인된 상세 범위와 수용 기준 또는 실행 환경이 없으므로 완료하지 않았다. 각 항목은 담당 요구사항과 수용 기준, 필요한 외부 시스템 또는 실행 환경이 확정되는 즉시 해당 경계의 테스트를 추가하여 재개한다.

## 인수인계 PDF 대조 및 최종 원격 검증

- `Edan.Tensor_Continuation_Handoff_2026-11-06.pdf`를 2쪽 전체 렌더링으로 검토했다. PDF의 기준 SHA `c187103cee76ad58e28633a259aeebc5e7dab75c`는 현재 main의 기준이 아니며, 이후 커밋과 PR이 반영되었다.
- PDF의 Actions Run #605(`42124026661`)는 Job 시작 전에 Actions 예산 정책으로 차단되었고 Runner와 Step이 생성되지 않은 과거 인프라 장애였다. 이 실행을 재시도하지 않았다. 현재 Run #621, #622, #623, #624는 실제 Job과 Step을 생성했고 `EDAN-TENSOR-WIN` Windows X64 자체 호스팅 Runner에서 실행되었다.
- 현재 Runner 실행 증거는 Actions Runner `2.338.2`, 이름 `EDAN-TENSOR-WIN`, 라벨 `self-hosted`, `Windows`, `X64`이다. 로컬 Windows 서비스 관리자 조회는 권한 부족으로 확인하지 못했지만, 현재 Job이 해당 Runner에서 성공한 사실로 연결·작업 수신·실행 상태를 확인했다. Billing 화면과 예산 설정은 변경하지 않았으며, 별도 Billing 권한 없이 설정값 자체는 검증하지 않았다.
- PDF의 Worker 설계 기준은 현재 소스와 자동 검증에 반영되어 있다. `WorkerExecutionRegistry`와 `WorkerTaskDispatcher`는 하나의 종료 인계 예산을 공유하고, Tenant 작업 커서는 Dispatcher 단일 루프의 LRU로 관리하며 기본 10,000, 허용 범위 500–100,000을 사용한다. 관련 직접 회귀 테스트와 실제 프로세스 복구 테스트가 Worker Infrastructure 테스트에 포함된다.
- WorkerHost는 전용 `WorkerDbContext`를 등록하고, DataSet 메타데이터·콘텐츠 저장소를 별도 DbContext와 연결로 등록한다. Platform 계층의 프로젝트 참조와 DataSet 경계도 기존 ArchitectureDependencyTests 및 API 통합 테스트로 확인했다.
- PR #2는 JWT Issuer/Audience 누락 시 검증이 꺼지는 보안 결함, Worker xUnit2031 경고 및 외부 PR의 GitHub-hosted Runner 사용을 수정하고 `af39e318547195d32acbef4c78843da5d4a97a8c`로 squash merge했다. PR #3은 `clean:false` 작업 디렉터리에서 고정 TRX 파일이 충돌하는 경고를 실행 ID별 파일명으로 수정하고 `9de3e2b3a5d5930fbbb780adf24f969f5cee73e0`으로 squash merge했다.
- 최종 main Run #624(`42177862848`)는 Restore, Release Build, Release Test를 모두 성공시켰다. 총 193개 테스트가 실패 0개·건너뜀 0개로 통과했고, 최종 로그에서 TRX 덮어쓰기·xUnit2031·NU1900 경고가 확인되지 않았다.
- 로컬 Windows에서는 exact `dotnet restore EDAN.Tensor.sln`과 exact `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`가 각각 종료 코드 1로 실패했다. Restore는 로컬 SDK workload resolver 및 네트워크 취약성 피드 환경 제약으로, Test는 testhost 연결 시간초과로 재현했다. 수정 후 로컬 Release Build는 오류 0으로 성공했으며, 전체 Release 수용 판정은 위 자체 호스팅 Runner 실행을 기준으로 한다.
- PDF에 기록된 Billing 화면 확인, 로컬 GitHub CLI 토큰 재인증, Windows 서비스 관리자 권한 및 승인되지 않은 제품 요구사항은 변경하지 않았다. Billing 예산 확인·로컬 Git 동기화·외부 IdP, 실제 외부 전달 수신 시스템, Commercial/Licensing 업무 계약, WPF, DataSet Validation 및 DuckDB/Parquet의 재개 조건은 해당 권한·승인·실행 환경이 제공될 때 처리한다.


