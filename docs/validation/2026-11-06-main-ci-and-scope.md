# 2026-11-06 구현 및 CI 검증 현황

## 판정

CI 실패 복구와 Worker 종료·프로세스 장애 경계의 회귀 검증을 완료했다. 프로젝트 전체 완료는 미선언이다.
기능 문서의 후속 과제는 승인된 상세 완료 계약과 구분하며, 테스트 통과만으로 운영 적합성을 승인하지 않는다.

## PR #12 병합 후 최종 main 재검증

- PR #12 [Actions 예산 차단 상태를 검증 문서에 기록합니다](https://github.com/namanvarava/Edan.Tensor/pull/12)는 승인된 Checks를 확인한 뒤 squash merge되었고, 병합 커밋은 `100c36fd629763329451646fba97c36dafed38f3`이다.
- 병합 후 원격 `main`의 [Actions Run #642](https://github.com/namanvarava/Edan.Tensor/actions/runs/42204928497)는 커밋 `100c36fd629763329451646fba97c36dafed38f3`에서 실행되어 `success`로 종료되었다. `build-and-test` 작업 ID는 `127849060237`이며, Restore·Build·Test·정리 단계를 모두 성공시켰다.
- Run #642는 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 실행되었다. 로그에는 Runner 버전 `2.338.2`, 이름 `EDAN-TENSOR-WIN`, exact `dotnet restore EDAN.Tensor.sln`, Release Build 및 Release Test가 기록되어 있다. Build는 경고 0개·오류 0개였고, 테스트는 193개 성공·실패 0개·건너뜀 0개였다.
- 병합 후 로컬 `main`과 `origin/main`은 모두 `100c36fd629763329451646fba97c36dafed38f3`을 가리켰고, 작업 디렉터리는 clean 상태였다. `git diff --check`도 종료 코드 0으로 완료했다.
- 병합 후 로컬 PC에서 exact `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 실행했다. 세 명령 모두 종료 코드 0이며, 테스트는 193개 통과·실패 0개·건너뜀 0개였다.
- 로컬 Restore와 Build에는 NuGet 감사 피드 조회 경로의 `NU1900` 경고가 각각 18개 출력되었지만 오류는 0개였다. 원격 Run #642에서는 동일한 Restore·Build가 경고 0개로 완료되었으므로 저장소 설정으로 로컬 경고를 숨기지 않았다.
- 현재 열린 Issue와 Pull Request는 각각 0개이며, `.github`에는 `schedule`, `workflow_dispatch`, `cron` 예약 실행 트리거가 없다. Runner Windows 서비스는 `Running`, `Automatic` 상태를 유지한다.
- GitHub Billing의 Actions 예산은 `$20.00 / $20.00`, 100% 사용, `Usage blocked` 상태로 남아 있다. PR #12에 이 외부 제약과 예산 증액·해제 없이 재개할 조건을 기록했으며, 예산 설정은 변경하지 않았다.

## Run #644 실패 원인과 종료 테스트 회귀 수정

- PR #13 병합 후 [Actions Run #644](https://github.com/namanvarava/Edan.Tensor/actions/runs/42206188178)는 커밋 `fab14a34009b453df7949a7cf974c7812b9a4fe3`에서 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`으로 실행되었다. Restore와 Release Build는 성공했으며 Build 경고 0개·오류 0개였지만, Test 단계에서 1개가 실패하여 전체 Run은 `failure`로 종료되었다.
- 실패 대상은 `WorkerTaskDispatcherShutdownTests.DispatcherDoesNotSpendHandoffBudgetAgainAfterRegistryTimesOut`이며, `WorkerTaskDispatcherShutdownTests.cs` 41번째 줄의 `WaitAsync(TimeSpan.FromMilliseconds(4500))`에서 `System.TimeoutException`이 발생했다. Worker Infrastructure 테스트는 59개 통과·1개 실패였고, 나머지 테스트 프로젝트는 모두 통과했다.
- 코드와 로그를 대조한 결과 제품 종료 예산 계산이나 Runner 연결 실패가 아니라, 정상 종료 약 3초와 잘못된 이중 대기 약 5초를 구분하던 4.5초 절대 시간 경계가 자체 호스팅 Runner의 동시 테스트 스케줄링 지연에 민감한 테스트 결함으로 확인되었다. 단순히 제한 시간만 늘리면 이중 대기 회귀를 놓칠 수 있으므로 적용하지 않았다.
- 최소 수정으로 테스트의 인계 예산을 2초에서 4초로 늘리고 제한 시간을 4.5초에서 8초로 조정했다. 정상 경로는 약 5초 안에 완료되고 이중 대기 경로는 약 9초가 필요하므로 두 동작의 판별 간격을 유지하면서 Runner 지연 여유를 확보한다. 테스트는 제품 동작을 우회하거나 실패를 무시하지 않는다.
- 수정 후 대상 테스트 반복 실행 4회가 모두 통과했고, Worker Infrastructure 전체 60개 테스트와 솔루션 전체 Release 검증도 통과했다. 로컬 exact Restore·Build·Test는 각각 종료 코드 0이며 전체 테스트는 193개 통과·실패 0개·건너뜀 0개였다. 수정 PR #14의 후속 자체 호스팅 CI는 아래 최종 main 검증에서 성공 여부를 확인한다.

## PR #14 수정 후 최종 main 검증

- PR #14 [Worker 종료 테스트의 Runner 시간 경계를 보강합니다](https://github.com/namanvarava/Edan.Tensor/pull/14)는 [Actions Run #645](https://github.com/namanvarava/Edan.Tensor/actions/runs/42207467868)에서 수정된 종료 테스트를 포함한 Restore·Release Build·Release Test를 모두 성공시킨 뒤 squash merge되었다. 병합 커밋은 `81abf63e602ed02335d446221c22757d1ab1c4ae`이다.
- 병합 후 최종 [Actions Run #646](https://github.com/namanvarava/Edan.Tensor/actions/runs/42207723631)은 push 이벤트로 동일한 `main` 커밋에서 실행되어 `success`로 종료되었다. `build-and-test` 작업은 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 실행되었고, Runner 버전은 `2.338.2`였다. Restore·Build·Test·정리 단계가 모두 성공했으며 Build는 경고 0개·오류 0개, 테스트는 193개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 로컬 `main`과 `origin/main`은 모두 `81abf63e602ed02335d446221c22757d1ab1c4ae`를 가리켰다. 작업 디렉터리는 clean 상태이고 `git diff --check`도 종료 코드 0으로 완료했다. 로컬 PC에서 병합 후 exact Restore·Release Build·Release Test를 다시 실행했으며 세 명령 모두 종료 코드 0, 테스트 193개 통과·실패 0개·건너뜀 0개였다.

## PR #15 문서 정리 후 최신 main 검증

- PR #15 [최종 main CI 시점을 검증 문서에 반영합니다](https://github.com/namanvarava/Edan.Tensor/pull/15)는 최신 CI 증거와 테스트 안내를 반영하여 `661b5ef83e152b1adefd908d57d080acb230d46c`로 squash merge되었다.
- PR #15의 [Actions Run #647](https://github.com/namanvarava/Edan.Tensor/actions/runs/42208473679)은 Pull Request 검증으로, 병합 후 main의 [Actions Run #648](https://github.com/namanvarava/Edan.Tensor/actions/runs/42208644723)은 push 검증으로 실행되었다. 두 실행 모두 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 Restore·Release Build·Release Test와 정리 단계를 성공시켰다.
- Run #648의 작업 ID는 `127861512047`이며 Runner 버전은 `2.338.2`이다. 로그는 커밋 `661b5ef83e152b1adefd908d57d080acb230d46c`를 checkout하고 Build 경고 0개·오류 0개, 테스트 193개 통과·실패 0개·건너뜀 0개를 기록한다.
- 최신 main 동기화 후 로컬 exact `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. 세 명령은 모두 종료 코드 0이며 테스트는 193개 통과·실패 0개·건너뜀 0개였다. 로컬 Restore와 Build에는 NuGet 취약성 피드 조회 실패에 따른 `NU1900` 경고 18개가 남았고, 경고를 숨기지 않았다.
- 최신 확인에서 열린 Issue와 Pull Request는 각각 0개이고, 작업 디렉터리는 clean 상태이며 `git diff --check`도 종료 코드 0이다. 이 기록은 자동 실행을 추가하지 않았으며, 현재 완료된 구현 범위와 승인되지 않은 잔여 제품 범위를 구분한다.

## PR #16 병합 후 main 소스 검증

- PR #16은 위 검증 문서를 갱신한 문서 전용 변경이며 `d09cc12d850a9b90e8528e71c6c74d844a290426`으로 squash merge되었다. 소스 구현은 `661b5ef83e152b1adefd908d57d080acb230d46c`와 동일하다.
- 병합 후 main의 [Actions Run #650](https://github.com/namanvarava/Edan.Tensor/actions/runs/42217248717)은 `d09cc12d850a9b90e8528e71c6c74d844a290426`을 대상으로 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 성공했다. 작업 ID는 `127890223583`, Runner 버전은 `2.338.2`이며 Restore·Release Build·Release Test·정리 단계가 모두 성공했다.
- Run #650 로그는 Build 경고 0개·오류 0개와 테스트 193개 통과·실패 0개·건너뜀 0개를 기록한다. 병합 후 로컬에서도 exact Restore·Release Build·Release Test를 모두 종료 코드 0으로 재실행했다.
- 이 문서 이후의 문서 전용 변경은 소스 동작을 바꾸지 않으며, 변경별 PR Check와 병합 후 main CI 결과를 별도로 확인한다.

## PR #17 병합 후 최신 main CI 검증

- PR #17은 PR #16 병합 후 검증 기준을 갱신한 문서 전용 변경이며 `2921556f9b9bef661f574a13e34c78038fb27170`으로 squash merge되었다.
- 병합 후 main의 [Actions Run #652](https://github.com/namanvarava/Edan.Tensor/actions/runs/42218105754)은 위 커밋을 대상으로 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 성공했다. 작업 ID는 `127893093152`, Runner 버전은 `2.338.2`이며 Restore·Release Build·Release Test·정리 단계가 모두 성공했다.
- Run #652 로그는 Build 경고 0개·오류 0개와 테스트 193개 통과·실패 0개·건너뜀 0개를 기록한다. 이 실행은 현재 main의 소스 검증 기준이며, 이후 문서 전용 변경에서도 구현 소스는 변경하지 않는다.

## PR #19 한글 문서화 변경 후 최신 main CI 검증

- PR #19 [공개 API 주석과 저장소 문서를 한글화합니다](https://github.com/namanvarava/Edan.Tensor/pull/19)는 SharedKernel 공개 API XML 설명, 모듈 표식 설명 및 SQLite·의존성 문서를 현재 구현과 한글 문서화 규칙에 맞춰 수정하고 `b763555ddd2d0c8fcdcde46bd3cbb39b88d65611`로 squash merge되었다.
- 병합 후 main의 [Actions Run #656](https://github.com/namanvarava/Edan.Tensor/actions/runs/42220435470)은 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 성공했다. 작업 ID는 `127900829188`, Runner 버전은 `2.338.2`이며 Restore·Release Build·Release Test·정리 단계가 모두 성공했다.
- Run #656 로그는 Build 경고 0개·오류 0개와 테스트 193개 통과·실패 0개·건너뜀 0개를 기록한다. 병합 후 로컬 exact Restore·Release Build·Release Test도 종료 코드 0으로 다시 실행했다.

## PR #8·#9 및 최종 main 경계 재검증

- PR #8 `로컬 main Release 검증 결과를 기록합니다`는 `039545bfacf9082cdcfd25d56067272945b12765`로 squash merge되었고, 해당 커밋의 [Actions Run #634](https://github.com/namanvarava/Edan.Tensor/actions/runs/42182959588)는 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 exact `dotnet restore EDAN.Tensor.sln`, Release Build 및 Release Test를 모두 성공시켰다. 총 193개 테스트가 실패 0개·건너뜀 0개로 통과했고, Build 경고와 오류는 0개였다.
- PR #9 `실제 Host 경계 검증 증거를 기록합니다`는 `236c6be2db6640e2bbece34578a8863617a6b730`으로 squash merge되었다. 병합 후 최신 로컬 `main`과 `origin/main`은 같은 SHA를 가리키며 작업 디렉터리는 clean 상태이고, 열린 Issue와 Pull Request는 각각 0개다.
- 병합 후 [Actions Run #636](https://github.com/namanvarava/Edan.Tensor/actions/runs/42201020746)은 `236c6be2db6640e2bbece34578a8863617a6b730`을 대상으로 동일한 Restore·Release Build·Release Test를 성공시켰다. Runner `EDAN-TENSOR-WIN`에서 Build는 경고 0개·오류 0개, 테스트는 193개 성공·실패 0개·건너뜀 0개였다.
- PR #10 `최종 main CI 시점을 검증 문서에 반영합니다`는 `965159b5ddc83907ed253671a7d0818930f73633`으로 squash merge되었다. 병합 후 [Actions Run #638](https://github.com/namanvarava/Edan.Tensor/actions/runs/42202248251)은 `EDAN-TENSOR-WIN`에서 Restore·Release Build·Release Test를 성공시켰고, Build는 경고 0개·오류 0개, 테스트는 193개 성공·실패 0개·건너뜀 0개였다.
- PR #11 `Runner 서비스 상태와 최종 CI 증거를 기록합니다`는 `e8ee1ac12f46d7f5afb0f8d166cc2e3b802dfb48`으로 squash merge되었다. 병합 후 [Actions Run #640](https://github.com/namanvarava/Edan.Tensor/actions/runs/42203632340)은 `EDAN-TENSOR-WIN`에서 Restore·Release Build·Release Test를 성공시켰고, Build는 경고 0개·오류 0개, 테스트는 193개 성공·실패 0개·건너뜀 0개였다.
- GitHub Billing의 Budgets and alerts 화면에서 `namanvarava` 계정 Actions 예산은 `$20.00 / $20.00`, 100% 사용, `Usage blocked`로 확인되었다. 예산 증액·해제·정지 설정은 변경하지 않았으며, 이 상태는 향후 Actions 실행을 차단할 수 있는 외부 재개 조건으로 남긴다.
- Windows 서비스 조회에서 `GitHub Actions Runner (namanvarava-Edan.Tensor.EDAN-TENSOR-WIN)`은 `Running`, `StartType=Automatic`으로 확인되었다. Actions 로그의 Runner 이름과 서비스 상태가 일치하며, 작업 수신·실행 및 자동 시작 설정을 모두 확인했다.
- 로컬에서 exact Restore와 Release Build는 종료 코드 0이지만 NuGet 감사 피드 조회 경로에서 `NU1900`이 반복된다. NuGet HTTP 캐시를 비워도 동일했으며, `NuGetAudit=false`를 진단용으로 지정하면 경고 없이 완료되므로 저장소 설정으로 경고를 숨기지 않았다. 원격 Actions의 동일한 Restore와 Build는 경고 0개다.
- 실제 WorkerHost를 임시 SQLite 3개와 Development Migration 설정으로 기동하여 `/health`가 HTTP 200을 반환하는 것을 확인했다. Platform·DataSet·Worker DB가 생성되었고 표준 오류 출력은 없었으며, 검증 후 프로세스를 종료했다.
- 실제 ApiHost도 임시 SQLite 3개로 기동하여 `/health`가 HTTP 200, `/health/database`가 `200 Connected/Green`을 반환하는 것을 확인했다. 인증 없는 Tenant 생성 요청은 HTTP 401을 반환하여 보호 API가 닫힌 상태임을 확인했다.
- 위 실행은 실제 수신 시스템을 포함한 외부 전달 응답 유실 종단 간 시험을 대체하지 않는다. 외부 Identity Provider·키 회전·Commercial/Licensing 업무 계약·WPF·DataSet Validation·DuckDB/Parquet 범위는 승인된 상세 계약과 실행 환경이 없어 완료로 판정하지 않는다.

## main 최종 검증 기록

- PR #5 병합 직후 검증 대상 `main`은 커밋 `4a20a71844d2663b7be338907979dae1aee59446`이며, README에 Release 검증 명령과 인증 경계를 반영한 상태였다.
- [Actions Run #628](https://github.com/namanvarava/Edan.Tensor/actions/runs/42179454874)은 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 모두 성공시켰다.
- Run #628은 총 193개 테스트를 실패 0개·건너뜀 0개로 통과했으며, TRX 덮어쓰기·xUnit2031·NU1900 경고도 확인되지 않았다.
- 당시 열린 Issue와 Pull Request는 0개였으며, 필수 변경 PR #5는 `4a20a71844d2663b7be338907979dae1aee59446`으로 squash merge되었다.
- 문서 정리 PR #6은 `734ca9a94edb610e383b7656bb291e89b3b370b6`으로 squash merge되었고, 병합 후 [Actions Run #630](https://github.com/namanvarava/Edan.Tensor/actions/runs/42180471013)도 동일한 Release 검증을 성공시켰다. Run #630은 총 193개 테스트를 실패 0개·건너뜀 0개로 통과했다.
- 이 절의 판정은 각 커밋에 연결된 원격 Actions 실행 증거에 근거한다. 과거 로컬 exact restore/test 실패와 GitHub CLI 인증 차단은 인수인계 이력으로 남기며, 현재 로컬 Git fetch·동기화 결과는 아래 절에 별도로 기록한다.

## 로컬 main 동기화 후 Release 검증

- 로컬 `main`을 원격 `origin/main`으로 동기화한 뒤 `HEAD`와 `origin/main`은 모두 `058b0346d57626f3e53875d04267ff84ec3a56ec`을 가리키며 작업 디렉터리는 clean 상태가 되었다.
- 로컬에서 exact `dotnet restore EDAN.Tensor.sln`은 종료 코드 0, exact `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`는 종료 코드 0, exact `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`는 종료 코드 0으로 완료했다.
- 로컬 Restore와 Build에는 NuGet 취약성 피드 조회 실패를 나타내는 `NU1900` 경고가 각각 19건 출력되었지만 패키지 복원·컴파일·테스트 자체는 실패하지 않았다. `https://api.nuget.org/v3/index.json`과 취약성 엔드포인트의 기본 HTTPS 조회는 200을 반환했으므로 저장소 설정으로 경고를 숨기지 않고 로컬 NuGet 감사 클라이언트의 원인을 환경 제약으로 기록한다.
- 로컬 Test는 SharedKernel 11, Platform Domain 27 / Application 30 / Infrastructure 12,
  DataSet Domain 6 / Application 8 / Infrastructure 8,
  Worker Domain 21 / Infrastructure 60, API 통합 10으로 총 193개를 실패 0개·건너뜀 0개로 통과시켰다.
- 위 로컬 결과와 별개로 최종 원격 [Actions Run #632](https://github.com/namanvarava/Edan.Tensor/actions/runs/42181072828)은 동일한 Release 검증을 Windows 자체 호스팅 Runner에서 경고 0개로 성공시켰다.

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
- 현재 Runner 실행 증거는 Actions Runner `2.338.2`, 이름 `EDAN-TENSOR-WIN`, 라벨 `self-hosted`, `Windows`, `X64`이다. 이전 제한된 조회에서 서비스 상태를 읽지 못했으나, 이후 읽기 전용 Windows 서비스 조회에서 해당 Runner가 `Running`·`Automatic`으로 확인되었다. Billing 화면도 읽기 전용으로 확인했으며, Actions 예산 `$20.00 / $20.00` 및 `Usage blocked` 상태를 확인했지만 설정은 변경하지 않았다.
- PDF의 Worker 설계 기준은 현재 소스와 자동 검증에 반영되어 있다. `WorkerExecutionRegistry`와 `WorkerTaskDispatcher`는 하나의 종료 인계 예산을 공유하고, Tenant 작업 커서는 Dispatcher 단일 루프의 LRU로 관리하며 기본 10,000, 허용 범위 500–100,000을 사용한다. 관련 직접 회귀 테스트와 실제 프로세스 복구 테스트가 Worker Infrastructure 테스트에 포함된다.
- WorkerHost는 전용 `WorkerDbContext`를 등록하고, DataSet 메타데이터·콘텐츠 저장소를 별도 DbContext와 연결로 등록한다. Platform 계층의 프로젝트 참조와 DataSet 경계도 기존 ArchitectureDependencyTests 및 API 통합 테스트로 확인했다.
- PR #2는 JWT Issuer/Audience 누락 시 검증이 꺼지는 보안 결함, Worker xUnit2031 경고 및 외부 PR의 GitHub-hosted Runner 사용을 수정하고 `af39e318547195d32acbef4c78843da5d4a97a8c`로 squash merge했다. PR #3은 `clean:false` 작업 디렉터리에서 고정 TRX 파일이 충돌하는 경고를 실행 ID별 파일명으로 수정하고 `9de3e2b3a5d5930fbbb780adf24f969f5cee73e0`으로 squash merge했다.
- 최종 main Run #624(`42177862848`)는 Restore, Release Build, Release Test를 모두 성공시켰다. 총 193개 테스트가 실패 0개·건너뜀 0개로 통과했고, 최종 로그에서 TRX 덮어쓰기·xUnit2031·NU1900 경고가 확인되지 않았다.
- 이전 로컬 exact restore/test 실패와 testhost 연결 시간초과는 원격 미동기화·로컬 환경이 정리되기 전의 과거 결과다. 현재 결과는 위 로컬 main 동기화 후 검증 절에 기록했다.
- PDF에 기록된 Billing 화면은 읽기 전용으로 재확인했으며, Actions 예산 차단을 해제하거나 과금을 발생시키는 설정은 변경하지 않았다. 외부 IdP, 실제 외부 전달 수신 시스템, Commercial/Licensing 업무 계약, WPF, DataSet Validation 및 DuckDB/Parquet의 재개 조건은 해당 권한·승인·실행 환경이 제공될 때 처리한다.

## Worker 동기 인계 콜백 시간 제한 수정 및 최신 main 검증

- 병합 전 main 커밋 `a5769c606d60465fb99aac7cf2d64920871b7a6f`의 [Actions Run #658](https://github.com/namanvarava/Edan.Tensor/actions/runs/42221242379)은 Restore와 Build를 성공했지만 `ShutdownReturnsWhenHandoffCallbackBlocksBeforeReturningTask`에서 실패했다. `HandoffElapsed`가 2.406초로 측정되어 테스트의 2초 경계를 넘었으며, Worker Infrastructure는 59개 통과·1개 실패했다.
- 근본 원인은 동기적으로 대기하는 외부 인계 콜백을 `Task.Run`으로 ThreadPool에 배치하여 취소 타이머와 종료 continuation 처리가 함께 지연될 수 있었던 점이다. 테스트 상한을 늘리지 않고, 커밋 `b66bc94aae1d8a4716b67f57fe2e1f3a8f31435f`에서 인계 콜백 시작을 `TaskCreationOptions.LongRunning` 전용 스레드로 변경했다.
- 이 변경은 콜백 자체를 강제 중단하거나 실행 Lease를 해제하지 않는다. `WaitAsync`와 취소 토큰으로 종료 조정기의 반환 시간을 제한하고, 콜백이 뒤늦게 끝나도 기존 Lease 복구 경계를 유지한다.
- PR #21(https://github.com/namanvarava/Edan.Tensor/pull/21)은 자체 호스팅 Runner Check를 통과한 뒤 `6b0113fc14c0f3f9f41937bfb24ebaf18fa2e15c`로 squash merge되었다.
- 병합 전 대상 경계 테스트는 5회 연속 통과했고 Worker Infrastructure 전체 60개도 통과했다. 병합 직전 로컬 exact Restore, Release Build(경고 0개·오류 0개), Release Test(193개 통과·실패 0개·건너뜀 0개)도 완료했다.
- 병합 후 [Actions Run #660](https://github.com/namanvarava/Edan.Tensor/actions/runs/42223986236)은 `EDAN-TENSOR-WIN` 자체 호스팅 Windows Runner(버전 2.338.2)에서 Restore, Build, Test 및 정리를 모두 성공시켰다. Build는 경고 0개·오류 0개였고, 테스트는 SharedKernel 11, Platform 27/30/12, DataSet 6/8/8, Worker 21/60, API 통합 10으로 총 193개를 실패 0개·건너뜀 0개로 통과했다.

## 실제 Host 경계 Smoke 검증

- 최종 main 커밋 `177ffe4caa202e13a694025c666cbe26a01a7514`에서 Windows 로컬 PC의 실제 프로세스로 ApiHost를 Development 환경에서 기동했다. 임시 경로의 Platform·Worker·DataSet 메타데이터·DataSet 콘텐츠 SQLite DB를 사용했으며 저장소나 기본 `App_Data`에는 검증 데이터를 남기지 않았다.
- ApiHost는 네 개 DB의 Migration을 적용한 뒤 `/health`에 HTTP 200과 `Healthy`, `/health/database`에 HTTP 200과 `Connected`·`Green`, `/`에 HTTP 200을 반환했다. 개발 환경 OpenAPI `/openapi/v1.json`도 HTTP 200을 반환했고, 보호된 `/api/platform/tenants/mine`은 토큰 없이 HTTP 401을 반환하여 인증 경계를 확인했다.
- 같은 방식으로 WorkerHost를 별도 실제 프로세스로 기동했다. 전용 Platform·Worker·DataSet 메타데이터·콘텐츠 DB의 Migration과 Handler catalog 동기화를 수행한 뒤 `/health`에 HTTP 200과 `Healthy`를 반환했다.
- 이 Smoke 검증은 실제 외부 Identity Provider, JWKS·키 회전, 외부 전달 수신 시스템의 응답 유실 종단 간 시험을 포함하지 않는다. 해당 검증은 외부 시스템과 승인된 운영 계약이 제공될 때 재개한다.


