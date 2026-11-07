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

- PR #22 병합 직후 main 커밋 `177ffe4caa202e13a694025c666cbe26a01a7514`에서 Windows 로컬 PC의 실제 프로세스로 ApiHost를 Development 환경에서 기동했다. 임시 경로의 Platform·Worker·DataSet 메타데이터·DataSet 콘텐츠 SQLite DB를 사용했으며 저장소나 기본 `App_Data`에는 검증 데이터를 남기지 않았다.
- ApiHost는 네 개 DB의 Migration을 적용한 뒤 `/health`에 HTTP 200과 `Healthy`, `/health/database`에 HTTP 200과 `Connected`·`Green`, `/`에 HTTP 200을 반환했다. 개발 환경 OpenAPI `/openapi/v1.json`도 HTTP 200을 반환했고, 보호된 `/api/platform/tenants/mine`은 토큰 없이 HTTP 401을 반환하여 인증 경계를 확인했다.
- 같은 방식으로 WorkerHost를 별도 실제 프로세스로 기동했다. 전용 Platform·Worker·DataSet 메타데이터·콘텐츠 DB의 Migration과 Handler catalog 동기화를 수행한 뒤 `/health`에 HTTP 200과 `Healthy`를 반환했다.
- 이 Smoke 검증은 실제 외부 Identity Provider, JWKS·키 회전, 외부 전달 수신 시스템의 응답 유실 종단 간 시험을 포함하지 않는다. 해당 검증은 외부 시스템과 승인된 운영 계약이 제공될 때 재개한다.
- PR #23 병합 커밋 `f2176620e70e8af54858f6fc82526948a81aa98a`에 대한 [Actions Run #664](https://github.com/namanvarava/Edan.Tensor/actions/runs/42226473641)도 `EDAN-TENSOR-WIN`에서 Restore·Build·Test·정리를 모두 성공시켰다. Build는 경고 0개·오류 0개였고 전체 테스트 193개가 실패 0개·건너뜀 0개로 통과했다.

## PR #24 병합 후 최신 main 재검증

- PR #24 [Host smoke 검증의 기준 커밋과 Run 증거를 명확히 합니다](https://github.com/namanvarava/Edan.Tensor/pull/24)는 자체 호스팅 Runner Check를 통과한 뒤 `5f92420fd3a6f28d049d3c7407053026f23961b7`로 squash merge되었다.
- 병합 후 원격 `main`과 로컬 `main`, `origin/main`은 모두 `5f92420fd3a6f28d049d3c7407053026f23961b7`을 가리킨다. 작업 디렉터리는 clean 상태이며 `git diff --check`도 종료 코드 0으로 완료했다.
- 병합 후 [Actions Run #666](https://github.com/namanvarava/Edan.Tensor/actions/runs/42227367592)은 해당 SHA에서 `success`로 종료되었다. `build-and-test` 작업은 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 실행되었고, Restore·Release Build·Release Test와 정리 단계를 모두 성공시켰다. Build는 경고 0개·오류 0개였고, 테스트는 SharedKernel 11, Platform Domain 27 / Application 30 / Infrastructure 12, DataSet Domain 6 / Application 8 / Infrastructure 8, Worker Domain 21 / Infrastructure 60, API 통합 10으로 총 193개를 통과했으며 실패 0개·건너뜀 0개였다.
- 같은 SHA에서 Windows 로컬 PC로 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 다시 실행했다. 세 명령은 모두 종료 코드 0이며, 로컬 테스트도 193개 통과·실패 0개·건너뜀 0개였다.
- 현재 Runner API는 `EDAN-TENSOR-WIN`을 `online`·`idle` 및 `self-hosted`, `Windows`, `X64` 레이블로 보고한다. Windows 서비스 `actions.runner.namanvarava-Edan.Tensor.EDAN-TENSOR-WIN`도 `Running`·`Auto` 상태다. 워크플로에는 `schedule`, `cron`, `workflow_dispatch` 예약 트리거가 없다.
- 현재 열린 Issue와 Pull Request는 각각 0개다. `main` 보호 규칙 REST 조회는 비공개 저장소 요금제 제한으로 HTTP 403을 반환하므로 독립적인 규칙 JSON 검증은 수행할 수 없었다. 이 제한을 우회하지 않았으며, PR #24의 Check는 `pass`, 병합 상태는 `MERGED`로 확인했다.

## DataSet null 어댑터 입력 결함 수정 및 재검증

- `0f5eb3a70629620cb32f0f9786c560618a65cb7f` 기준에서 `storageAdapter: null` 입력을 전달하는 회귀 테스트를 추가하여 `DataSetManagementService.CreateAsync`의 `adapterId.Trim()`에서 `NullReferenceException`이 발생하는 것을 재현했다. 이 오류는 잘못된 요청을 HTTP 500으로 전파할 수 있고, 어댑터 입력 검증 전에 중단되는 문제였다.
- Application 서비스 경계에서 어댑터 식별자를 `ArgumentException.ThrowIfNullOrWhiteSpace`로 먼저 검증했다. API 계층만 수정하는 대안은 다른 Application 호출자를 보호하지 못하므로 적용하지 않았으며, 기존 `dataset.validation` 오류 계약과 저장소 할당 순서를 유지했다.
- PR #26 [DataSet null 어댑터 입력을 안전하게 거부합니다](https://github.com/namanvarava/Edan.Tensor/pull/26)는 자체 호스팅 Check를 통과한 뒤 `f6f692b2d5a5276026cb4ec616948a63944435b2`로 squash merge되었다. 단위 회귀 테스트는 잘못된 입력에서 실패 결과를 반환하고 저장소 할당을 0회로 유지하며, ApiHost 통합 테스트는 HTTP 400과 `dataset.validation` 코드를 확인한다.
- 수정 후 DataSet Application 전체 9개와 ApiHost 통합 전체 10개가 통과했다. 병합 후 로컬 exact Restore·Release Build·Release Test는 모두 종료 코드 0이며 전체 테스트는 194개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #670](https://github.com/namanvarava/Edan.Tensor/actions/runs/42231001719)은 `f6f692b2d5a5276026cb4ec616948a63944435b2`에서 `EDAN-TENSOR-WIN`으로 성공했다. Build는 경고 0개·오류 0개였고 전체 테스트 194개가 통과했다. 이 회귀 테스트는 이후 어댑터 입력 처리에서 null·공백 검증이 제거되는 경우를 자동으로 탐지한다.

## Run #672 Worker 종료 테스트 실패 원인과 수정

- PR #27 병합 후 `b9941e8b3e271d1bd2d95579c2a16f0db900db14`에서 실행한 [Actions Run #672](https://github.com/namanvarava/Edan.Tensor/actions/runs/42231748053)은 Restore·Build와 나머지 테스트를 통과했지만 `WorkerExecutionRegistryTests.ShutdownReturnsWhenHandoffCallbackIgnoresCancellation` 1개가 실패했다. Worker Infrastructure는 59개 통과·1개 실패였고 전체 실행은 실패로 판정했다.
- 실패한 줄은 `HandoffElapsed <= 350ms`라는 절대 시간 단정이었다. `StopAsync`는 250ms 인계 취소 예산을 사용하지만, `HandoffElapsed`에는 LongRunning 스레드 시작과 테스트 호스트 continuation의 스케줄링 지연도 포함된다. 따라서 제품의 종료 예산 계산 결함이 아니라 자체 호스팅 테스트 동시 실행에 민감한 테스트 경계 결함으로 분류했다.
- 로컬에서 동일 대상 테스트를 20회, Worker Infrastructure 전체를 5회 반복하여 모두 통과했지만, Run #672의 실제 실패 로그가 존재하므로 단순 재시도로 무시하지 않았다. 제품 timeout을 늘리면 실제 종료 계약을 약화시키고, `HandoffElapsed`를 인위적으로 자르면 관측값을 왜곡하므로 두 대안을 적용하지 않았다.
- 테스트는 외부 콜백이 취소를 무시해도 `shutdown.WaitAsync(TimeSpan.FromSeconds(1))` 안에 반환되는 의미를 유지하고, 스케줄링 지연이 포함된 `HandoffElapsed`의 350ms 절대 상한만 제거했다. 음수가 아닌 측정값과 콜백 해제·Lease 정리 절차는 그대로 검증한다.
- 수정 후 대상 테스트 1개와 Worker Infrastructure 전체 60개가 통과했으며, 이 회귀 테스트는 이후 종료 호출이 1초 관찰 상한을 넘거나 콜백 정리가 누락되는 경우를 계속 탐지한다.
- PR #28 [Worker 종료 테스트의 시간 경계를 안정화합니다](https://github.com/namanvarava/Edan.Tensor/pull/28)는 자체 호스팅 Check를 통과한 뒤 `fef93883e45bf281123ef720eab006f2bc295624`로 squash merge되었다. 병합 후 로컬 exact Restore·Release Build·Release Test는 모두 종료 코드 0이며 전체 테스트는 194개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #674](https://github.com/namanvarava/Edan.Tensor/actions/runs/42233142705)은 `fef93883e45bf281123ef720eab006f2bc295624`에서 `EDAN-TENSOR-WIN`으로 성공했다. Build는 경고 0개·오류 0개였고 전체 테스트 194개가 통과했다. Run #672의 유일한 테스트 실패는 이 수정으로 해결되었으며, 실패를 무시하거나 테스트 실행을 생략하지 않았다.

## 최종 main Host Smoke 재검증

- 문서 PR #29 병합 후 최종 main 커밋 `e04d15d952d1b95d3e94c61dc96aaf4da0602e3b`에서 ApiHost와 WorkerHost를 Windows 로컬 PC의 실제 프로세스로 다시 기동했다. 각 Host는 임시 Platform·Worker·DataSet 메타데이터·DataSet 콘텐츠 SQLite DB와 임시 아카이브 경로를 사용했으며 검증 후 모두 제거했다.
- ApiHost는 `/health`, `/health/database`, `/`, `/openapi/v1.json`에서 HTTP 200을 반환했고, `/health/database`는 `Connected`·`Green`을 반환했다. 인증 없는 `/api/platform/tenants/mine`은 HTTP 401을 반환하여 보호 API 경계를 유지했다.
- WorkerHost는 `/health`에서 HTTP 200과 `Healthy`를 반환했다. 두 프로세스 모두 표준 오류 출력이 없었고 임시 경로 정리를 확인했다.
- 이 검증은 외부 Identity Provider·JWKS·키 회전과 외부 전달 수신 시스템의 응답 유실 E2E를 포함하지 않는다. 해당 항목은 승인된 운영 계약과 외부 실행 환경이 제공될 때 별도로 검증한다.

## 최종 main CI 및 개발 범위 판정

- Host Smoke 문서 PR #30을 squash merge한 최종 main 커밋은 `1816fc07a3eb5dbc695a453ee1aefdb1446a729a`이다. 병합 후 [Actions Run #678](https://github.com/namanvarava/Edan.Tensor/actions/runs/42234816562)은 이 SHA에서 `success`로 종료되었다.
- Run #678의 `build-and-test` 작업은 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`(Runner `2.338.2`)에서 실행되었다. Restore·Release Build·Release Test·정리 단계가 모두 성공했고, Build는 경고 0개·오류 0개였다.
- Run #678의 테스트 결과는 SharedKernel 11, Platform Domain 27 / Application 30 / Infrastructure 12, DataSet Domain 6 / Application 9 / Infrastructure 8, Worker Domain 21 / Infrastructure 60, API 통합 10으로 총 194개 통과·실패 0개·건너뜀 0개였다.
- 같은 최종 SHA의 Windows 로컬 PC에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 실행했다. Restore·Build·Test는 모두 종료 코드 0이며 로컬 테스트도 194개 통과·실패 0개·건너뜀 0개였다.
- 이 기록은 현재 구현된 Platform·DataSet·Worker·Host 경계와 회귀 검증의 완료 증거다. README에 명시된 실제 Identity Provider·키 회전·외부 전달 응답 유실 E2E, DataSet Validation 승인 계약·Validation Handler·DuckDB·Parquet, Commercial·Licensing 업무 흐름, WPF 클라이언트는 승인된 계약 또는 독립 실행 환경이 없으므로 제품 전체 개발 완료로 판정하지 않는다. 해당 계약과 실행 환경이 제공되면 각 범위의 수용 기준과 실제 경계 시험을 추가하여 재개한다.

## 보안 의존성과 인수인계 자료 재검증

- 최종 main에서 `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`를 실행했다. NuGet 공개 피드와 현재 구성된 패키지 소스를 기준으로 모든 솔루션 프로젝트에 알려진 취약 패키지가 없었고 종료 코드는 0이었다.
- 인수인계 PDF는 2페이지·A4·비암호화 문서로 다시 확인했고, 두 페이지를 PNG로 렌더링하여 제목·섹션·Worker 설계·Run #605 예산 차단 기록의 배치와 가독성을 검토했다. Poppler가 `Symbol` 및 `ArialUnicode` 표시 글꼴 경고를 출력했지만 페이지의 잘림·겹침·검은 사각형은 확인되지 않았다. PDF 원본은 변경하지 않았다.
- Actions 예산 제한, 외부 Identity Provider·키 회전·외부 전달 수신 시스템, 승인 전 DataSet Validation·Commercial·Licensing·WPF 범위는 이 검증으로 해결되지 않으며 기존 재개 조건을 유지한다.

## 2026-11-07 승인 범위 구현 및 로컬 경계 검증

- 사용자가 `dataset-validation-v1-proposal.md`를 승인된 계약으로 확정했다. 이번 릴리스의 DataSet 검증은 UTF-8 CSV와 JSON 객체 배열, 필수값·문자열·정수·소수·불리언 형식, 수치 범위·중복 검사, 최상위 필드, Tenant별 불변 프로필 ID·revision, 결과 자동 삭제 금지와 관리자 명시적 삭제, SQLite 어댑터만 포함한다.
- DataSet Validation Handler와 API는 불투명 실행 참조를 사용한다. 실행은 DataSet·Schema·프로필 revision을 고정하고 진행 위치·상세 상한을 저장하며, 결과에 원문 셀 값·연결 문자열·물리 경로를 기록하지 않는다. DuckDB·Parquet, Commercial·Licensing 업무 흐름·결제·권리·만료, WPF 클라이언트는 이번 릴리스에서 제외한다.
- 외부 인증 자동 검증은 저장소에 비밀을 기록하지 않는 로컬 IdP/JWKS TCP 서버로 정상 토큰, 잘못된 Issuer/Audience, 만료 토큰, 서명 실패, 알 수 없는 `kid`, 기존 키와 새 키의 겹침 회전을 확인했다. 최종 비운영 외부 IdP 테스트 테넌트는 아직 제공되지 않아 실제 외부 경계 검증은 미실행이다.
- 외부 전달은 실제 Windows loopback TCP 수신기에서 첫 응답 유실 후 동일 Idempotency Key 재시도를 확인했다. 같은 Worker 인프라 테스트 묶음에서 HTTP 5xx, timeout, 응답 본문 미보관, Tenant 격리를 확인했고, API 통합 테스트에서 opaque Worker 참조와 멱등 생성 경계를 확인했다.
- PR #33 병합 시점의 최신 변경 기준 로컬 Windows에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 실행했다. Restore·Build·Test는 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 208개 통과·실패 0개·건너뜀 0개였다.
- 최신 Release 산출물의 실제 Windows Host Smoke에서 ApiHost `/health`, `/health/database`, `/`, `/openapi/v1.json`은 HTTP 200, 보호 API는 HTTP 401을 반환했고 WorkerHost `/health`는 HTTP 200을 반환했다. 두 Host의 표준 오류 출력은 비어 있었고 프로세스와 임시 DB 디렉터리를 정리했다.
- 위 내용은 이전 절의 “승인되지 않은 DataSet Validation” 기록을 현재 승인 시점부터 대체한다. 외부 IdP 테스트 테넌트의 Issuer·Audience·Metadata 주소·테스트 주체 소속·키 회전 권한이 제공되면 같은 토큰 경계를 실제 외부 환경에서 재실행한다.

## 2026-11-07 인수인계 PDF 및 PR #34 병합 후 최종 재검증

- 인수인계 PDF의 기준 main SHA `c187103cee76ad58e28633a259aeebc5e7dab75c`와 현재 main SHA `72cf580b1368e3273246a46e80f7801b7c5e14cb`를 대조했다. PDF의 Worker 종료 인계·Tenant 작업 커서 LRU·전용 Worker DbContext·DataSet 저장소 분리·Platform 의존성 경계 요구를 현재 코드와 테스트에서 재확인했다.
- PDF가 기록한 과거 Actions Run #605(`42124026661`)는 `runner_id=0`, 실행 단계 없음으로 종료된 예산 차단 이력이었다. 현재는 예산 설정을 변경하지 않았고, 최신 Run #686이 실제 Runner에서 성공하여 동일 차단은 재현되지 않았다. Billing REST 조회는 현재 계정에서 HTTP 404를 반환했으며, 예산 증액·사용 중지 옵션 변경은 수행하지 않았다.
- PR #34 [Worker Tenant 커서 상한 계약의 회귀 검증을 보강합니다](https://github.com/namanvarava/Edan.Tensor/pull/34)는 자체 호스팅 PR Run #685를 통과한 뒤 `72cf580b1368e3273246a46e80f7801b7c5e14cb`로 squash merge되었다. 새 테스트는 설정 기본값 10,000, 최소값 500, 최대값 100,000, LRU 퇴출 및 stale entry 제거를 직접 검증한다.
- 병합 후 로컬 Windows에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 실행했다. 세 명령은 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 210개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #686](https://github.com/namanvarava/Edan.Tensor/actions/runs/42266713040)은 `EDAN-TENSOR-WIN`(Runner 2.338.2, runner_id=22)에서 Restore·Release Build·Release Test·정리를 모두 성공시켰다. Build 경고 0개·오류 0개, 테스트는 SharedKernel 11, Platform Domain 27 / Application 30 / Infrastructure 12, DataSet Domain 6 / Application 14 / Infrastructure 9, Worker Domain 21 / Infrastructure 66, API 통합 14로 총 210개 통과·실패 0개·건너뜀 0개였다.
- 병합된 최신 Release 산출물의 실제 Windows Host Smoke에서 ApiHost `/health`, `/health/database`, `/`, `/openapi/v1.json`은 HTTP 200, 보호 API는 HTTP 401을 반환했고 WorkerHost `/health`는 HTTP 200을 반환했다. API 데이터베이스는 `Connected/Green`이었고 두 프로세스의 표준 오류는 비어 있었으며 임시 프로세스·SQLite 디렉터리를 정리했다.
- 최종 상태는 로컬 main·origin/main·GitHub main이 모두 `72cf580b1368e3273246a46e80f7801b7c5e14cb`이고 작업 디렉터리가 clean이며 열린 PR·Issue가 없다. Runner는 온라인·Idle이고 Windows 서비스는 `Running/Auto` 상태다.
- 외부 비운영 IdP 테스트 테넌트의 실제 경계 검증만 환경 제공 전까지 남아 있다. Metadata/JWKS 주소, Issuer, Audience, Tenant 소속 테스트 주체, 키 회전 권한이 제공되면 정상 토큰·잘못된 Issuer/Audience·만료·서명 실패·알 수 없는 `kid`·키 회전을 외부 환경에서 재실행한다.

## 2026-11-07 DataSet 검증 Fencing Token 결함 수정 후 main 재검증

- PR #37 [DataSet 검증 결과에 Fencing Token을 적용합니다](https://github.com/namanvarava/Edan.Tensor/pull/37)는 필수 Check를 통과한 뒤 `2e2a49efdac38417037b7072c1220af17eeb8afb`로 squash merge되었다.
- 기존 결함은 DataSet 검증 결과 저장소가 Worker의 Fencing Token을 받지 않아, 새 Worker가 같은 실행을 인계한 뒤에도 이전 실행이 결과·상세·상태를 기록할 수 있었던 점이다. 실행 레코드에 Token을 저장하고 시작·행 진행·완료·실패·재시도 대기 전환을 Token 및 낙관적 동시성으로 보호했다.
- `20261107120000_AddDataSetValidationFencing` Migration과 오래된 Token의 결과 쓰기 거부 회귀 테스트를 추가했다. DataSet Infrastructure 테스트는 10개, 전체 Release 테스트는 211개이며 모두 실패 0개·건너뜀 0개로 통과했다.
- 병합 후 로컬 Windows에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 실행했다. 세 명령은 모두 종료 코드 0이고 Build는 경고 0개·오류 0개였다.
- 병합 후 [Actions Run #692](https://github.com/namanvarava/Edan.Tensor/actions/runs/42326121824)은 `EDAN-TENSOR-WIN` Runner 버전 `2.338.2`에서 exact Restore·Release Build·Release Test를 성공시켰다. 로그는 Build 경고 0개·오류 0개와 전체 211개 통과·실패 0개·건너뜀 0개를 기록한다.
- 병합 후 실제 Windows Host Smoke는 임시 SQLite 4개로 ApiHost와 WorkerHost를 기동하여 ApiHost `/health`, `/health/database`, `/`, `/openapi/v1.json`과 WorkerHost `/health`에 HTTP 200, 보호 API에 HTTP 401을 확인했다. 두 프로세스 stderr는 각각 0자였고 임시 DB·프로세스를 정리했다.
- 현재 범위에서 남은 필수 외부 검증은 비운영 `Edan.Identity` 테스트 Tenant 경계뿐이다. 실제 Metadata/JWKS 주소, 기대 Issuer/Audience, Tenant 소속 테스트 사용자와 키 회전 권한이 제공되기 전에는 외부 IdP 검증을 완료로 표시하지 않는다. 토큰과 비밀키는 저장소·문서·로그에 기록하지 않는다.

## 2026-11-07 최종 문서 기준 정정 및 main 재검증

- PR #39 [테스트 증거의 기준 커밋을 명확히 기록합니다](https://github.com/namanvarava/Edan.Tensor/pull/39)는 `50872843d77704458f23c4d873defea22bd0e73e`로 squash merge되었다. `tests/README.md`는 DataSet Fencing Token 테스트의 코드 기준 커밋 `2e2a49efdac38417037b7072c1220af17eeb8afb`와 최신 main 커밋을 구분하도록 정정했다.
- 병합 후 실제 Windows에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. 세 명령은 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 211개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #696](https://github.com/namanvarava/Edan.Tensor/actions/runs/42327146981)은 `EDAN-TENSOR-WIN`에서 exact Restore·Release Build·Release Test를 성공시켰다. 로그는 Build 경고 0개·오류 0개와 전체 211개 통과·실패 0개·건너뜀 0개를 기록한다.
- PR #39 병합 시점의 main 기준 SHA는 `50872843d77704458f23c4d873defea22bd0e73e`이며, 외부 비운영 `Edan.Identity` Metadata/JWKS 경계 검증만 실제 주소·테스트 Tenant·키 회전 권한 제공 전까지 남아 있다. 토큰·비밀키·비밀번호는 저장소와 검증 기록에 기록하지 않는다.

## 2026-11-07 개발 Bypass 및 External 인증 모드 구성

- 개인 개발에서 도메인이나 외부 IdP가 없어도 `src/Hosts/EDAN.Tensor.ApiHost/appsettings.Development.json`의 `Authentication:Mode=Bypass`로 실행할 수 있도록 구성했다. Bypass는 외부 JWT 검증만 생략하고 Tenant 소속·조직 범위·관리자 권한 검사는 유지하며, Development가 아닌 환경에서는 시작 단계에서 거부한다.
- `Authentication:Mode=External`은 MetadataAddress, Issuer, Audience, JWKS 및 HTTPS 정책을 적용한다. MetadataAddress와 Issuer는 실제 도메인 또는 개발망 IP 주소를 환경 변수로 받을 수 있으며, HTTP Metadata는 Development에서만 허용한다.
- 새 인증 설정 테스트 5개를 포함한 로컬 Release 전체 테스트는 214개 통과·실패 0개·건너뜀 0개였고 Build 경고 0개·오류 0개였다. 실제 Windows ApiHost Smoke는 Development Bypass에서 health 200·보호 API 200, External과 IP Metadata 설정에서는 health 200·토큰 없는 보호 API 401을 반환했으며 두 실행의 stderr는 0자였다. Bypass 회귀 테스트는 구성된 안정적인 `sub`와 `platform_admin` 역할이 실제 인증 Claim으로 발급되는지도 확인한다.
- 외부 비운영 IdP 자체 검증은 실제 Metadata/JWKS 주소, 기대 Issuer/Audience, 테스트 Tenant·사용자·opaque `sub`, 키 회전 권한이 제공된 뒤 재실행한다. 토큰과 비밀정보는 저장소·문서·로그에 기록하지 않는다.

## 2026-11-07 PR #43 Worker 종료 경계 테스트 실패와 수정

- PR #43의 첫 CI인 Actions Run #703(`42340222653`)은 `EDAN-TENSOR-WIN` Runner 22에서 Restore와 Release Build를 성공시켰지만 Test 단계에서 `WorkerExecutionRegistryTests.ShutdownReturnsWhenHandoffCallbackIgnoresCancellation`가 `handoffStarted.Task.WaitAsync(TimeSpan.FromSeconds(5))`에서 `TimeoutException`을 반환했다. 새 Bypass Claim 테스트가 아니라 기존 Worker 종료 테스트의 실패였으며, 나머지 테스트는 통과했다.
- 실패 로그와 소스를 대조한 결과, 해당 테스트는 다른 Worker 인프라 테스트와 병렬 실행될 때 10밀리초 grace 타이머와 250밀리초 취소 타이머의 ThreadPool continuation 스케줄링에 의존했다. 제품의 종료 예산 계산 오류나 외부 환경 장애로 분류하지 않았다.
- 제품의 250밀리초 인계 예산을 늘리거나 테스트 대기 상한을 무조건 늘리는 대안은 종료 계약을 약화하거나 관측을 흐리므로 적용하지 않았다. 테스트는 grace 대기 없이 인계를 시작하고, 시간 경계 테스트 컬렉션을 다른 Worker 인프라 테스트와 병렬 실행하지 않도록 격리했다. 이 변경은 제품 코드와 CI Workflow를 바꾸지 않으며, 취소 무시 콜백·실행 Lease 정리·인계 실패 결과 검증은 그대로 유지한다.
- 수정 후 대상 테스트를 20회 연속 실행하여 모두 통과했고, 로컬 Windows에서 Release 전체 테스트 214개가 실패 0개·건너뜀 0개로 통과했다. PR #43의 수정 커밋은 동일 Runner의 Restore·Build·Test Check를 통과한 뒤 병합되었다.

## 2026-11-07 PR #43 병합 후 최종 main 검증

- PR #43은 필수 Check가 `pass`이고 병합 상태가 `CLEAN`인 것을 확인한 뒤 `874ec208e06bc08cc25fb7a5d09f547f9732fa0a`로 Squash Merge했다. 로컬 `main`, `origin/main` 및 원격 `refs/heads/main`은 모두 같은 SHA를 가리키며 작업 디렉터리는 clean 상태다.
- 병합 후 push Actions Run #705(`42341308727`)는 `EDAN-TENSOR-WIN` Runner 22(`self-hosted`, `Windows`, `X64`)에서 Restore·Release Build·Release Test를 모두 성공시켰다. Build는 경고 0개·오류 0개였고 테스트는 214개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 Windows 로컬에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 실행했다. 세 명령은 모두 종료 코드 0이며 동일하게 214개 테스트가 통과했다.
- 병합된 PR #43과 기존 PR #42를 포함해 열린 Pull Request와 Issue는 각각 0개다. Runner는 `online`·`busy=false`이고 Windows 서비스는 `Running`·`Auto`이며, Workflow에는 `schedule`, `cron`, `workflow_dispatch` 예약 트리거가 없다.
- 실제 비운영 외부 IdP의 Metadata/JWKS·Issuer·Audience·Tenant 테스트 주체·키 회전 권한이 없는 상태이므로 외부 경계 검증은 완료로 표시하지 않는다. GitHub 비공개 저장소의 main 보호 규칙 REST 조회는 요금제 제한 HTTP 403으로 계속 독립 확인할 수 없다.

## 2026-11-07 개발 Bypass 재검증 및 최신 main CI 확인

- 현재 로컬 `main`, `origin/main`과 GitHub `main`의 기준 커밋은 `b06fa8bfca3f88aa77a2fb34ed963a1a394df7d1`이며 작업 디렉터리는 clean 상태다. 이번 UI 관련 요청은 Edan.Tensor 저장소에 반영하지 않았다.
- 개인 개발 설정은 `src/Hosts/EDAN.Tensor.ApiHost/appsettings.Development.json`의 `Authentication:Mode=Bypass`를 사용한다. Bypass는 외부 JWT 검증만 생략하고 Tenant 소속·조직 범위·관리자 권한 검사를 유지하며, `Development`가 아닌 환경에서는 등록 단계에서 거부된다.
- Windows 로컬에서 Release 기준 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 실행했다. Restore·Build·Test는 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 214개 통과·실패 0개·건너뜀 0개였다. 인증 설정 통합 테스트는 17개 모두 통과했다.
- Release ApiHost를 실제 Windows 프로세스로 임시 SQLite 4개와 `Development` 환경에서 기동했다. `/health`, `/health/database`, `/`, `/openapi/v1.json`은 모두 HTTP 200을 반환했고, Bypass 인증이 적용된 `/api/platform/tenants/mine`도 HTTP 200을 반환했다. `/health/database`는 `Connected`·`Green`이었고 stderr는 0자였으며, 검증 후 임시 프로세스·데이터베이스·아카이브 디렉터리를 제거했다.
- 최신 main 커밋의 [Actions Run #707](https://github.com/namanvarava/Edan.Tensor/actions/runs/42341723020)은 `EDAN-TENSOR-WIN` Runner 22(`self-hosted`, `Windows`, `X64`)에서 실행되어 Restore·Build·Test 단계를 모두 성공시켰다. 이 실행의 head SHA는 `b06fa8bfca3f88aa77a2fb34ed963a1a394df7d1`이다. Workflow에는 `schedule`, `cron`, `workflow_dispatch` 예약 트리거가 없다.
- 현재 GitHub 조회에서 열린 Issue와 PR은 각각 0개다. `branches/main` 응답은 `protected=false`를 반환하지만, 별도 main 보호 규칙 API는 HTTP 403으로 규칙 세부 JSON을 독립 조회할 수 없다. 이 제한은 변경하지 않았고 보호 정책을 우회하지 않았다.
- 실제 비운영 외부 IdP의 Metadata/JWKS 주소, 기대 Issuer·Audience, `edan-nonprod` Tenant·테스트 사용자·안정적인 `sub`, 키 회전 권한은 아직 제공되지 않았다. 따라서 이번 Bypass 검증은 개인 개발 실행을 가능하게 한 증거이며, 정상 토큰·잘못된 Issuer/Audience·만료·서명 실패·알 수 없는 `kid`·외부 키 회전의 실제 IdP 경계 검증 완료를 의미하지 않는다.

## 2026-11-07 PR #45 병합 후 최종 main 재검증

- 검증 증거 문서 PR #45는 자체 호스팅 Check 성공 후 `7f0fc64d1279eed27e3730e2fc744f613f993260`으로 Squash Merge되었다. 병합 후 로컬 `main`, `origin/main`과 GitHub `main`은 이 SHA를 가리키고 작업 디렉터리는 clean 상태다.
- 병합 후 Windows 로컬에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. Restore·Build·Test는 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 214개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #709](https://github.com/namanvarava/Edan.Tensor/actions/runs/42360371388)은 head SHA `7f0fc64d1279eed27e3730e2fc744f613f993260`에서 `EDAN-TENSOR-WIN` Runner 22(`self-hosted`, `Windows`, `X64`)로 실행되었다. Restore·Build·Test와 정리 단계가 모두 성공했다.
- 현재 열린 Issue와 PR은 각각 0개이며, 예약 실행 트리거는 없다. `branches/main` 조회는 `protected=false`를 반환하지만 별도 보호 규칙 API는 HTTP 403으로 세부 규칙을 읽을 수 없다. 이 환경적 제한은 변경하거나 우회하지 않았다.
- 개발 Bypass의 실제 Windows Host Smoke와 외부 IdP가 없는 개인 개발 경계는 앞 절의 증거로 확인되었다. 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·Tenant·opaque `sub`·키 회전 검증은 해당 외부 환경과 권한이 제공될 때까지 미완료로 유지한다.

## 2026-11-07 PR #46 병합 후 최종 main 재검증

- 최종 검증 증거 문서 PR #46은 자체 호스팅 Check 성공 후 `49aca0f7d15f536f614f8720674f7a19a35b7275`로 Squash Merge되었다. 병합 후 로컬 `main`, `origin/main`과 GitHub `main`은 이 SHA를 가리키고 작업 디렉터리는 clean 상태다.
- 병합 후 Windows 로컬에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. Restore·Build·Test는 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 214개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #711](https://github.com/namanvarava/Edan.Tensor/actions/runs/42360939566)은 head SHA `49aca0f7d15f536f614f8720674f7a19a35b7275`에서 `EDAN-TENSOR-WIN` Runner 22(`self-hosted`, `Windows`, `X64`)로 실행되었다. Restore·Build·Test와 정리 단계가 모두 성공했다.
- 현재 열린 Issue와 PR은 각각 0개이며, Workflow에는 `schedule`, `cron`, `workflow_dispatch` 예약 트리거가 없다. `branches/main` 조회는 `protected=false`를 반환하지만 별도 main 보호 규칙 API는 HTTP 403으로 세부 규칙을 읽을 수 없다. 이 환경적 제한은 변경하거나 우회하지 않았다.
- 개발 Bypass는 외부 IdP가 없는 Development 환경에서만 허용되고, External 모드는 Issuer·Audience·Metadata·JWKS·만료·서명 검증을 유지한다. 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·Tenant·opaque `sub`·키 회전 검증은 해당 외부 환경과 권한이 제공될 때까지 미완료로 유지한다.

## 2026-11-07 최신 main Host Smoke 실행 경로 재검증

- 최신 Release 산출물을 저장소 루트에서 DLL로 직접 실행한 첫 Smoke에서 `/health`는 HTTP 200이었지만 보호 API는 HTTP 401이었다. 표준 오류는 0자였고, 출력 디렉터리와 소스의 `appsettings.Development.json`에는 `Authentication:Mode=Bypass`가 동일하게 존재했다.
- 로그와 실행 코드를 대조한 결과, 저장소 루트를 작업 디렉터리로 사용하여 ASP.NET Core Content Root가 호스트 출력 디렉터리가 되지 않았고, `appsettings.Development.json`을 구성 공급자로 읽지 못한 것이 원인이었다. 제품 인증 코드나 Bypass 정책의 결함으로 분류하지 않았다.
- 구성 파일을 복사하거나 External 검증을 끄는 대안은 실행 환경과 제품 경계를 흐리므로 적용하지 않았다. Smoke 프로세스의 작업 디렉터리를 `src/Hosts/EDAN.Tensor.ApiHost/bin/Release/net10.0`으로 지정하는 최소 실행 수정만 적용했다.
- 교정 후 실제 Windows 프로세스에서 `/health`, `/health/database`, `/`, `/openapi/v1.json`, Bypass 보호 API가 모두 HTTP 200을 반환했고 데이터베이스 상태는 `Connected`·`Green`, 표준 오류는 0자였다. 임시 프로세스·SQLite·아카이브 디렉터리는 검증 후 제거했다.
- 재발 방지를 위해 이후 Host Smoke는 DLL이 있는 호스트 출력 디렉터리를 작업 위치로 사용하고, `appsettings.Development.json` 로드 여부를 Bypass 보호 API 상태와 함께 확인한다. 최신 main Release 전체 테스트는 214개 통과·실패 0개·건너뜀 0개로 유지되었다.

## 2026-11-07 PR #49 보안 경계 회귀 테스트 병합 후 재검증

- PR #49 [Production HTTPS 인증 경계 회귀 테스트를 추가합니다](https://github.com/namanvarava/Edan.Tensor/pull/49)는 자체 호스팅 Check 성공 후 `6f0fa8b674dbe2819a9a22b56f706925c91c17f8`로 Squash Merge되었다.
- `External` 인증이 Development가 아닌 환경에서 `RequireHttpsMetadata=false`와 HTTP Metadata 주소를 거부하는 회귀 테스트를 추가했다. Development Bypass와 External JWT 검증 구현은 변경하지 않았다.
- 병합 후 Windows 로컬에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실행했다. Restore·Build·Test는 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 215개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #717](https://github.com/namanvarava/Edan.Tensor/actions/runs/42363000487)은 `EDAN-TENSOR-WIN` Runner 22(`self-hosted`, `Windows`, `X64`)에서 Restore·Build·Test와 정리 단계를 모두 성공시켰다.
- 최신 Release ApiHost의 실제 Windows Bypass Smoke에서 `/health`, `/health/database`, `/`, `/openapi/v1.json`, 보호 API가 모두 HTTP 200을 반환했고 데이터베이스는 `Connected`·`Green`, 표준 오류는 0자였다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·Tenant·opaque `sub`·키 회전 검증은 외부 환경과 권한이 제공될 때까지 미완료로 유지한다. 토큰·비밀키·비밀번호는 저장소와 검증 기록에 기록하지 않는다.

## 2026-11-07 외부 조건 비차단 개발 연속성 정책

- 외부 IdP Metadata·JWKS·Issuer·Audience·`edan-nonprod` Tenant·테스트 사용자·opaque `sub`·키 회전 권한의 부재는 Development 작업을 중지시키는 차단 사유가 아니라 외부 경계 검증의 미완료 조건으로 분류한다.
- 위 정보가 없을 때는 Development `Authentication:Mode=Bypass`, 저장소에 비밀을 기록하지 않는 로컬 IdP/JWKS 테스트, Windows 자체 호스팅 CI를 계속 사용한다. Bypass는 외부 JWT 검증만 생략하고 Tenant 소속·조직 범위·관리자 권한 정책은 유지한다.
- GitHub main 보호 규칙 API의 HTTP 403은 비공개 저장소 요금제에 따른 형상 관리 조회 제한이다. 예산·저장소 공개 설정·보호 정책을 변경하거나 우회하지 않고, 로컬 개발·PR·자체 호스팅 CI를 계속 진행한다. 보호 규칙을 확인하지 못한 사실은 완료 증거로 표현하지 않는다.
- `External` 및 운영 환경에서는 이 정책을 적용하지 않는다. 실제 Metadata·Issuer·Audience·JWKS·HTTPS 검증을 유지하고, 외부 정보가 없으면 보호 API를 닫힌 상태로 유지한다. 외부 정보가 제공되면 동일한 인증 경계를 실제 비운영 환경에서 재검증한다.

## 2026-11-07 최신 main 전체 검증 재실행

- 현재 `main`과 `origin/main`은 `d1ae663547896191a4526dce4cf64f66a018fb32`로 동기화되어 있고 작업 디렉터리는 clean 상태다. GitHub 기본 브랜치도 같은 SHA를 가리킨다.
- Windows 로컬에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release --logger "console;verbosity=minimal"`을 순서대로 실행했다. Restore·Build·Test는 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 215개 통과·실패 0개·건너뜀 0개였다.
- Windows 서비스 `actions.runner.namanvarava-Edan.Tensor.EDAN-TENSOR-WIN`은 `Running`·`Automatic` 상태다. 최신 [Actions Run #721](https://github.com/namanvarava/Edan.Tensor/actions/runs/42364856137)은 같은 SHA에서 Job `128380659254`를 `EDAN-TENSOR-WIN` Runner 22(`self-hosted`, `Windows`, `X64`)로 실행했고 Restore·Build·Test를 모두 성공시켰다.
- GitHub 원격 조회에서 열린 Issue와 PR은 각각 0개다. `branches/main`은 `protected=false`를 반환했으며 보호 규칙 세부 API는 HTTP 403으로 조회할 수 없었다. 이는 비공개 저장소 요금제에 따른 환경 제약으로 기록하고 보호 정책을 변경하거나 우회하지 않았다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·`edan-nonprod` Tenant·테스트 사용자·안정적인 opaque `sub`·키 회전 권한은 아직 제공되지 않았다. Development Bypass와 로컬 IdP/JWKS 경계 검증은 계속 사용할 수 있지만, 실제 외부 IdP 경계 검증은 완료로 표시하지 않는다.

## 2026-11-07 PR #52 병합 후 최신 main 재검증

- PR #52의 문서 정정 변경을 `e94586fcbcae1ba1ebc4b0389385c5448de25906`으로 Squash Merge한 뒤 로컬 `main`과 `origin/main`, GitHub `main`이 같은 SHA를 가리키고 작업 디렉터리가 clean임을 확인했다.
- Windows 로컬에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실행했다. 세 명령은 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 215개 통과·실패 0개·건너뜀 0개였다.
- `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive --format json`도 종료 코드 0으로 완료되었고 취약성 항목을 반환하지 않았다.
- 최신 [Actions Run #723](https://github.com/namanvarava/Edan.Tensor/actions/runs/42368346011)은 병합 SHA에서 Job `128391099173`을 `EDAN-TENSOR-WIN` Runner 22(`self-hosted`, `Windows`, `X64`)로 실행하여 Restore·Build·Test·정리 단계를 모두 성공시켰다. Windows 서비스 `actions.runner.namanvarava-Edan.Tensor.EDAN-TENSOR-WIN`은 `RUNNING`·`AUTO_START`로 확인했다.
- 열린 Issue와 PR은 각각 0개다. `branches/main`은 `protected=false`이며 보호 규칙 세부 API는 HTTP 403으로 조회할 수 없으므로, 이 저장소 요금제 제약을 완료 증거로 오인하지 않는다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·`edan-nonprod` Tenant·테스트 사용자·안정적인 opaque `sub`·키 회전 권한은 여전히 제공되지 않았다. 따라서 실제 외부 IdP 경계 검증만 미완료이며, Development 작업은 Bypass와 로컬 경계 검증으로 계속할 수 있다.

## 2026-11-07 PR #53 병합 후 최종 main 검증

- PR #53을 `b950cf3657d5409df2ca8aa7a05cb348aabe098d`로 Squash Merge한 뒤 로컬 `main`, `origin/main`, GitHub `main`이 같은 SHA를 가리키고 작업 디렉터리가 clean임을 확인했다.
- Windows 로컬에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실행했다. 세 명령은 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 215개 통과·실패 0개·건너뜀 0개였다.
- `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive --format json`은 종료 코드 0으로 완료되었고 취약성 항목을 반환하지 않았다.
- 최신 [Actions Run #725](https://github.com/namanvarava/Edan.Tensor/actions/runs/42368919802)은 병합 SHA에서 Job `128392781568`을 `EDAN-TENSOR-WIN` Runner 22(`self-hosted`, `Windows`, `X64`)로 실행하여 Restore·Build·Test·정리 단계를 모두 성공시켰다. Windows 서비스는 `RUNNING`·`AUTO_START`였다.
- 열린 Issue와 PR은 각각 0개이며 예약 실행 트리거는 없다. `branches/main`은 `protected=false`이고 보호 규칙 세부 API는 HTTP 403이므로, 해당 정책 상세는 검증하지 못한 환경 제약으로 남긴다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·`edan-nonprod` Tenant·테스트 사용자·안정적인 opaque `sub`·키 회전 권한은 제공되지 않았다. 외부 경계 검증만 미완료이며 Development 작업은 중지하지 않는다.

## 2026-11-07 PR #54 병합 후 ApiHost Host Smoke 재검증

- PR #54 병합 직후 main 커밋 `0659d77b00b93c8c534a5c76591a6ecd6dae0645`의 Release 출력 디렉터리에서 실제 Windows ApiHost를 임시 포트와 임시 SQLite 네 개로 기동했다.
- 첫 번째 Smoke 하네스는 실행 중인 프로세스가 점유한 표준 오류 로그 파일을 종료 전에 읽으려 하여 파일 잠금 예외로 실패했다. 제품 프로세스의 로그나 API 오류가 아니라 검증 하네스의 순서 결함으로 분류했으며, `finally`에서 프로세스를 종료한 뒤 로그를 읽도록 최소 수정했다.
- 수정한 하네스로 `/health`, `/health/database`, `/`, `/openapi/v1.json`, `/api/platform/tenants/mine`을 호출했다. 모든 응답은 HTTP 200이었고 `/health/database`는 `Connected`·`Green`, Bypass 보호 API 응답은 빈 Tenant 배열, 표준 오류 길이는 0자였다.
- 검증 후 ApiHost 프로세스와 임시 SQLite·아카이브 디렉터리를 제거했으며 임시 디렉터리 삭제 여부도 확인했다. 이 검증은 외부 IdP 경계를 대신하지 않으며 Development Bypass 실행 경계만 확인한다.

## 2026-11-07 승인 범위 완료 감사 매트릭스

다음 표는 README의 승인 범위와 인수인계 PDF를 현재 main의 구현·테스트·실행 증거와 대조한 결과다. `완료`는 이번 릴리스 범위가 실제 증거로 확인되었다는 뜻이며, `환경 미검증`은 제품 코드의 개발 중단 사유가 아니라 외부 실행 조건이 없어 확인하지 못한 경계다.

| 영역 | 승인된 범위와 현재 상태 | 증거 | 판정 |
| --- | --- | --- | --- |
| Platform | Tenant·Organization·Project, Tenant 소속, 조직·프로젝트 권한, 감사·아카이브, 데이터베이스 무결성 및 격리를 구현했다. | Platform 테스트, API 통합 테스트, 실제 ApiHost Smoke | 완료 |
| DataSet | SQLite 메타데이터·콘텐츠 분리, UTF-8 CSV·JSON 객체 배열, 필수값·형식·범위·중복 규칙, 불변 profile revision, 결과 보존·명시적 삭제와 fencing을 구현했다. | DataSet Domain/Application/Infrastructure 테스트, Migration 및 Tenant 격리 검증 | 완료 |
| Worker | 전용 DbContext, 작업 카탈로그, Lease/Fencing, 체크포인트, 재시도·DLQ·재처리, Shutdown Handoff, DataSet Handler와 외부 전달 Idempotency를 구현했다. | Worker Domain/Infrastructure 테스트, 장애 복구·커서 LRU·외부 수신기 장애 테스트 | 완료 |
| Commercial / Licensing | 모듈 경계와 기반 프로젝트만 유지했으며 업무 흐름·결제·권리·만료 기능은 승인 범위에서 제외했다. | README 범위 표, 아키텍처 의존성 테스트 | 완료(제외 범위 준수) |
| Host / API | ApiHost·WorkerHost·HostServices 조합과 Health·OpenAPI·보호 API를 제공한다. | Release 빌드, 실제 Windows ApiHost Smoke, API 통합 테스트 | 완료 |
| 클라이언트 | WPF 화면과 업무 흐름은 이번 릴리스에서 제외했다. | README와 승인 범위 기록 | 완료(제외 범위 준수) |
| 보안 / 운영 | External 모드는 Issuer·Audience·Lifetime·서명·JWKS·HTTPS를 검증하고, Bypass는 Development에서만 외부 JWT 검증을 생략한다. | 로컬 JWKS 7개 경계 테스트, Bypass·HTTPS 회귀 테스트, 비밀 미저장 감사 | 완료 |
| CI / 문서 / 의존성 | Windows 자체 호스팅 Runner, Release 전체 테스트, 취약성 감사, 사용 가이드와 검증 기록을 유지한다. | Run #731, 215개 테스트, NuGet 취약성 감사 종료 코드 0, 현재 문서 | 완료 |
| 인수인계 PDF | PDF 기준 SHA `c187103cee76ad58e28633a259aeebc5e7dab75c` 이후의 변경을 현재 main과 대조했다. Worker Handoff·Tenant Task Cursor LRU·전용 DB·DataSet 저장소 분리·계층 규칙을 구현과 테스트에서 재확인했다. | PDF 검토 기록, ArchitectureDependencyTests, Worker 테스트, 현재 SHA `400b3beca3f887f4409af9cbfea137b724d48033` | 완료 |
| 실제 비운영 외부 IdP | Metadata/JWKS 주소, 기대 Issuer/Audience, `edan-nonprod` 사용자·opaque `sub`, 겹치는 키 회전 권한이 제공되지 않아 실제 외부 환경은 시험하지 못했다. | README 외부 조건, 로컬 JWKS 대체 검증, GitHub/로컬 상태 | 환경 미검증 |
| GitHub main 보호 규칙 | `branches/main`은 `protected=false`로 조회되지만 상세 보호 API는 비공개 저장소 요금제 제한으로 HTTP 403이다. 설정 변경이나 우회는 하지 않았다. | GitHub API 응답 기록 | 환경 미검증 |

이 감사 결과에 따라 외부 IdP와 보호 규칙 상세 조회만 최종 외부 조건으로 남기며, 해당 조건이 없다는 이유로 Development Bypass·로컬 테스트·자체 호스팅 CI를 중지하지 않는다. 실제 외부 IdP 정보가 제공되면 정상 토큰, 잘못된 Issuer/Audience, 만료, 서명 실패, 알 수 없는 `kid`, 기존·신규 키 겹침 회전을 같은 테스트 계약으로 재실행한다.

## 2026-11-07 최신 main 최종 재검증

- 현재 로컬 `main`, `origin/main`, GitHub `main`은 `400b3beca3f887f4409af9cbfea137b724d48033`으로 동기화되어 있고 작업 디렉터리는 clean 상태다. PR #56은 이 SHA로 Squash Merge되었으며 열린 Issue와 Pull Request는 각각 0개다.
- 실제 Windows 권한에서 다음 명령을 원문 그대로 순서대로 실행했다. `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`가 모두 종료 코드 0으로 완료되었다. Build는 경고 0개·오류 0개였고 전체 테스트는 215개 통과·실패 0개·건너뜀 0개였다.
- 제한된 실행 컨텍스트에서 같은 Test 명령을 실행했을 때 VSTest가 테스트 코드 실행 전에 `testhost` 연결 시간 초과로 종료되었다. 진단 로그에는 SDK 확장 로딩 중 `Microsoft.Bcl.AsyncInterfaces` 누락 경고와 로컬 Testhost 연결 실패가 기록되었다. 관리자 권한의 실제 Windows 실행에서 SharedKernel 11개와 전체 215개가 정상 통과했으므로 제품 코드·테스트 계약의 결함이 아니라 현재 실행 권한/SDK 테스트 호스트 환경 문제로 분류했다. 테스트 생략이나 실패 무시는 적용하지 않았다.
- Windows 서비스 `actions.runner.namanvarava-Edan.Tensor.EDAN-TENSOR-WIN`은 `RUNNING`이며 시작 유형은 `AUTO_START (DELAYED)`다. 최신 [Actions Run #731](https://github.com/namanvarava/Edan.Tensor/actions/runs/42370455298)은 Job `128397324749`를 `EDAN-TENSOR-WIN` Runner 22와 `self-hosted`, `Windows`, `X64` 레이블에서 실행했고 Restore·Build·Test·정리 단계가 모두 성공했다.
- `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive --format json`은 종료 코드 0으로 완료되었고 취약성 항목을 반환하지 않았다. Workflow에는 `schedule`, `cron`, `workflow_dispatch` 예약 트리거가 없다.
- `branches/main`은 `protected=false`로 조회되며 상세 보호 규칙 API는 비공개 저장소 요금제 제한으로 HTTP 403(`Upgrade to GitHub Pro or make this repository public`)을 반환한다. 설정 변경·과금 변경·보호 정책 우회는 하지 않았다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·`edan-nonprod` Tenant·테스트 사용자·안정적인 opaque `sub`·키 회전 권한은 여전히 제공되지 않았다. 이 조건은 Development 작업을 중지시키지 않으며 Bypass·로컬 IdP/JWKS·자체 호스팅 CI를 계속 사용한다. 다만 실제 외부 IdP 경계 검증만 환경 미검증으로 유지한다.


