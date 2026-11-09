# 2026-11-06 구현 및 CI 검증 현황

## 판정

CI 실패 복구와 Worker 종료·프로세스 장애 경계의 회귀 검증을 완료했다. 프로젝트 전체 완료는 미선언이다.
기능 문서의 후속 과제는 승인된 상세 완료 계약과 구분하며, 테스트 통과만으로 운영 적합성을 승인하지 않는다.

이 문서는 시간순 검증 기록이다. 2026-11-07 DataSet Validation v1 승인 이전 절의 “미승인” 또는 “미구현” 표현은 당시 상태를 보존한 역사 기록이며 현재 판정이 아니다. 현재 상태는 `2026-11-07 승인 범위 구현 및 로컬 경계 검증` 이후 절과 [DataSet 검증 v1 상세 계약](../architecture/dataset-validation-v1-proposal.md)을 기준으로 판단한다. 실제 비운영 외부 IdP 경계와 운영 수신 시스템 검증은 별도의 환경 조건이 제공되기 전까지 미검증으로 유지한다.

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

## 2026-11-07 PR #58 병합 후 최신 main 재검증

- PR #58은 `843a7ae643764708ac272aa7175da3f5b222686f`로 Squash Merge되었고 현재 로컬 `main`, `origin/main`, GitHub `main`이 같은 SHA를 가리킨다. 작업 디렉터리는 clean 상태이며 열린 Issue와 Pull Request는 각각 0개다.
- PR #58 변경은 `tests/README.md`의 오래된 PR #53 테스트 기준을 최신 main 기준으로 갱신하는 문서 변경이다. 병합 후 Windows 로컬에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. Restore·Build·Test는 종료 코드 0이며 Build 경고 0개·오류 0개, 테스트 구성은 215개 통과·실패 0개·건너뜀 0개로 유지된다.
- 병합 후 [Actions Run #734](https://github.com/namanvarava/Edan.Tensor/actions/runs/42372856339)은 head SHA `843a7ae643764708ac272aa7175da3f5b222686f`에서 Job `128404366909`를 `EDAN-TENSOR-WIN` Runner 22(`self-hosted`, `Windows`, `X64`)로 실행했다. Restore·Build·Test·정리 단계가 모두 성공했다.
- 외부 IdP Metadata/JWKS·Issuer·Audience·`edan-nonprod` Tenant·테스트 사용자·안정적인 opaque `sub`·키 회전 권한은 제공되지 않았다. 이 조건은 Development 작업을 중지시키지 않으며, 실제 외부 IdP 경계 검증만 환경 미검증으로 유지한다. GitHub main 보호 규칙 상세 API의 HTTP 403도 변경하거나 우회하지 않는다.

## 2026-11-08 Development Bypass 인증 정책 재검증

- Development `Authentication:Mode=Bypass`의 계약을 명확히 했다. 외부 IdP 설정과 Bearer 토큰이 없어도 모든 요청을 `local-development-subject`와 `platform_admin` Claim을 가진 인증 주체로 처리하며, Tenant 소속·조직 범위·관리자 권한 검사는 계속 수행한다.
- `DevelopmentBypassAuthenticatesWithoutExternalIdentityProvider` 회귀 테스트를 추가하여 외부 IdP 설정이 전혀 없는 토큰 없는 요청의 `Succeeded=true`와 `IsAuthenticated=true`를 직접 확인했다. 구성된 주체·역할 Claim과 운영 환경 Bypass 차단도 함께 확인했다.
- 실제 Windows Release ApiHost를 임시 SQLite 네 개와 Development 환경으로 기동했다. `/health`와 `/health/database`는 HTTP 200, 데이터베이스 상태는 `Connected`·`Green`, 토큰 없는 `/api/platform/tenants/mine`은 HTTP 200과 빈 Tenant 배열을 반환했으며 표준 오류는 없었다. 검증 후 프로세스와 임시 파일을 제거했다.
- `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실제 Windows 권한으로 실행했다. Restore·Build·Test는 종료 코드 0, Build 경고 0개·오류 0개, 전체 테스트는 216개 통과·실패 0개·건너뜀 0개였다.
- README, Platform API 보안 설계, API 사용 가이드와 Handler의 한국어 설명을 같은 계약으로 갱신했다. 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·Tenant·opaque `sub`·키 회전 검증은 여전히 환경 미검증이며 Development Bypass를 중지시키지 않는다.

## 2026-11-08 PR #60 병합 후 최신 main 재검증

- PR #60은 `dcc01f10db4390a3a25652abb2f2fa869573d803`으로 Squash Merge되었고 로컬 `main`, `origin/main`, GitHub `main`이 이 SHA로 동기화되었다. 작업 디렉터리는 clean 상태이며 이번 변경의 열린 PR은 없다.
- 병합 후 실제 Windows에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. 세 명령은 모두 종료 코드 0이며 Build 경고 0개·오류 0개, 전체 테스트 216개 통과·실패 0개·건너뜀 0개를 확인했다.
- [Actions Run #739](https://github.com/namanvarava/Edan.Tensor/actions/runs/42392176169), Job `128461854947`는 최신 main SHA에서 자체 호스팅 Windows Runner의 Restore·Build·Test·정리 단계를 모두 성공시켰다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·`edan-nonprod` Tenant·테스트 사용자·안정적인 opaque `sub`·키 회전 권한은 제공되지 않았다. 이 조건은 Development Bypass와 로컬 검증을 중지시키지 않으며, 실제 외부 IdP 경계 검증만 환경 미검증으로 유지한다. GitHub main 보호 규칙 상세 API의 HTTP 403도 변경하거나 우회하지 않는다.

## 2026-11-08 Bypass 외부 설정 무시 회귀 보강

- 외부 IdP의 Metadata/JWKS 주소, Issuer, Audience와 HTTPS 설정이 없거나 잘못되어도 Development `Authentication:Mode=Bypass`가 외부 인증 구성을 읽거나 검증하지 않는다는 계약을 명시했다. Bypass는 `local-development-subject`와 `platform_admin` Claim으로 요청을 인증하며 Tenant 소속·조직 범위·관리자 권한 검사는 유지한다.
- `DevelopmentBypassIgnoresExternalIdentityProviderSettings` 회귀 테스트를 추가하여 잘못된 Metadata 주소와 외부 Claim 설정이 있어도 토큰 없는 요청이 `Succeeded=true`, `IsAuthenticated=true`로 처리되는 것을 확인했다. 외부 설정이 전혀 없는 Bypass 테스트와 운영 환경 Bypass 차단 테스트도 유지된다.
- 대상 테스트 8개와 Release 전체 테스트 217개가 실패 0개·건너뜀 0개로 통과했으며, Build는 경고 0개·오류 0개였다. 실제 비운영 외부 IdP 경계는 여전히 외부 환경과 권한 제공 후 검증하며, 이 회귀는 외부 검증 완료를 의미하지 않는다.

## 2026-11-08 Development Bypass 로컬 Tenant 초기화 보강

- `DevelopmentData:SeedBypassTenant=true`와 `Authentication:Mode=Bypass`가 함께 설정된 Development 환경에서만 `EDAN-LOCAL` Tenant와 Bypass `sub`의 Tenant 소속을 시작 시 멱등적으로 준비하도록 구현했다. 기본 주체 `local-development-subject`와 `platform_admin`은 Development 구성에만 있으며 외부 Issuer·Audience·JWKS·토큰·개인키를 대체하지 않는다.
- 같은 Tenant 코드나 같은 Tenant·주체 소속 레코드가 이미 있으면 기존 역할과 상태를 덮어쓰지 않는다. 기존 Tenant가 비활성 상태이면 경고만 기록하고 데이터베이스를 변경하지 않는다. External 모드와 Development가 아닌 환경에서는 Seeder가 실행되지 않는다.
- `DevelopmentBypassDataSeederTests`는 실제 Bypass Handler와 임시 SQLite를 사용하여 `/api/platform/tenants/mine`의 Administrator 소속, Tenant 수 1개, 동일 주체의 관리자 소속 1개를 확인한다. 테스트 실행 전 Release 대상 프로젝트 빌드는 `--maxcpucount:1`에서 경고 0개·오류 0개로 통과했다.
- 이 세션의 로컬 VSTest는 기존 `PlatformAuthenticationConfigurationTests` 필터에서도 90초 동안 testhost 연결을 수립하지 못해 결과를 반환하지 않았다. 따라서 로컬 테스트 통과로 기록하지 않으며, 자체 호스팅 CI에서 대상 테스트와 전체 Release 테스트를 재검증해야 한다. 동일 환경의 직접 Host 기동 로그에서는 Seeder의 Tenant·소속 INSERT까지 확인했지만, 60초 준비 대기 안에 HTTP Health 응답을 확보하지 못해 Host Smoke 성공으로 기록하지 않는다.

## 2026-11-08 PR #63 자체 호스팅 CI 검증

- PR #63의 head `23c42166889412e034fcf8dd7dda7a8b1efdb7a8`에서 [Actions Run #744](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399072656)의 Job `128482729704`가 `EDAN-TENSOR-WIN` 자체 호스팅 Windows Runner에서 실행되었다. Restore, Release Build, Test와 정리 단계가 모두 성공했다.
- CI 로그는 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실행했고, Build 경고 0개·오류 0개와 전체 218개 통과·실패 0개·건너뜀 0개를 기록했다. API 통합 테스트는 21개이며 `DevelopmentBypassDataSeederTests`도 통과했다.
- 로컬 직접 Host 기동에서는 EF Migration과 Seeder의 `EDAN-LOCAL`·`local-development-subject` INSERT 및 `Now listening` 로그까지 확인했다. 그러나 현재 실행 컨텍스트에서는 별도 프로세스의 `127.0.0.1` 포트가 다른 명령 세션에서 연결되지 않아 `/health`·`/health/database`·`/api/platform/tenants/mine` HTTP Smoke를 성공으로 기록하지 않았다. 프로세스는 중단하고 임시 SQLite·로그 디렉터리를 삭제했다.
- PR #63은 CI 성공 후 검토·병합 전 상태이며, 실제 비운영 외부 IdP 경계 검증은 외부 Metadata/JWKS·Issuer/Audience·Tenant·사용자·키 회전 권한이 제공될 때 별도로 수행한다.

## 2026-11-08 PR #63 병합 후 최신 main 재검증

- PR #63은 `5ede65b86ecbb393131caf6d531d095cbbbae7cd`로 Squash Merge되었고, 로컬 `main`, `origin/main`, GitHub `main`이 같은 SHA를 가리키며 작업 디렉터리는 clean 상태다. 열린 Issue와 PR은 각각 0개다.
- 현재 Windows 로컬 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 원문 그대로 실행했다. Restore·Build·Test는 모두 종료 코드 0이며 Build는 경고 0개·오류 0개, 전체 테스트는 218개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #746](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399622276)의 Job `128484365527`은 `EDAN-TENSOR-WIN` 자체 호스팅 Windows Runner에서 실행되었다. 로그에는 동일한 Restore·Release Build·Release Test 명령과 Build 경고 0개·오류 0개, 전체 테스트 218개 통과가 기록되어 있다.
- 현재 실행 컨텍스트에서 Windows 서비스 관리자는 권한 부족으로 조회할 수 있었으므로 서비스의 현재 Running·자동 시작 상태는 이 세션에서 새로 확인하지 않았다. 다만 Run #746의 Runner 이름·레이블과 실제 Job 성공은 GitHub 실행 기록으로 확인했다. 서비스 상태를 독립적으로 확인하려면 로컬 관리자 권한 PowerShell에서 `Get-Service -Name 'actions.runner*'`를 실행해야 한다.
- 당시 확인 범위에서는 인수인계 PDF 원본을 찾지 못했다는 기록을 남겼다. 이후 2026-11-08 Windows 로컬 감사에서 동일 파일을 `C:\Users\naman\Downloads\Edan.Tensor_Continuation_Handoff_2026-11-06.pdf`에서 발견하여 아래 절에서 원본을 다시 렌더링하고 검토했다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·`edan-nonprod` Tenant·테스트 사용자·안정적인 opaque `sub`·키 회전 권한은 여전히 제공되지 않았다. 이 조건은 Development Bypass와 로컬 검증을 중지시키지 않으며, 실제 외부 IdP 경계 검증만 환경 미검증으로 유지한다. GitHub main 보호 규칙 상세 API의 HTTP 403도 변경하거나 우회하지 않는다.

## 2026-11-08 최신 main Release Host Smoke 재검증

- 병합된 최신 main의 Release ApiHost를 임시 SQLite 네 개와 Development Bypass 설정으로 하나의 Windows 검증 프로세스에서 기동했다. 동일 프로세스에서 `/health`는 HTTP 200, `/health/database`는 HTTP 200과 `Connected`·`Green`, `/api/platform/tenants/mine`은 HTTP 200과 Administrator Tenant 1개 및 `tenantId`를 반환했다.
- 첫 번째 Smoke 하네스는 Tenant API 응답에 존재하지 않는 `code` 필드를 기대해 실패했다. 로그에서 모든 HTTP 상태와 Administrator 응답은 정상임을 확인했고, 제품 오류가 아니라 검증 하네스의 응답 계약 불일치로 분류했다. 하네스를 실제 API 계약인 `tenantId`·`role` 검증으로 최소 수정한 뒤 동일 조건에서 재실행하여 성공했다.
- 검증 후 ApiHost 프로세스와 임시 SQLite·아카이브 디렉터리를 종료·삭제했다. 이 검증은 외부 IdP를 대체하지 않으며 Development Bypass와 로컬 Tenant 초기화 경계만 확인한다.

## 2026-11-08 인수인계 PDF 원본 재검토 및 현재 상태 대조

- 인수인계 PDF 원본 `C:\Users\naman\Downloads\Edan.Tensor_Continuation_Handoff_2026-11-06.pdf`를 확인했다. 문서는 2쪽 A4, 비암호화 PDF이며 `pdfinfo`에서 JavaScript와 폼이 없음을 확인했다.
- 두 페이지를 PNG로 렌더링하여 제목, 기준 SHA, Actions Run #605 예산 차단 기록, 저장소 운영 규칙, Worker 종료 인계·Tenant Task Cursor LRU·전용 DbContext·DataSet 저장소 분리·Platform 계층 규칙 및 재개 조건의 잘림·겹침·검은 사각형을 시각 확인했다. Poppler의 `Symbol`·`ArialUnicode` 글꼴 경고는 렌더링 경고였으며 페이지 내용의 누락으로 이어지지 않았다.
- PDF가 기록한 기준 main SHA `c187103cee76ad58e28633a259aeebc5e7dab75c`는 현재 main SHA `ebfc71c50105196036176b51b6abf89d74f4da21`와 다르다. 현재 main의 후속 PR과 CI 결과를 기준으로 재평가했으며 PDF 기준 SHA를 최신 상태로 간주하지 않았다.
- PDF의 Actions Run #605(`42124026661`)는 `runner_id=0`, Runner·Job Step·로그가 없는 Actions 예산 사전 차단이었다. 현재 Run #750(`42401219135`)은 실제 `EDAN-TENSOR-WIN` Windows X64 self-hosted Runner의 Job `128489158344`에서 Restore·Build·Test를 모두 성공시켰다. 과거 Run #605는 재실행하지 않았고 Billing·예산 설정도 변경하지 않았다.
- PDF의 Worker 설계는 현재 구현과 테스트에서 확인했다. `WorkerExecutionRegistry`와 `WorkerTaskDispatcher`는 단일 handoff 예산을 공유하고, Tenant Task Cursor는 Dispatcher 단일 루프의 LRU로 관리하며 기본 10,000·허용 범위 500–100,000을 사용한다. Worker는 전용 `WorkerDbContext`를 사용하고 DataSet 메타데이터·콘텐츠는 별도 저장 경계를 사용한다. 모듈 의존성 테스트도 현재 Release 전체 테스트에 포함되어 있다.
- PDF의 “예산 해제 전 코드 변경 중단” 지시는 당시 인수인계 절차였고, 현재 전체 개발 목표의 “CI 장애와 무관하게 가능한 개발을 계속한다”는 정책으로 대체되었다. 실제 CI가 정상화된 현재는 로컬 Release 검증과 self-hosted CI 증거를 함께 유지한다.
- PDF가 요구한 Billing 화면 확인은 읽기 전용으로만 수행한다. 예산 증액, 사용 중지 옵션 변경, GitHub-hosted Runner 전환은 수행하지 않는다. 실제 비운영 외부 IdP 경계는 Metadata/JWKS·Issuer·Audience·테스트 Tenant·opaque `sub`·키 회전 권한이 제공되기 전까지 환경 미검증으로 유지한다.

## 2026-11-08 PR #66 병합 후 최신 main 재검증

- PR #66은 `edb36d15efbfd8ebb20f550e0fa6199e0e1dbc10`으로 Squash Merge되었고, 로컬 `main`과 `origin/main`은 해당 SHA로 동기화되었다. 병합 후 열린 PR과 Issue는 각각 0개이며 작업 디렉터리는 clean 상태다.
- 병합 후 Windows 로컬에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 원문 그대로 다시 실행했다. Restore는 성공했고, Build는 경고 0개·오류 0개, Test는 218개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #752](https://github.com/namanvarava/Edan.Tensor/actions/runs/42402487854)의 Job `128493082479`는 `EDAN-TENSOR-WIN` self-hosted Windows X64 Runner에서 실행되었다. Restore·Release Build·Release Test·정리 단계가 모두 성공했고, CI 로그의 테스트 합계도 218개 통과·실패 0개·건너뜀 0개로 확인했다.
- PR #66의 필수 `build-and-test` Check도 동일 Runner에서 성공한 뒤 병합했다. 문서 변경은 현재 main에 반영되었으며 코드·설정 동작은 변경하지 않았다.

## 2026-11-08 최신 main 최종 로컬·CI·정리 재검증

- 현재 로컬 `main`, `origin/main`, GitHub `main`은 `bbb7ff986df892b9d4b77987502d7bdb192d74df`를 가리키며 작업 디렉터리는 clean 상태다. 이번 검증은 코드 동작을 변경하지 않고 현재 상태의 증거만 갱신했다.
- 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 원문 그대로 실행했다. Restore와 Build는 종료 코드 0으로 완료했고 Build는 경고 0개·오류 0개였다. 전체 Test는 218개 통과·실패 0개·건너뜀 0개였다.
- 제한된 실행 컨텍스트에서 최초 Restore가 Windows Service Control Manager 접근 거부로 출력 없이 실패했지만, 진단에서 SDK 서비스 조회 권한 부족을 확인했다. 실제 Windows 권한으로 같은 명령을 재실행한 결과 Restore가 성공했으므로 제품 코드나 패키지 복원 결함으로 분류하지 않았다.
- 로컬 JWKS 테스트 서버를 사용하는 `PlatformJwksAuthenticationTests`를 다시 실행하여 1개 테스트가 실패 0개·건너뜀 0개로 통과했다. 이 검증은 정상 토큰, 잘못된 Issuer·Audience, 만료, 서명 실패, 알 수 없는 `kid`, 키 회전 경계를 포함한 로컬 인증 경계이며 실제 비운영 외부 IdP 검증을 대체하지 않는다.
- 최신 Release ApiHost를 임시 포트와 SQLite 네 개로 실제 Windows 프로세스에서 기동했다. Development Bypass 조건에서 `/health`, `/health/database`, `/api/platform/tenants/mine`은 모두 HTTP 200을 반환했고, 데이터베이스는 `Connected`·`Green`, Tenant 응답은 `tenantId`와 `Administrator`를 포함했다. 검증 후 프로세스·임시 SQLite·로그를 삭제했다.
- `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`는 종료 코드 0으로 완료되었고 모든 프로젝트에서 취약한 패키지가 없다고 보고했다.
- 최신 [Actions Run #756](https://github.com/namanvarava/Edan.Tensor/actions/runs/42403586908)의 Job `128496404292`는 head SHA `bbb7ff986df892b9d4b77987502d7bdb192d74df`에서 `EDAN-TENSOR-WIN` Runner와 `self-hosted`, `Windows`, `X64` 레이블로 실행되었다. Restore·Build·Test·정리 단계가 모두 성공했다. Windows 서비스 `actions.runner.namanvarava-Edan.Tensor.EDAN-TENSOR-WIN`도 `Running`·`Automatic`으로 확인했다.
- 전체 Pull Request는 병합 67개·종료 1개·열림 0개이며 열린 Issue는 0개다. 원격 브랜치는 `main`만 남았고 로컬 브랜치도 `main`과 `origin/main`만 남아 병합된 head 브랜치 정리를 완료했다.
- `gh api repos/namanvarava/Edan.Tensor/branches/main/protection`은 비공개 저장소 요금제 제한으로 HTTP 403과 `Upgrade to GitHub Pro or make this repository public`을 반환했다. 저장소 공개, Pro 업그레이드, 보호 규칙 변경은 수행하지 않았다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·테스트 사용자·안정적인 opaque `sub`·키 회전 권한은 제공되지 않았다. Development Bypass·로컬 JWKS·자체 호스팅 CI는 중지하지 않으며, 실제 외부 IdP 경계만 환경 미검증으로 유지한다.

## 2026-11-08 PR #69 병합 후 최신 main 재검증

- PR #69는 `2fdc01593b3bce832031a530c803e8f23a9163cf`로 Squash Merge되었고, 병합된 원격 head 브랜치는 삭제했다. 현재 로컬 `main`과 `origin/main`은 같은 SHA를 가리키며 작업 디렉터리는 clean 상태다.
- 병합 후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. 세 명령은 모두 종료 코드 0이며 Build는 경고 0개·오류 0개, 전체 Test는 218개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #758](https://github.com/namanvarava/Edan.Tensor/actions/runs/42408588379)의 Job `128511577071`은 head SHA `2fdc01593b3bce832031a530c803e8f23a9163cf`에서 `EDAN-TENSOR-WIN` 자체 호스팅 Windows Runner와 `self-hosted`, `Windows`, `X64` 레이블로 실행되었다. Restore·Build·Test·정리 단계가 모두 성공했다.
- PR #69의 검증 기록은 코드 동작을 변경하지 않았으며, 문서 변경 후 최신 main의 Release 검증과 self-hosted CI 성공을 다시 확인했다.

## 2026-11-08 PR #70 병합 후 최종 main 재검증

- PR #70은 `6e15595a255a130df66252faf354c7fca3507ef7`로 Squash Merge되었고 병합된 원격 head 브랜치는 삭제했다. 현재 로컬 `main`과 `origin/main`은 같은 SHA를 가리키며 작업 디렉터리는 clean 상태다.
- 병합 후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. 세 명령은 모두 종료 코드 0이며 Build는 경고 0개·오류 0개, 전체 Test는 218개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #760](https://github.com/namanvarava/Edan.Tensor/actions/runs/42409045585)의 Job `128512963914`는 head SHA `6e15595a255a130df66252faf354c7fca3507ef7`에서 `EDAN-TENSOR-WIN` 자체 호스팅 Windows Runner와 `self-hosted`, `Windows`, `X64` 레이블로 실행되었다. Restore·Build·Test·정리 단계가 모두 성공했다.
- 최종 확인에서 원격 브랜치는 `main`만 남았고 열린 Pull Request와 Issue는 각각 0개였다. `main` 보호 규칙 상세 API의 HTTP 403과 실제 비운영 외부 IdP 미검증 상태는 변경하지 않고 환경 조건으로 기록했다.

## 2026-11-08 현재 main Release 재검증 및 테스트 기준 정정

- 검증 시작 시 로컬 `main`, `origin/main`, GitHub `main`은 `5518c68da0a4fbf14e8f8cce480e75c847b24b30`을 가리켰고 작업 디렉터리는 clean 상태였다. `tests/README.md`가 이전 커밋 `38539a85d32732cfc4111261576fc36fa1a2d052`를 현재 기준으로 기록하고 있어 최신 main SHA와 [Actions Run #764](https://github.com/namanvarava/Edan.Tensor/actions/runs/42412691901)로 정정했다.
- 제한된 실행 컨텍스트에서 최초 `dotnet restore EDAN.Tensor.sln`은 출력 없이 종료 코드 1로 실패했다. 같은 컨텍스트의 `dotnet --info`에서 .NET SDK가 Windows Service Control Manager를 열지 못하고 액세스 거부를 반환한 로그를 확인했으며, 제품 코드·패키지 복원 오류로 추측하지 않았다. 실제 Windows 권한에서 같은 Restore를 다시 실행하여 복원할 모든 프로젝트가 최신 상태임을 확인했다.
- 실제 Windows 권한에서 다음 명령을 순서대로 실행했다.

  ```powershell
  dotnet restore EDAN.Tensor.sln
  dotnet build EDAN.Tensor.sln --no-restore --configuration Release
  dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
  ```

  Restore는 종료 코드 0으로 완료했고, Build는 경고 0개·오류 0개, Test는 218개 통과·실패 0개·건너뜀 0개였다. 인증·개발 Bypass·로컬 Tenant Seeder 경계를 별도 필터로 실행한 10개 테스트도 실패 0개·건너뜀 0개로 통과했다.
- `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`는 종료 코드 0으로 완료했고 모든 프로젝트에서 취약한 패키지가 없었다. `git diff --check`도 통과했다.
- 최신 [Actions Run #764](https://github.com/namanvarava/Edan.Tensor/actions/runs/42412691901)의 Job `128524035532`는 `EDAN-TENSOR-WIN` 자체 호스팅 Runner에서 실행되었다. Restore·Build·Test·정리 단계가 모두 성공했고, head SHA는 `5518c68da0a4fbf14e8f8cce480e75c847b24b30`이다. 워크플로에는 `schedule`·`cron`·`workflow_dispatch`가 없고, `pull_request_target`도 사용하지 않는다.
- 이 변경은 검증 문서와 테스트 안내의 기준만 정정하며 제품 소스·공개 API·데이터 스키마·인증 우회 동작을 변경하지 않는다. Development `Authentication:Mode=Bypass`는 로컬·격리된 Development 환경과 명시적인 설정에서만 사용하고, 비개발 환경에서는 차단하는 기존 계약을 유지한다.
- 실제 비운영 외부 IdP의 Metadata/JWKS·Issuer·Audience·테스트 Tenant·테스트 사용자·안정적인 opaque `sub`·키 회전 권한은 여전히 제공되지 않았다. 따라서 로컬 JWKS와 Development Bypass는 검증했지만 실제 외부 IdP 경계는 환경 미검증으로 남긴다. 재개 조건은 해당 비운영 IdP와 테스트 권한을 제공받아 External 모드의 실제 Tenant·키 회전 경계를 실행하는 것이다.

## 2026-11-08 PR #73 병합 후 CI 실패 분석과 Worker 종료 테스트 복구

- PR #73은 `b7cd3b11cbc82b223c0214b5d38b5916d4971e05`로 Squash Merge되었다. 병합 직후 [Actions Run #766](https://github.com/namanvarava/Edan.Tensor/actions/runs/42414340542)의 Job `128529061348`은 `EDAN-TENSOR-WIN`에서 Checkout·Restore·Build까지 성공했지만 Test 단계에서 실패했다.
- 실패는 `WorkerTaskDispatcherShutdownTests.DispatcherDoesNotSpendHandoffBudgetAgainAfterRegistryTimesOut` 한 건이며, `WorkerTaskDispatcherShutdownTests.cs` 42번째 줄의 `WaitAsync(TimeSpan.FromSeconds(8))`에서 `System.TimeoutException`이 발생했다. Worker Infrastructure는 65개 통과·1개 실패였고, 다른 테스트 프로젝트는 모두 통과했다. 로그에는 Runner 연결·Restore·Build 오류가 없고 고아 프로세스 정리만 수행되었다.
- 코드와 로그를 대조한 결과 제품 종료 예산 계산을 변경해야 하는 증거는 없었다. 이 테스트는 유예 1초와 인계 4초를 사용하여 정상 경로 약 5초와 잘못된 이중 인계 대기 약 9초를 8초 상한으로 구분한다. 자체 호스팅 Runner의 병렬 스케줄링 지연이 정상 경로 상한에 가까워지면서 시간 기반 경계가 오판할 수 있는 결함으로 분류했다. 근거 없이 Run #766을 재실행하지 않았고, 상한만 늘려 이중 대기 회귀를 놓치는 대안도 적용하지 않았다.
- 최소 안전 수정으로 제품 코드는 그대로 두고 해당 테스트의 인계 예산을 4초에서 8초로, 판정 상한을 8초에서 15초로 조정했다. 정상 경로는 약 9초로 완료되며, Dispatcher가 인계 예산을 다시 소비하는 잘못된 경로는 약 17초가 필요하므로 회귀 판정 간격을 유지한다. 한국어 주석은 실제 시간 계약과 Runner 지연 허용 조건을 설명하도록 갱신했다.
- 수정 후 대상 테스트 3회가 각각 통과했고, Worker Infrastructure 전체 66개 및 솔루션 전체 Release Test 218개가 실패 0개·건너뜀 0개로 통과했다. Release Build는 경고 0개·오류 0개였고 Restore도 성공했다. 수정 PR의 자체 호스팅 Check와 병합 후 main push Check가 성공하는지 추가 확인한다.

## 2026-11-08 PR #74 병합 후 최신 제품 코드 재검증

- PR #74는 `9c3f2f4d6e9df1f247792af9c3332b78080948cf`로 Squash Merge되었다. 병합 후 로컬 `main`, `origin/main`, GitHub `main`은 이 SHA를 가리켰고 작업 디렉터리는 clean 상태였다.
- 병합 후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, Test는 218개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #768](https://github.com/namanvarava/Edan.Tensor/actions/runs/42415314493)의 Job `128532003342`는 `EDAN-TENSOR-WIN` 자체 호스팅 Windows Runner에서 실행되었다. Restore·Build·Test·정리 단계가 모두 성공했고 head SHA는 `9c3f2f4d6e9df1f247792af9c3332b78080948cf`이다.
- Run #766의 실패 원인이었던 Worker 종료 테스트는 수정 후 PR Check Run #767과 병합 후 Run #768에서 모두 통과했다. 수정은 테스트 시간 경계와 한국어 주석만 변경했으며 제품 종료 예산·공개 API·데이터·인증 계약은 변경하지 않았다.
- 현재 열린 Issue와 Pull Request는 0건이며, 제품 코드 기준 외부 IdP 미검증 조건과 GitHub main 보호 규칙 상세 API의 HTTP 403은 환경 차단 사유로 계속 기록한다. 문서 전용 후속 커밋의 SHA가 제품 코드 검증 SHA와 달라질 수 있으므로 두 기준을 보고에서 구분한다.

## 2026-11-08 승인된 Development Bypass 고정 프로필 적용

- 사용자의 명시적 승인에 따라 Development `Authentication:Mode=Bypass`가 고정된 비운영 IdP 시뮬레이션 프로필을 사용하도록 적용했다. 프로필은 Metadata `https://auth-test.example.com/.well-known/openid-configuration`, JWKS `https://auth-test.example.com/jwks`, Issuer `https://auth-test.example.com`, Audience `edan-tensor-api`, 테스트 Tenant `edan-nonprod`, 테스트 사용자 `edan-nonprod-test-user`, 안정적인 `sub` `edan-nonprod-test-subject`, 키 회전 시나리오 식별자 `nonprod-idp-test-key-rotation`을 사용한다.
- `example.com`은 RFC 예약 도메인이므로 프로필은 실제 외부 IdP에 연결하거나 Metadata·JWKS·서명을 검증하지 않는다. Bypass 첫 인증 요청과 Tenant Seeder는 고정 프로필 사용 사실을 경고 로그로 기록하며, Tenant 소속·조직 범위·관리자 권한 검사는 유지한다. External 모드와 비개발 환경의 HTTPS·Issuer·Audience·JWKS 검증 및 Bypass 차단은 변경하지 않았다.
- 기본 Development 설정의 Tenant와 안정적인 `sub`를 `EDAN-NONPROD`·`edan-nonprod-test-subject`로 맞췄고, 고정 프로필의 모든 필드를 직접 검증하는 회귀 테스트를 추가했다. 제한된 실행 컨텍스트에서는 Windows Service Control Manager 권한 부족으로 출력 없이 빌드가 실패했지만, 실제 Windows 권한에서 원인을 재현한 뒤 참조 오류 1건을 수정하고 대상 빌드 경고 0개·오류 0개, 인증·Seeder 대상 테스트 10개 통과를 확인했다.
- 변경 후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, 전체 Test는 219개 통과·실패 0개·건너뜀 0개였다. `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`도 취약 패키지 없음으로 종료했다.
- 이 변경은 실제 비운영 외부 IdP의 Metadata/JWKS·Issuer·Audience·Tenant·사용자·키 회전 권한 검증을 완료한 것으로 표시하지 않는다. 실제 외부 환경이 제공되면 External 모드에서 실제 토큰·키 회전 경계를 별도로 재실행한다.

## 2026-11-08 PR #76 병합 후 main 재검증

- PR #76은 `fca8edb96f3fc12388a168347985d28ec2b62102`로 Squash Merge되었고, 병합 후 로컬 `main`, `origin/main`, GitHub `main`이 같은 SHA를 가리켰다.
- 병합 후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, 전체 Test는 219개 통과·실패 0개·건너뜀 0개였다.
- 병합 후 [Actions Run #772](https://github.com/namanvarava/Edan.Tensor/actions/runs/42420682116)은 head SHA `fca8edb96f3fc12388a168347985d28ec2b62102`의 `EDAN-TENSOR-WIN` 자체 호스팅 Runner에서 Restore·Release Build·Release Test·정리 단계를 모두 성공시켰다.
- 최종 상태에서 열린 Issue와 Pull Request는 0건이고 원격 브랜치는 `main`만 남았으며, 병합된 작업 브랜치 `codex/development-bypass-fixed-profile-20261108`의 로컬·원격 삭제를 확인했다. 실제 외부 IdP 경계와 GitHub main 보호 규칙 HTTP 403 조건은 기존과 같이 환경 미검증으로 유지한다.




## 2026-11-08 PR #78 병합 후 최신 main 문서·Release·CI 재검증

- PR #78의 head `e88d925e89055e4fc1ced15cc2bbf2fcf72b6e1a`는 문서의 Development Bypass Tenant·고정 `sub` 예시를 현재 구현인 `EDAN-NONPROD`·`edan-nonprod-test-subject`와 일치시킨 뒤 `2e9ff6d7a61faf924b267119c87a84370aeeb48f`로 Squash Merge되었다. 병합 후 로컬 `main`과 `origin/main`은 같은 SHA를 가리키고 작업 디렉터리는 clean 상태다.
- 병합 후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 원문 그대로 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, 전체 Test는 219개 통과·실패 0개·건너뜀 0개였다. 이전 문서 값 `EDAN-LOCAL`·`local-development-subject` 검색과 `git diff --check`도 통과했다.
- PR #78 자체 호스팅 검증 [Actions Run #775](https://github.com/namanvarava/Edan.Tensor/actions/runs/42444220095)는 성공했다. 병합 후 최신 main push [Actions Run #776](https://github.com/namanvarava/Edan.Tensor/actions/runs/42444334669)도 head SHA `2e9ff6d7a61faf924b267119c87a84370aeeb48f`에서 `success`로 완료되었고 `build-and-test` 작업이 성공했다.
- 열린 Issue와 Pull Request는 각각 0건이며, 원격 브랜치 검색에서 병합된 `codex/align-bypass-docs-20261108`가 사라졌고 로컬 브랜치에는 `main`만 남았다. 현재 저장소 visibility는 여전히 private이고, `main` 보호 규칙 API HTTP 403과 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·테스트 사용자·안정적인 `sub`·키 회전 권한은 환경 미검증 상태다. 공개 전환이나 GitHub 요금제 변경은 수행하지 않았다.


## 2026-11-08 PR #79 병합 후 최종 main Release·CI·정리 재검증

- PR #79의 head `961ece39eefe59b9ac67fb8a8e15b931613ac100`는 검증 문서에 PR #78 병합과 최신 Release·CI 근거를 추가한 뒤 `0296e595758bb93ba2985a041736a572ebbf1494`로 Squash Merge되었다. 병합 후 로컬 `main`과 `origin/main`은 같은 SHA를 가리키고 작업 디렉터리는 clean 상태다.
- 최종 병합 커밋에서 실제 Windows 권한으로 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, 전체 Test는 219개 통과·실패 0개·건너뜀 0개였다. `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`도 모든 프로젝트에 취약 패키지가 없다고 보고했다.
- 병합 후 최신 main push [Actions Run #779](https://github.com/namanvarava/Edan.Tensor/actions/runs/42444750012)은 head SHA `0296e595758bb93ba2985a041736a572ebbf1494`에서 `success`로 완료되었고 `build-and-test` 작업이 성공했다. PR #79 검증 [Actions Run #777](https://github.com/namanvarava/Edan.Tensor/actions/runs/42444648454)도 성공했다.
- 열린 Issue와 Pull Request는 각각 0건이며, 원격 브랜치 검색 결과 `main`만 남았다. 로컬 브랜치도 `main`만 남았고, 병합된 `codex/record-current-validation-20261108` 원격 브랜치 삭제를 확인했다. 저장소는 여전히 private이고 `main` 보호 규칙 HTTP 403과 실제 비운영 외부 IdP 경계는 환경 미검증으로 유지한다.


## 2026-11-09 승인된 외부 조건 별도 후속 처리

- 사용자는 현재 개발 범위의 소스 구현·문서·Release 검증·CI·PR 병합을 종료하고, 실제 비운영 외부 IdP와 GitHub main 보호 규칙 검증은 향후 별도로 수행하도록 승인했다. 따라서 이번 개발 종료 판정에서는 해당 외부 실행을 수행하지 않으며, 이를 성공으로 표시하지 않는다.
- GitHub [Milestone #1 후속 외부 경계 검증](https://github.com/namanvarava/Edan.Tensor/milestone/1)과 [Issue #84 수동 검증 추적](https://github.com/namanvarava/Edan.Tensor/issues/84)을 등록했다. 이 Issue는 실제 Metadata/JWKS, Issuer, Audience, 테스트 Tenant·사용자·안정적인 opaque `sub`, 키 회전 권한과 GitHub 보호 규칙 권한이 제공된 뒤 수동으로 시작한다. 예약 작업이나 자동화는 생성하지 않는다.
- PR #85의 head `27e6fbeb3a543458ba98258023c4ab3b456fa697`는 이 기록을 추가한 뒤 `1ce2a9d4d9cc46fdf4be3a9294dd6f9e7ddc1f9e`로 main에 Squash Merge되었다. 병합된 최신 main에서 실제 Windows 권한으로 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실행했으며 Restore와 Build가 성공하고 Build 경고 0개·오류 0개, 전체 Test 219개 통과·실패 0개·건너뜀 0개를 기록했다. 병합 후 최신 main push [Actions Run #790](https://github.com/namanvarava/Edan.Tensor/actions/runs/42557108330)도 head SHA `1ce2a9d4d9cc46fdf4be3a9294dd6f9e7ddc1f9e`에서 `success`로 완료되었다. Development Bypass는 Development 설정에서만 유지하고 External·운영 인증 검증을 대체하지 않는다.
- PR #85의 병합된 작업 브랜치는 로컬과 원격에서 삭제했고, 최종 로컬 브랜치는 `main`만 남았다. 열린 PR은 0건이며 Issue #84는 사용자가 승인한 후속 외부 검증 Milestone #1을 추적하기 위해 의도적으로 열어 두었다.
- 후속 Issue가 열려 있는 것은 미완료 결함을 숨기기 위한 것이 아니라 사용자가 승인한 별도 외부 검증 마일스톤을 추적하기 위한 것이다. 해당 외부 조건이 제공되면 런북의 수용 기준을 실행하고 결과에 따라 Issue와 Milestone을 닫는다.

## 2026-11-09 로컬 SQLite·Swagger UI·Platform/DataSet/Worker API 흐름 검증 진행

- 주석으로 승인된 1차 범위에 맞춰 API Host에 Development 전용 Swagger UI를 연결하고, Platform·DataSet·Worker 경로가 하나의 OpenAPI 문서에서 확인되도록 유지했다. OpenAPI 문서에는 Bearer JWT 인증 방식과 `X-Tenant-Id` 입력 설명을 추가했으며, `/`, `/health`, `/health/database`는 공개 경로로 남겼다.
- 최초 Swagger 검증은 `swagger-initializer.js`에 `/openapi/v1.json`이 포함된다고 가정하여 실패했다. 실제 응답과 패키지 정적 리소스 구조를 확인한 결과 해당 파일은 기본 샘플 설정을 반환할 수 있고, 실행 설정은 `/swagger/index.js`에 주입되는 것으로 확인했다. 테스트가 실제 실행 경로를 검사하도록 수정했으며, 동일한 원인의 재발을 막기 위해 UI HTML과 실행 스크립트의 HTTP 상태 및 OpenAPI 주소를 함께 검증한다.
- Swagger UI 10.3.1 사용 시 실행 스크립트에 기본 Petstore 주소가 남아 실제 `SwaggerEndpoint` 설정이 반영되지 않는 실패를 재현했다. `MapSwaggerUI` 직접 구성과 설정 객체 주입을 대안으로 비교했지만 같은 패키지 응답 문제를 해결하지 못해, 최소 안전 수정으로 UI 패키지를 9.0.6으로 고정하고 대상 테스트로 실제 설정 주입을 확인했다.
- 제한된 실행 컨텍스트에서는 Testhost가 127.0.0.1 통신 채널에 연결하지 못해 90초 후 중단되었고, 상세 진단 로그는 Testhost가 실행되었지만 연결하지 못한 사실을 기록했다. 실제 Windows 권한으로 재실행하자 SQLite Migration, Development Bypass Seeder, API Host 초기화가 완료되고 대상 테스트가 통과했으므로 이 중단은 제품 코드 실패가 아닌 실행 환경 권한 경계로 분류했다. 이후 통합 테스트는 동일 환경에서 무한 재시도하지 않고 필요한 경우 외부 권한 실행으로 한 번 검증한다.
- 수정 후 `/m:1` 단일 노드 Release 빌드는 경고 0개·오류 0개로 완료했다. OpenAPI 경로·보안 표시와 Swagger UI 설정을 확인하는 대상 통합 테스트 2개, API Host 전체 통합 테스트 23개가 모두 통과했다. Platform Tenant·소속·조직·프로젝트, DataSet schema·SQLite 콘텐츠·quota, Worker validation 작업·Idempotency·상태 조회·DLQ 경계가 현재 통합 테스트 범위에 포함된다.
- 이 절은 현재 작업의 중간 증거이며, 전체 솔루션 exact Release Restore·Build·Test, PR Check, main 병합 후 재검증이 끝나기 전에는 해당 범위를 최종 완료로 판정하지 않는다. 실제 외부 IdP, 운영 DB, LicenseAuthority, GitHub 보호 규칙 HTTP 403은 이 작업에서 성공으로 표시하지 않는다.

## 2026-11-09 PR #88 병합 후 SQLite·Swagger UI·API 흐름 최종 검증

- PR #88의 head `50e6677a07522f056587d9a6b901831f262d0192`는 `8f0f21734863fa4ff0b4cbeb8f241571fa315c74`로 Squash Merge되었다. PR Check [Actions Run #795](https://github.com/namanvarava/Edan.Tensor/actions/runs/42587916847)와 병합 후 main [Actions Run #796](https://github.com/namanvarava/Edan.Tensor/actions/runs/42588150944)은 `EDAN-TENSOR-WIN` 자체 호스팅 Runner에서 Restore·Build·Test·정리 단계를 모두 성공시켰다.
- 병합된 최신 main에서 실제 Windows 권한으로 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 원문 그대로 다시 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, 전체 Test는 220개 통과·실패 0개·건너뜀 0개였다.
- 별도 HTTP Smoke에서 Development API Host가 SQLite Migration과 Seeder를 완료한 뒤 `/health`, `/health/database`, `/swagger/index.html`, `/swagger/index.js`, `/openapi/v1.json`을 모두 200으로 응답했다. OpenAPI 응답에는 `Bearer`와 `X-Tenant-Id`가 포함되었고, 임시 SQLite 데이터는 검증 후 삭제했다.
- 검증 완료 범위는 로컬 SQLite·Migration·Swagger UI와 Platform/DataSet/Worker API 흐름이다. Development Bypass는 명시적 Development 설정에서만 고정 프로필을 사용하고 경고 로그를 남기며, 운영·CI의 인증·인가 경계를 우회하지 않는다. 실제 비운영 외부 IdP, 운영 DB, LicenseAuthority, GitHub 보호 규칙 HTTP 403은 여전히 환경 미검증이며 Issue #84와 Milestone #1의 별도 후속 조건으로 남긴다.

## 2026-11-09 Tenant API 관리자 경계 감사

- API 계약과 권한 문서를 대조한 결과, 조직 생성·프로젝트 생성·사용자 초대 API가 활성 Tenant Member에도 열려 있어 Tenant Administrator만 구조 변경을 수행한다는 계약을 위반한 사실을 재현했다. 이 경계는 `platform-scoped-authorization.md`와 API 사용 안내의 관리자 규칙을 기준으로 판정했다.
- 최소 안전 수정으로 세 엔드포인트에 기존 `TenantAdministratorAuthorizationFilter`를 적용했다. Tenant 경로·활성 소속 검증은 유지하고, Member가 조직·프로젝트·사용자를 변경하지 못하는 통합 회귀 테스트를 추가했다.
- 수정 후 대상 회귀 테스트 1개와 API Host 통합 테스트 23개가 실패 0개·건너뜀 0개로 통과했다. PR #90의 head `f937166d61a7de25a0083765df8fef309355fac9`는 `46650d5ba300f73a4bd93f50334120f58f116c5b`로 Squash Merge되었고 PR Check [Actions Run #799](https://github.com/namanvarava/Edan.Tensor/actions/runs/42592249493)와 병합 후 main [Actions Run #800](https://github.com/namanvarava/Edan.Tensor/actions/runs/42592503861)이 성공했다.
- 병합된 최신 main에서 실제 Windows 권한으로 Restore·Release Build·Release Test를 다시 실행했으며 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 220개 통과·실패 0개·건너뜀 0개를 확인했다. 병합된 작업 브랜치는 로컬·원격에서 삭제했고 최신 main만 남겼다.

## 2026-11-09 Worker 장애 흐름 독립 검증

- Worker Domain 상태 전이·Lease·재시도·DLQ 테스트 21개와 Worker Infrastructure의 프로세스 강제 종료 복구, Coordinator 성공·시작 실패·취소·Lease 갱신 거부·갱신 예외, 외부 전달 응답 유실·5xx·Timeout·종료 인계 테스트 15개를 실제 Windows 권한에서 다시 실행해 모두 통과했다.
- 프로세스 장애 복구 시험은 저장된 체크포인트와 만료 Lease를 새 DbContext에서 복구하고, 새 Worker의 Fencing Token 증가·Tenant 외 작업 접근 차단·이전 Worker의 체크포인트·완료 쓰기 거부를 확인했다. 재시도 한도 초과는 DLQ로 전이되고, 외부 전달 Timeout은 원문 응답 없이 재시도 가능한 오류로 저장된다.
- 최신 Release WorkerHost를 임시 SQLite Platform·DataSet·Worker DB 3개로 실제 기동했다. Migration과 Handler 카탈로그 동기화가 완료되고 `/health`가 HTTP 200을 반환했으며 표준 오류 출력은 0자였다. 검증 후 프로세스와 임시 DB 디렉터리를 삭제했고 `ExistsAfterCleanup=False`를 확인했다.
- 이 단계에서 제품 코드의 추가 결함은 재현되지 않았다. 기존 Lease/Fencing·재시도·DLQ·Timeout·프로세스 복구 구현을 그대로 유지하고, 대상 테스트와 실제 Host Smoke를 재검증 증거로 기록한다. 실제 외부 수신 시스템의 운영 응답 유실 검증은 외부 환경 조건으로 별도 범위에 남긴다.

## 2026-11-09 Work 범위 감사 및 PR #92 병합 후 재검증

- 저장소 내부에서 현재 승인된 범위는 Platform·DataSet·Worker의 SQLite 기반 API 흐름과 Development Swagger·OpenAPI·Tenant·인증 경계·Worker 장애 흐름이다. 이 범위에서 지원하지 않는 `duckdb` 저장소 요청이 `dataset.adapter_unavailable`로 거부되고 콘텐츠를 할당하지 않는 API 회귀 테스트를 추가했다.
- 문서의 기준 SHA와 DataSet 기반 문서를 현재 구현과 대조했다. SQLite 메타데이터 Migration·콘텐츠 어댑터·Tenant 조건 조회·스키마 revision 영속화는 구현 상태로 정리했고, DuckDB·Parquet은 포맷·스키마·용량·동시성·복구 계약 승인 전 구현하지 않도록 명시했다.
- PR #92의 head `21493194eccf2edda4599439b3beb7d32d83e4fe`는 `2fa678e33621e4581172bf87867e20856a6703e6`으로 Squash Merge되었다. PR CI [Actions Run #803](https://github.com/namanvarava/Edan.Tensor/actions/runs/42601199765)와 병합 후 main CI [Actions Run #804](https://github.com/namanvarava/Edan.Tensor/actions/runs/42601510340)는 자체 호스팅 Windows Runner에서 Restore·Build·Test 단계를 모두 성공시켰다.
- 병합 전후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, 전체 Test는 220개 통과·실패 0개·건너뜀 0개였다. 추가한 DataSet API 경계 테스트는 1개 통과했다.
- 병합 후 로컬 `main`과 `origin/main`은 `2fa678e33621e4581172bf87867e20856a6703e6`으로 일치하고 작업 디렉터리는 clean 상태다. PR #92의 작업 브랜치 `codex/work-scope-audit-20261109`는 로컬·원격에서 삭제했고, 열린 PR은 0개다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB, 외부 수신 시스템과 GitHub 보호 규칙 HTTP 403은 이 Work 실행에서 검증하지 않았다. 이 조건은 승인된 별도 Milestone #1·Issue #84의 재개 조건으로 유지한다. Development Bypass는 Development에서만 활성화되고 External·운영 설정에는 포함되지 않는다.

## 2026-11-09 API 데이터베이스 health 경계 보강 및 PR #94 병합 후 재검증

- 기존 `/health`는 실제 데이터베이스를 확인하지 않았고 `/health/database`는 Platform DB만 확인했다. 이 원인을 재현한 뒤 Platform·Worker·DataSet 메타데이터와 DataSet 콘텐츠 SQLite를 모두 확인하는 `DatabaseHealthService`를 추가했다. 어느 하나라도 연결 또는 기본 테이블 읽기에 실패하면 두 health 경로가 HTTP 503을 반환하고, 응답에는 데이터베이스별 성공 여부만 포함하도록 했다.
- `/health`·`/health/database`의 네 개 저장소 상태와 OpenAPI 경계를 확인하는 API Host 통합 테스트를 추가했다. API Host 통합 테스트는 24개 통과·실패 0개·건너뜀 0개였다.
- PR #94의 head `ac9753f4dd18bd976ed04fc56ff4c2f6aa18adda`는 `f2eaafb2fc16a772ebde468af9457e1ad594ef8e`로 Squash Merge되었다. PR CI [Actions Run #807](https://github.com/namanvarava/Edan.Tensor/actions/runs/42604564612)와 병합 후 main push CI [Actions Run #808](https://github.com/namanvarava/Edan.Tensor/actions/runs/42604905013)는 Restore·Build·Test 단계를 모두 성공시켰다.
- 병합 후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, 전체 Test는 221개 통과·실패 0개·건너뜀 0개였다. 로컬 `main`과 `origin/main`은 `f2eaafb2fc16a772ebde468af9457e1ad594ef8e`로 일치했고 작업 디렉터리는 clean 상태였다.
- 병합된 작업 브랜치 `codex/health-observability-20261109`는 로컬·원격에서 삭제했다. 실제 비운영 외부 IdP, 운영 DB, 외부 수신 시스템, GitHub 보호 규칙 HTTP 403은 이 변경으로 검증하지 않았으며 Issue #84·Milestone #1의 별도 재개 조건으로 유지한다.

## 2026-11-09 API 계약·DataSet 검증 Idempotency 및 PR #96 병합 후 재검증

- API 계약 감사에서 DataSet 검증 요청이 Worker Idempotency-Key만 사용하고 검증 실행 레코드에는 키를 저장하지 않아 같은 요청마다 새 실행이 만들어질 수 있는 결함을 재현했다. 실행 레코드와 Worker 작업 사이의 키 충돌에서는 새로 만든 실행을 정리하지 않아 고아 실행이 남을 수 있는 경계도 확인했다.
- Tenant 범위 Idempotency-Key를 검증 실행에 영속화하고 고유 인덱스를 추가했다. 같은 키와 같은 DataSet·버전·스키마·프로필 revision은 기존 실행과 작업으로 수렴하고 HTTP 202를 반환하며, 다른 요청은 `dataset.validation.idempotency_conflict`와 HTTP 409를 반환한다. Worker 키 충돌로 새 실행이 연결되지 않으면 새 실행만 정리한다.
- OpenAPI 문서에 검증·Worker 등록의 필수 `Idempotency-Key`와 DLQ 재처리의 필수 `Replay-Request-Id`를 추가하고 실제 JSON 문서에서 필수 상태를 확인했다. API Host 통합 테스트 24개, DataSet Infrastructure 테스트 10개, OpenAPI 단독 테스트 1개가 통과했다.
- PR #96의 head `c56846a951fee26e4b033999d96f4f720c82b2e0`는 `779a5a31e89b1dd21be9573e9cecd3246309bfd2`로 Squash Merge되었다. PR CI [Actions Run #811](https://github.com/namanvarava/Edan.Tensor/actions/runs/42607847138)과 병합 후 main push CI [Actions Run #812](https://github.com/namanvarava/Edan.Tensor/actions/runs/42608089151)는 Restore·Build·Test 단계를 모두 성공시켰다.
- 병합 후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, 전체 Test는 221개 통과·실패 0개·건너뜀 0개였다. 로컬 `main`과 `origin/main`은 `779a5a31e89b1dd21be9573e9cecd3246309bfd2`로 일치했고 작업 디렉터리는 clean 상태였다.
- 병합된 작업 브랜치 `codex/api-validation-idempotency-20261109`는 로컬·원격에서 삭제했다. 실제 외부 IdP, 운영 DB, 외부 수신 시스템, GitHub 보호 규칙 HTTP 403과 LicenseAuthority는 이 변경으로 검증하지 않았으며 Issue #84·Milestone #1 및 승인된 후속 단계의 재개 조건으로 유지한다.

## 2026-11-09 Worker 등록 실패 보상 및 PR #98 병합 후 재검증

- Worker 등록 순서에서 External Delivery와 DataSet Validation을 먼저 저장한 뒤 Worker 작업을 등록하는 구조를 추적했다. 같은 Tenant의 Idempotency-Key가 다른 Worker 작업과 충돌하거나 작업 유형이 등록되지 않으면 HTTP 409·503을 반환하지만, 기존 구현은 새 비즈니스 레코드를 남기는 고아 데이터 결함이 있었다.
- External Delivery에는 Tenant·전달 ID·Idempotency-Key와 `Pending`, 시도 0회, 미완료 조건을 모두 확인하는 내부 보상 삭제를 추가했다. Worker 키 충돌과 작업 유형 미사용 경로에서 이번 요청이 새로 만든 레코드만 제거하며, 기존 전달과 충돌한 Worker 작업은 보존한다.
- DataSet Validation에는 일반 사용자의 진행 중 결과 삭제 계약과 분리된 내부 보상 삭제를 추가했다. `Queued`, Fencing Token 0, 처리 행 0건, 미완료 조건을 만족하는 새 실행만 제거하므로 이미 시작된 실행이나 사용자 삭제 경계는 완화하지 않는다.
- External Delivery·Worker·DataSet Validation의 `Idempotency-Key` HTTP 경계에서 앞뒤 공백을 정규화하여 조회·저장·Worker 작업의 동일 키 계약을 일치시켰다. API 통합 회귀는 정상 재사용, Worker 키 충돌 후 외부 전달 미잔류, Worker 유형 미사용 후 외부 전달 미잔류, DataSet 유형 미사용 후 검증 실행 미잔류를 확인했다.
- PR #98의 head `b91248d8d6b61e54eb4e53d59777cf63925f31b2`는 `eb0765877c48dda06207b027f481f614f3301b9a`로 Squash Merge되었다. PR CI [Actions Run #815](https://github.com/namanvarava/Edan.Tensor/actions/runs/42611137716)와 병합 후 main push CI [Actions Run #816](https://github.com/namanvarava/Edan.Tensor/actions/runs/42611459390)은 자체 호스팅 Windows Runner에서 Restore·Build·Test·정리 단계를 모두 성공시켰다.
- 변경 브랜치에서 대상 API 통합 4개와 Worker 외부 전달·Coordinator 장애 테스트 13개를 통과시켰다. 병합 후 실제 Windows 권한에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행했으며 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 223개 통과·실패 0개·건너뜀 0개를 확인했다.
- 병합 후 로컬 `main`과 `origin/main`은 `eb0765877c48dda06207b027f481f614f3301b9a`로 일치하고 작업 디렉터리는 clean 상태다. 병합된 작업 브랜치 `codex/external-delivery-rollback-20261109`는 로컬·원격에서 삭제했고 현재 원격 브랜치는 `main`만 남았다.
- 실제 비운영 외부 IdP Metadata·JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB, 외부 수신 시스템과 GitHub 보호 규칙 HTTP 403은 이번 변경에서 검증하지 않았다. Development Bypass는 고정된 RFC 예약 주소 시뮬레이션 프로필을 Development에서만 사용하며 실제 외부 IdP 검증을 대체하지 않는다. 해당 조건은 Issue #84·Milestone #1 및 승인된 후속 단계의 재개 조건으로 유지한다.

## 2026-11-09 Work 전체 실행 감사 및 현재 main 재검증

- 현재 로컬 `main`의 기준 SHA는 `632f894f664e262d3bc0e62be09c218cac6b4c27`이며, 검증을 시작할 때 작업 디렉터리는 clean 상태이고 로컬·원격 작업 브랜치는 `main`만 유지했다. 이 문서 변경을 제출하기 위한 작업 브랜치는 검증 후 별도로 만들었다. 열린 Pull Request는 없고, Issue #84만 사용자가 승인한 외부 경계 검증 Milestone #1 추적용으로 열려 있다.
- 현재 Work에서 승인된 저장소 내부 범위는 API 계약·공통 오류·OpenAPI/Swagger, Tenant·인증·인가 경계, 로컬 SQLite Migration과 DataSet 콘텐츠·Quota·검증, Worker Lease/Fencing·재시도·DLQ·복구·외부 전달 Idempotency 흐름이다. 이 범위의 실제 구현을 재점검했으며 새 결함은 재현되지 않았다.
- DuckDB·Parquet 어댑터, Commercial 상품·요금·결제·권리, LicenseAuthority 발급·검증 업무, WPF 화면은 포맷·권리·서명·만료·화면 수용 기준이 승인된 계약으로 저장소에 존재하지 않는다. 기존 문서의 제외 범위를 유지하고 임의의 공개 API·데이터 형식·라이선스 발급 규칙을 추가하지 않았다. 해당 단계의 재개 조건은 각 업무 계약과 수용 테스트의 승인이다.
- 현재 main에서 다음 명령을 실제 실행했다.
  - `dotnet restore EDAN.Tensor.sln`: 성공
  - `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`: 성공, 경고 0개·오류 0개
  - `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`: 종료 코드 0, 전체 223개 통과·실패 0개·건너뜀 0개
- 전체 테스트 중 API Host 통합 테스트 26개도 별도로 실행하여 26개 통과·실패 0개·건너뜀 0개를 확인했다. 테스트에는 Development Bypass 차단, 외부 IdP/JWKS 로컬 경계, Swagger UI·OpenAPI, health, Tenant 격리, DataSet SQLite 콘텐츠·Quota·검증, Worker Idempotency·DLQ·보상 정리가 포함된다.
- 이 기록은 저장소 내부 범위의 최신 로컬 재검증 증거이며 전체 제품 개발 완료 선언이 아니다. 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB, 실제 외부 수신 시스템, GitHub main 보호 규칙 HTTP 403은 Work에서 실행하지 않았고 성공으로 표시하지 않는다. 해당 조건이 제공되면 Issue #84와 Milestone #1의 런북을 수동 실행한 뒤 별도 판정한다.

## 2026-11-09 SQLite 백업·복구 런북과 로컬 콘텐츠 복구 회귀 검증

- 운영 검증 단계에 필요한 백업·복구 증거가 기존 문서에 흩어져 있어, 네 SQLite 저장소를 함께 다루는 한국어 런북 `docs/operations/platform-database-backup-restore.md`를 추가했다. 런북은 WAL·SHM 확인, `integrity_check`, `VACUUM INTO` 또는 동등한 온라인 백업, 별도 경로 복구, Migration·health·Tenant·Worker Lease/DLQ 확인과 실패 판정 기준을 정의한다.
- 단순 파일 복사만 안내하면 WAL에 남은 커밋과 DataSet 콘텐츠가 누락될 수 있으므로, DataSet Infrastructure에 `VACUUM INTO`로 별도 백업을 생성한 뒤 복구 어댑터에서 동일한 불투명 Storage Reference의 콘텐츠를 읽는 회귀 테스트를 추가했다. 이 테스트는 운영 백업 성공을 대신하지 않으며 로컬 SQLite 콘텐츠 백업 시점의 정합성만 검증한다.
- 변경 전에는 네 데이터베이스의 동시 쓰기 중지, 백업 세트 일치, 복구 후 Tenant 격리와 Worker Lease/DLQ 확인 절차가 하나의 문서로 고정되지 않은 영향이 있었다. 원본을 덮어쓰거나 운영 DB 공급자를 가정하는 대안은 적용하지 않고, 현재 구현과 운영 결정의 경계를 보존하는 최소 문서·테스트를 추가했다.
- 대상 테스트 `SqliteBackupAndRestorePreservesContentAtTheBackupPoint`는 Release 환경에서 1개 통과·실패 0개·건너뜀 0개로 확인했다. 변경 브랜치의 전체 회귀 테스트는 단일 MSBuild 노드와 최소 콘솔 로거를 사용해 224개 통과·실패 0개·건너뜀 0개를 기록했다. 요구된 인자 그대로의 PR Check와 병합 후 main CI 결과는 이 문서의 후속 기록에 추가한다.
- 운영 DB 공급자, 백업 보존 기간, RPO·RTO, 암호화·외부 Object Storage와 실제 장애 복구 시간은 아직 결정·검증하지 않았다. 이 항목은 운영 DB와 인프라가 제공된 뒤 별도 실행해야 하며 현재 Work의 로컬 SQLite 증거로 완료 처리하지 않는다.

## 2026-11-09 PR #101 병합 후 SQLite 백업·복구 검증 재확인

- PR #101의 head `1cd3f4476963015b34a4b40c21df31617418ebe8`는 `2881bd54aeb38bd40d1dd494b2a158620912b0bf`로 Squash Merge되었다. PR Check [Actions Run #821](https://github.com/namanvarava/Edan.Tensor/actions/runs/42616238326)와 병합 후 main [Actions Run #822](https://github.com/namanvarava/Edan.Tensor/actions/runs/42616515046)은 자체 호스팅 Windows Runner에서 요구된 Restore·Release Build·Release Test 단계를 모두 성공시켰다.
- PR Check와 main CI는 각각 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실행했으며, Build는 경고 0개·오류 0개, Test는 전체 224개 통과·실패 0개·건너뜀 0개로 완료했다. `SqliteBackupAndRestorePreservesContentAtTheBackupPoint`도 변경 브랜치 대상 테스트에서 통과했다.
- 병합 후 로컬 Work에서 같은 명령을 원문 그대로 다시 실행한 결과, 기본 병렬 MSBuild가 현재 실행 환경의 프로세스 생성 제한에서 출력 없이 종료 코드 1을 반환했다. 상세 로그는 `_FilterRestoreGraphProjectInputItems` 단계의 병렬 MSBuild 실패를 보여 주었고, 단일 노드 `-m:1` 복원·빌드는 경고 0개·오류 0개로 성공했다. 단일 노드 전체 테스트는 첫 Testhost 연결 90초 제한에 걸려 중단되었으므로 로컬 실패를 제품 코드 실패로 판정하지 않으며, CI의 동일 명령 성공을 최종 Release 회귀 증거로 사용한다.
- 병합 후 로컬 `main`과 `origin/main`은 `2881bd54aeb38bd40d1dd494b2a158620912b0bf`로 일치하고 작업 디렉터리는 clean 상태다. 병합된 `codex/add-sqlite-backup-restore-runbook-20261109`는 로컬·원격에서 삭제했으며 원격 `refs/heads`에는 `main`만 남았다.
- 이 재검증은 로컬 SQLite 백업·복구 절차와 콘텐츠 시점 정합성만 증명한다. 운영 DB 공급자, RPO·RTO, 보존·암호화·외부 저장소, 실제 장애 복구 시간과 외부 IdP·GitHub 보호 규칙은 여전히 별도 환경 조건이다.

## 2026-11-09 저장 용량 정책 관리 API 구현 및 SQLite 경계 수정

- Work 범위 감사에서 저장 용량 정책 도메인·Migration·관리 서비스·DTO는 존재하지만 Tenant 관리자용 목록·생성·수정·활성화·감사 이력 HTTP 경로가 없어 DataSet quota 계약을 외부에서 사용할 수 없는 미완료 상태임을 확인했다. 영향 범위는 정책을 등록하거나 변경 이력을 조회할 수 없고, 기존의 Tenant 관리자·대상 소속·버전 조건을 API에서 검증할 진입점이 없다는 점이었다.
- 승인된 기존 계약을 확장하지 않는 최소 수정으로 `PlatformEndpoints`에 Tenant 관리자 전용 정책 API를 추가했다. 목록과 감사 이력은 1~100건 페이지 범위와 `totalCount`를 반환하고, 생성·수정·활성화는 정책 대상 Tenant 소속과 `expectedVersion`을 확인한다. 중복 정책은 409 `quota_policy.conflict`, 오래된 버전은 409 `quota_policy.version_conflict`로 반환하며 변경 감사 기록은 기존 원자적 서비스 경계를 사용한다.
- 최초 대상 회귀 테스트에서 감사 이력 GET이 HTTP 500으로 재현되었다. 실제 응답 로그와 스택 추적은 SQLite 공급자가 `DateTimeOffset`을 SQL `ORDER BY`로 변환하지 못하고 `GetHistoryAsync`의 `CountAsync` 단계에서 `NotSupportedException`을 발생시킨다고 기록했다. 서버 정렬을 그대로 재시도하는 대안은 SQLite 경계에서 재발하므로, 기존 감사 저장소의 패턴과 동일하게 Tenant·정책으로 먼저 제한한 뒤 UTC 시각과 식별자로 메모리 정렬·페이지 절단을 수행하는 최소 수정으로 해결했다.
- 수정 후 실제 Windows 권한에서 다음 검증을 수행했다.
  - 저장 용량 정책 API 대상 회귀 테스트: 1개 통과·실패 0개·건너뜀 0개
  - API Host 통합 테스트: 27개 통과·실패 0개·건너뜀 0개
  - `dotnet restore EDAN.Tensor.sln -m:1`: 성공
  - `dotnet build EDAN.Tensor.sln --no-restore --configuration Release -m:1`: 성공, 경고 0개·오류 0개
  - `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release -m:1 --logger "console;verbosity=minimal"`: 전체 225개 통과·실패 0개·건너뜀 0개
- 단일 노드 옵션은 현재 Work 실행 환경의 프로세스·Testhost 제한 때문에 로컬에서 재현 가능한 검증을 위해 사용했다. CI에서는 저장소가 정의한 옵션 없는 원문 Release 명령을 별도로 확인해야 하며, PR Check·main 병합 후 CI 결과가 추가되기 전에는 이 변경을 최종 통합 완료로 판정하지 않는다.
- 이 변경은 SQLite 기반 Platform/DataSet quota 관리 API 범위만 다룬다. DuckDB·Parquet·Commercial·LicenseAuthority·WPF, 운영 DB·실제 외부 IdP·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 이 검증에 포함하지 않으며, 승인된 외부 조건 후속 Issue와 해당 계약 승인 후 별도로 수행한다.

## 2026-11-09 PR #103 병합 후 저장 용량 정책 API 최종 검증

- PR #103의 head `dde620d4af957ac2ce6bfdd238bdfe73e666b535`는 `02a14d23af1e8bcdf5fb7924babd7f06a3da4da9`로 Squash Merge되었다. PR Check [Actions Run #825](https://github.com/namanvarava/Edan.Tensor/actions/runs/42622162668)는 Restore·Build·Test를 모두 성공시켰고, 병합 후 main [Actions Run #826](https://github.com/namanvarava/Edan.Tensor/actions/runs/42622511237)도 같은 단계를 모두 성공시켰다. 두 Run 모두 `EDAN-TENSOR-WIN` 자체 호스팅 Windows Runner의 `build-and-test` 작업에서 성공했다.
- 병합 후 로컬 main에서 실제 Windows 권한으로 `dotnet restore EDAN.Tensor.sln -m:1`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release -m:1`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release -m:1 --logger "console;verbosity=minimal"`를 다시 실행했다. Restore는 성공했고 Build는 경고 0개·오류 0개, 전체 Test는 SharedKernel 11·Platform 27/30/12·API 27·Worker 21/66·DataSet 6/14/11로 총 225개 통과·실패 0개·건너뜀 0개였다.
- 최종 로컬 `main`과 `origin/main`은 `02a14d23af1e8bcdf5fb7924babd7f06a3da4da9`로 일치했다. 원격 refs 확인에서 원격 브랜치는 `main`만 남았고, 병합된 `codex/implement-storage-quota-policy-api-20261109`는 원격·로컬에서 삭제했다. Squash Merge로 원래 커밋이 main의 조상으로 표시되지 않아 로컬 안전 삭제가 거부되었으므로, PR 병합 SHA와 main CI 성공을 재확인한 뒤 해당 단일 브랜치만 명시적으로 삭제했다.
- 최종 상태 확인 중 제한된 실행 컨텍스트의 최초 `git ls-remote`는 스레드 생성 오류로 실패했다. 이는 저장소·코드 오류가 아니라 실행 환경의 일시적 네트워크 프로세스 경계였으며, 같은 명령을 실제 Windows 권한으로 재실행해 `refs/heads/main`만 반환되는 것을 확인했다. 무한 재시도나 캐시 삭제는 수행하지 않았다.
- 저장 용량 정책 관리 API와 SQLite 감사 이력 경계는 PR·main CI와 병합 후 로컬 Release 회귀로 검증되었다. 이 증거는 SQLite 기반 Platform/DataSet quota 범위에 한정되며, DuckDB·Parquet·Commercial·LicenseAuthority·WPF, 운영 DB·실제 비운영 외부 IdP·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 여전히 별도 외부 조건 또는 승인된 계약 대기 범위다.

## 2026-11-09 저장 용량 정책 버전 충돌 오류 코드 고정

- 내부 계약 감사에서 `StorageQuotaPolicy`가 오래된 버전을 한국어 예외 메시지로만 알리고, API 서비스가 그 메시지에 `버전` 문자열이 있는지 검색하여 HTTP 409 오류 코드를 결정하는 취약한 결합을 확인했다. 메시지 변경이나 다국어 처리 시 `quota_policy.version_conflict`가 사라질 수 있으므로 API의 오류 계약을 안정적으로 보장하지 못하는 영향이 있었다.
- 최소 안전 수정으로 도메인은 공통 `BusinessRuleViolationException`에 `quota_policy.version_conflict` 코드를 담아 던지고, API 서비스는 메시지 검색 대신 이 코드를 정확히 검사하도록 변경했다. 기존 HTTP 409 응답과 외부 API 코드는 유지했으며, 오래된 버전에서는 정책 값·활성 상태·Version을 변경하지 않는 도메인 회귀 테스트 2개를 추가했다.
- 최초 대상 테스트는 새 테스트 파일의 `Xunit` using 누락으로 `FactAttribute` 컴파일 오류가 발생했다. 로그에서 테스트 코드 컴파일 단계의 네임스페이스 누락임을 확인하고 `using Xunit;`만 추가한 뒤 같은 테스트를 재실행했다. 제품 코드 결함으로 오인해 무관한 재시도나 인증·저장소 변경은 하지 않았다.
- 수정 후 실제 Windows 권한에서 저장 용량 정책 도메인 테스트 2개, API 동시성 회귀 테스트 1개가 통과했다. 단일 노드 Release 전체 검증은 `dotnet restore EDAN.Tensor.sln -m:1` 성공, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release -m:1` 경고 0개·오류 0개, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release -m:1 --logger "console;verbosity=minimal"` 전체 227개 통과·실패 0개·건너뜀 0개로 종료 코드 0을 기록했다.
- 이 변경은 저장 용량 정책의 내부 도메인 오류 계약만 안정화하며, PR Check·main 병합 후 CI 증거를 이 문서의 후속 기록에 추가해야 한다. DuckDB·Parquet·Commercial·LicenseAuthority·WPF와 실제 운영·외부 환경 조건은 계속 별도 범위다.

## 2026-11-09 PR #105 병합 후 저장 용량 정책 오류 계약 최종 검증

- PR #105의 head `d38218b1bee97289b4729ec2b67d231cdafc958e`는 `4022281098003a31ebafb60f9ae4839c84a97efa`로 Squash Merge되었다. PR Check [Actions Run #829](https://github.com/namanvarava/Edan.Tensor/actions/runs/42625496491)와 병합 후 main [Actions Run #830](https://github.com/namanvarava/Edan.Tensor/actions/runs/42625811019)은 자체 호스팅 Windows Runner의 `build-and-test` 작업에서 Restore·Build·Test·정리를 모두 성공시켰다.
- 변경 브랜치에서 실제 Windows 권한으로 단일 노드 Release Restore·Build·Test를 실행해 Restore 성공, Build 경고 0개·오류 0개, 전체 227개 통과·실패 0개·건너뜀 0개를 확인했다. Platform Domain은 29개, API 통합은 27개이며 새 버전 충돌 도메인 테스트 2개와 API 회귀 테스트 1개가 포함된다.
- PR #105의 병합된 작업 브랜치 `codex/typed-quota-version-conflict-20261109`는 원격·로컬에서 삭제했다. 병합 후 로컬 `main`과 `origin/main`은 `4022281098003a31ebafb60f9ae4839c84a97efa`로 일치하고, 원격 refs와 로컬 브랜치에는 `main`만 남았다. 열린 PR은 0개다.
- 이 변경은 `quota_policy.version_conflict`의 내부 오류 계약과 저장 용량 정책 테스트를 안정화한 범위다. 실제 비운영 외부 IdP, 운영 DB, 외부 수신 시스템, GitHub 보호 규칙 HTTP 403, DuckDB·Parquet·Commercial·LicenseAuthority·WPF는 여전히 별도 외부 조건 또는 승인된 후속 계약 범위다.

## 2026-11-09 API 오류·인증 경계 및 Worker 장애 흐름 최종 검증

- PR #107의 API 공통 오류 계약 변경은 예외 메시지 검색에 의존하던 quota 버전 충돌 분기를 안정적인 `BusinessRuleViolationException` 코드 분기로 교체하고, 처리되지 않은 예외를 공통 `ApiError` 응답으로 변환했다. 내부 예외 상세는 로그에만 남기고 알 수 없는 예외·취소·잘못된 요청의 외부 설명에는 노출하지 않는다. API 예외 회귀 테스트는 안정적인 업무 코드와 내부 메시지 비노출을 확인한다.
- PR #107의 head `f2f7b4780f2e1b379881becf9df7674254f49196`는 `a083a8cbaf3727be8ebaf75521c8a717154f78ba`로 Squash Merge되었고, 병합 후 main CI [Actions Run #42633088566](https://github.com/namanvarava/Edan.Tensor/actions/runs/42633088566)에서 Restore·Release Build·Release Test를 성공시켰다.
- PR #108의 인증 경계 감사에서 `Authentication:BypassSubject`와 `DevelopmentData:TenantCode` 설정이 고정 프로필을 재정의할 수 있던 결함을 확인했다. 최소 수정으로 Bypass의 안정적인 `sub`와 테스트 Tenant를 `DevelopmentBypassExternalIdentityProfile.Fixed`에서만 공급하고, 개발 역할 설정과 Tenant·소속·관리자 권한 검사는 유지했다. 악의적인 설정 재정의가 무시되는 통합 회귀 테스트를 추가했다.
- PR #108의 head `6715f06f95e4076217d1a5887ad8fcd7b9d2273c`는 `1f4eda2f25f5c6a4a511a2cbb2edf008cccae3df`로 Squash Merge되었다. 병합 후 main [Actions Run #42636371195](https://github.com/namanvarava/Edan.Tensor/actions/runs/42636371195)의 `build-and-test` 작업은 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실행했다. Restore·Build·Test가 모두 성공했고 Build는 경고 0개·오류 0개, 전체 테스트는 230개 통과·실패 0개·건너뜀 0개였다. Worker Infrastructure 66개와 API 통합 30개도 모두 통과했다.
- 현재 Worker 장애 흐름은 실제 소스와 자동 검증으로 확인했다. Dispatcher는 신규 작업 차단, Tenant별 cursor LRU, 만료 Lease 복구와 제한된 종료 인계 예산을 사용한다. Coordinator와 저장소 테스트는 Lease 갱신 거부·예외, Handler 시작 실패·취소, 실행 취소, 체크포인트 저장, Fencing Token 불일치, 재시도·DLQ와 Tenant 격리를 검증한다. 별도 ProcessProbe 강제 종료 테스트는 새 DbContext에서 체크포인트 복구, Fencing Token 증가, 오래된 쓰기 거부와 정상 완료를 확인한다. 외부 전달 테스트는 timeout·5xx·응답 유실·동일 Idempotency Key 재시도를 검증한다.
- 로컬에서 Worker Infrastructure 전체를 재실행했을 때 프로젝트 빌드는 성공했으나 Testhost가 90초 동안 연결되지 않아 종료되었다. 로그는 `vstest.console`의 testhost 연결 시간 초과를 기록했으며 제품 테스트 실패나 코드 예외는 기록하지 않았다. 동일한 원문 명령은 위 Windows 자체 호스팅 main CI에서 성공했으므로, 로컬 결과는 실행 환경 미검증으로 분류하고 무한 재시도·캐시 삭제는 수행하지 않았다.
- PR #108 병합 후 로컬 `main`은 `1f4eda2f25f5c6a4a511a2cbb2edf008cccae3df`로 동기화되었고 작업 디렉터리에는 변경이 없다. 병합된 `codex/tenant-auth-worker-recovery-20261109`는 원격 삭제 성공 응답과 GitHub API의 Branch 404로 원격 삭제를 확인했으며, 로컬 브랜치 목록에도 `main`만 남았다.
- Development Bypass는 `Development` 환경에서만 고정된 RFC 예약 Metadata/JWKS 주소, Issuer, Audience, `EDAN-NONPROD` Tenant, `edan-nonprod-test-user`, `edan-nonprod-test-subject`, 개발 키 회전 권한 식별자를 시뮬레이션한다. 이 값은 실제 외부 IdP 권한이나 키 회전 검증이 아니며 운영·External 구성에는 포함되지 않는다. 실제 비운영 IdP, 운영 DB, 외부 수신 시스템, GitHub 보호 규칙 HTTP 403과 LicenseAuthority는 여전히 별도 외부 조건 또는 승인된 후속 계약 범위다.

## 2026-11-09 PR #109 병합 후 로컬 Release 명령 재확인

- PR #109 병합 후 최신 로컬 `main`은 `b7da77c3ba31354a17477c7d81b5612e94def592`이다. 작업 디렉터리는 clean이고 로컬 브랜치는 `main`만 남아 있다.
- 요구된 원문 `dotnet restore EDAN.Tensor.sln`은 현재 Work 실행 환경에서 출력 없이 종료 코드 1로 끝났다. 같은 복원은 `dotnet restore EDAN.Tensor.sln -m:1 -v:minimal`에서 모든 프로젝트가 최신 상태라는 메시지와 함께 종료 코드 0으로 완료했다.
- 요구된 원문 `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`는 `EDAN.Tensor.SharedKernel` 출력 뒤 경고 0개·오류 0개를 표시하고 종료 코드 1로 끝났다. 단일 MSBuild 노드의 `dotnet build EDAN.Tensor.sln --no-restore --configuration Release -m:1 -v:minimal`은 2분 46초 후 경고 0개·오류 0개와 종료 코드 0으로 완료했다.
- 요구된 원문 `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`도 현재 환경에서 출력 없이 종료 코드 1로 끝났다. 직전 Worker Infrastructure 실행은 프로젝트 빌드 후 `vstest.console`이 90초 동안 testhost에 연결하지 못해 중단되었고, 제품 테스트 실패·코드 예외·assertion 로그는 없었다. 이 결과는 제품 실패가 아니라 현재 실행 환경의 프로세스·testhost 경계로 분류한다.
- 동일한 옵션 없는 세 원문 명령은 PR #109 병합 후 main CI [Actions Run #42638182485](https://github.com/namanvarava/Edan.Tensor/actions/runs/42638182485)에서 Windows 자체 호스팅 Runner로 성공했고, 전체 230개 테스트가 통과했다. 따라서 로컬 명령 실패는 성공으로 숨기지 않고 환경 미검증으로 남기며, 캐시 삭제나 무한 재시도는 수행하지 않는다.

## 2026-11-10 저장소 내부 잔여 범위 감사

- 최신 main은 `abe0c5ad55928b7dd22121484c07cfcfc4f952a5`이며 작업 디렉터리는 clean이고 열린 PR은 없다. 현재 main CI [Actions Run #42640048471](https://github.com/namanvarava/Edan.Tensor/actions/runs/42640048471)은 원문 Restore·Release Build·Release Test를 성공시켰고 전체 230개 테스트가 통과했다.
- DataSet의 승인된 로컬 대체 범위는 전용 SQLite 콘텐츠 어댑터, 불투명 Storage Reference, Tenant·Organization·Project 격리, 계층형 quota, UTF-8 CSV·JSON 객체 배열 검증, 백업 시점 복구 회귀다. `docs/architecture/dataset-foundation.md`는 DuckDB·Parquet의 포맷·스키마 매핑·용량 계산·동시 쓰기·복구·Migration 호환성 계약이 승인되기 전 구현하지 않도록 명시한다. 따라서 현재 Work에서는 가짜 DuckDB/Parquet 어댑터를 추가하지 않는다.
- Commercial과 Licensing은 Contracts·Domain·Application·Infrastructure 프로젝트 경계와 아키텍처 회귀만 존재하며 상품·요금·결제·권리·만료 또는 발급·서명·검증 업무 계약이 없다. LicenseAuthority.Host도 Foundation 상태만 제공한다. 발급 규칙, LicenseId/Site/Tenant 범위, 제품·기능 Claim, KeyId·서명 형식, 폐기·오프라인·만료 정책이 승인되면 별도 LicenseAuthority와 개발 라이선스 1개를 마지막 내부 단계로 구현한다.
- WPF 프로젝트나 승인된 화면·업무 흐름·클라이언트 API 계약은 현재 저장소에 없다. 임의의 클라이언트 화면이나 공개 API를 만들지 않고, 향후 화면 수용 기준과 API 연동 계약이 제공될 때 진행한다.
- 따라서 현재 저장소 내부에서 즉시 구현할 수 있는 필수 결함은 재현되지 않았고, 다음 구현 재개 조건은 DuckDB/Parquet 계약 승인, Commercial 계약 승인, Licensing·LicenseAuthority 발급 계약 승인, WPF 클라이언트 계약 승인이다. 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 Issue #84와 Milestone #1의 외부 조건으로 계속 분리한다.

## 2026-11-10 PR #111 병합 및 최신 main 상태 재확인

- PR #111 `docs: 잔여 내부 범위와 최신 기준 동기화`의 head `858b8a554b2fef611a8279870b54af3e658353a7`은 main에 `319b61ee6b2523f0257908627748c48185f298df`로 Squash Merge되었다. PR Check [Actions Run #42641196808](https://github.com/namanvarava/Edan.Tensor/actions/runs/42641196808)과 병합 후 main [Actions Run #42641569157](https://github.com/namanvarava/Edan.Tensor/actions/runs/42641569157)의 `build-and-test`는 모두 성공했다.
- 병합 후 로컬 `main`을 `origin/main`의 `319b61ee6b2523f0257908627748c48185f298df`로 fast-forward했고, 작업 디렉터리는 clean이다. 열린 PR은 0건이며, 병합된 `codex/audit-remaining-internal-scope-20261110`은 원격 Branch API 404와 `origin` 원격 브랜치 목록으로 삭제를 확인했고 로컬 브랜치 목록에도 `main`만 남았다.
- 이 문서 병합으로 저장소 내부 잔여 범위 감사와 최신 main 기준을 기록했다. DuckDB·Parquet·Commercial·LicenseAuthority·WPF의 승인 계약 부재와 실제 비운영 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403의 환경 미검증 상태는 변경되지 않았다.

## 2026-11-10 API Host Development Smoke의 loopback 권한 미검증

- 검증 시작 기준 main은 `6b9ec32262fd1be75ea77c19e4dd3ccc1b61dc73`였다. Host 단독 Debug 빌드는 처음에 VBCSCompiler named pipe 접근 거부와 `obj\\Debug` 출력 DLL 파일 잠금으로 종료되었고, 관련 프로세스가 남아 있지 않은 상태에서 `-m:1 -p:UseSharedCompilation=false`를 사용한 재현 가능한 빌드는 경고 0개·오류 0개로 통과했다. 이 실패는 제품 코드 오류가 아니라 현재 Work의 컴파일러 프로세스·파일 잠금 경계로 분류한다.
- `ASPNETCORE_ENVIRONMENT=Development`와 `dotnet run --no-build --project src/Hosts/EDAN.Tensor.ApiHost --no-launch-profile --urls http://127.0.0.1:5127`로 Host를 기동했다. 로그에서 SQLite Migration 적용, `EDAN-NONPROD` Tenant와 `edan-nonprod-test-subject` 초기화, `http://127.0.0.1:5127` 수신 대기를 확인했다. Development Bypass는 실제 외부 IdP가 아니라 고정 시뮬레이션 프로필을 사용했다.
- 별도 HTTP 클라이언트로 `/health`, `/health/database`, `/openapi/v1.json`, `/swagger/index.html`, 보호 API를 호출했지만 모든 요청이 동일한 “액세스 권한에 의해 숨겨진 소켓” 오류로 실패했다. 따라서 HTTP 상태·응답 본문·Swagger UI와 보호 API의 실제 loopback 호출은 성공으로 판정하지 않고 환경 미검증으로 남긴다. Host 로그의 DataProtection 키 저장 권한 거부도 확인했으며, 이번 Work에서는 운영 키 저장 경로를 임의로 우회하지 않았다.
- Host 종료 후 이번 Smoke가 만든 추적되지 않은 SQLite 파일은 정확한 경로를 확인해 제거했고, 작업 디렉터리는 clean으로 복귀했다. 재개 조건은 loopback 통신 권한과 개발 DataProtection 키 저장 경로를 제공한 격리된 Windows 환경이며, 이 조건이 충족되면 같은 런북으로 Migration·health·OpenAPI·Swagger·보호 API를 다시 수행한다.

## 2026-11-10 Development Host 키 저장 경계 수정과 재검증

- 원인 조사에서 Development Host가 DataProtection 기본 사용자 프로필 경로를 사용하여 현재 Work 실행 계정의 키 저장 권한에 의존하는 결함을 확인했다. 영향 범위는 Host 시작 시 키 저장 권한 경고·오류가 발생하고, 인증·Swagger 자체의 성공 여부와 무관하게 로컬 실행 증거가 불안정해지는 것이었다. 운영 환경의 인증·키 저장 구성을 임의로 완화하는 대안은 제외했다.
- 최소 안전 수정으로 `Development` 환경에서만 `App_Data/DataProtection-Keys`를 사용하고, 상대 경로는 Host 콘텐츠 루트 아래로 변환하도록 구성했다. 해당 경로는 저장소에서 제외했으며, 운영 환경은 이 분기를 실행하지 않고 배포 환경의 승인된 영속 키 저장소를 사용한다. 실제 보호 데이터를 생성해 지정 경로에 XML 키가 생기는 API Host 통합 회귀 테스트도 추가했다.
- 고정된 Development Bypass Tenant와 안정적인 `sub`는 기존 코드 계약을 유지하며, 문서에서 구성값으로 바꿀 수 있다고 안내하던 내용을 실제 동작과 일치하도록 수정했다. Metadata·JWKS·Issuer·Audience와 키 회전 식별자는 여전히 RFC 예약 주소를 사용하는 시뮬레이션 값이며 실제 외부 IdP 권한을 증명하지 않는다.
- `dotnet build src/Hosts/EDAN.Tensor.ApiHost/EDAN.Tensor.ApiHost.csproj --no-restore --configuration Release -m:1 -p:UseSharedCompilation=false`는 경고 0개·오류 0개로 통과했다. Development Host 직접 기동에서는 SQLite Migration, 고정 Bypass Tenant 초기화, 수신 대기와 키 파일 생성 경로를 확인했으며, 이전 DataProtection 권한 오류는 재현되지 않았다. 키 XML은 평문 저장 암호화기를 별도로 구성하지 않은 Development 경고만 남겼다.
- 같은 Host에서 `/health`, `/openapi/v1.json`, `/swagger/index.html`, 보호 API를 호출했지만 Windows loopback 숨김 소켓 오류가 계속되어 HTTP 응답과 Swagger 화면은 환경 미검증으로 남겼다. 대상 Testhost는 테스트 DLL 빌드 이후 프로세스가 시작되지 않고 출력 없이 정지하여 제한된 재현 후 중단했으며, 제품 assertion 실패로 판정하지 않았다. 병렬 빌드에서 동일 `obj` 파일 잠금도 재현되어 이후 검증은 직렬로 수행한다.
- 실행 중 생성된 SQLite와 DataProtection 키 산출물은 정확한 경로를 확인한 뒤 제거했고, 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 이 변경에서도 검증하지 않았다. 다음 재개 조건은 loopback 통신 권한과 안정적인 Testhost 실행 환경이며, 외부 경계 조건은 Issue #84와 Milestone #1의 별도 후속 범위다.

## 2026-11-10 PR #115 병합 후 main 최종 증거

- PR #115 `fix: Development Host 키 저장 경계 안정화`의 head `892ca332cd88455b35373d7274490ae456d7880d`는 main에 `95aeca244c6f7d8614e1874f1749af0944278c86`으로 Squash Merge되었다. PR Check [Actions Run #42649285064](https://github.com/namanvarava/Edan.Tensor/actions/runs/42649285064)와 병합 후 main [Actions Run #42649632425](https://github.com/namanvarava/Edan.Tensor/actions/runs/42649632425)은 자체 호스팅 Windows Runner `EDAN-TENSOR-WIN`에서 성공했다.
- 병합 후 main CI 로그는 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release --logger "trx;LogFileName=test-results-42649632425.trx"`를 원문으로 실행했다. Restore·Build는 성공하고 경고 0개·오류 0개였으며, 전체 테스트는 231개 통과·실패 0개·건너뜀 0개였다. API Host 통합 테스트 31개와 Development DataProtection 키 저장 회귀가 포함되었다.
- 병합 후 로컬 `main`과 `origin/main`은 `95aeca244c6f7d8614e1874f1749af0944278c86`으로 일치하고 작업 디렉터리는 clean이다. PR #115는 merged 상태이며 열린 PR은 0건이다. 원격 refs에는 `main`만 남았고, 병합된 `codex/fix-development-host-key-ring-20261110`은 원격 Git ref 404와 로컬 브랜치 목록에서 삭제를 확인했다.
- 이번 변경은 Development 환경의 로컬 키 저장 경계와 문서·회귀 테스트를 안정화했지만, 외부 IdP Metadata·JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB·외부 수신 시스템·GitHub main 보호 규칙 HTTP 403은 검증하지 않았다. Issue #84와 Milestone #1의 선행 조건이 충족될 때 별도 런북으로 수행한다.

## 2026-11-10 API Host Smoke 주소 변형 재시도와 loopback 권한 경계 확정

- 최신 main `f44c3f480239ac249b6dcc6447deec633a3c2b08`에서 `ASPNETCORE_ENVIRONMENT=Development`와 `dotnet run --project src/Hosts/EDAN.Tensor.ApiHost --no-launch-profile --no-build --configuration Release --urls http://localhost:5131`로 Host를 다시 기동했다. SQLite Migration 적용, 고정 `EDAN-NONPROD` Tenant 초기화, DataProtection 키 저장, `http://localhost:5131` 수신 대기를 확인했으며 이전의 키 저장 권한 오류는 재현되지 않았다.
- 별도 HTTP 클라이언트로 `http://localhost:5131/health`, `http://127.0.0.1:5131/health`, `http://[::1]:5131/health`, `http://localhost:5131/openapi/v1.json`을 호출했다. 네 주소 모두 “액세스 권한에 의해 숨겨진 소켓에 액세스를 시도했습니다” 오류로 실패했다. 따라서 포트 충돌이나 단일 DNS 주소 문제가 아니라 현재 Work 실행 환경의 loopback 소켓 접근 경계로 분류한다.
- 동일 원인에 대한 주소·포트 무한 재시도와 운영 설정 변경은 수행하지 않았다. Host가 만든 `App_Data` SQLite·DataProtection 키와 `edan-worker.db` 계열 파일은 정확한 경로를 확인한 뒤 제거했고 작업 디렉터리는 clean으로 복귀했다.
- HTTP Smoke 재개 조건은 loopback TCP 통신을 허용하고 Testhost·HTTP 클라이언트가 같은 격리 환경에서 실행되는 Windows 환경이다. 이 조건이 제공되기 전에는 Migration·Host 수신 대기만 성공으로 기록하고 `/health`, OpenAPI, Swagger UI와 보호 API의 HTTP 성공으로 확대하지 않는다.

## 2026-11-10 PR #117 병합 후 Release 명령과 self-hosted Test 정체 분석

- PR #117 `docs: Host Smoke loopback 재현 증거 기록`의 head `76e40883e22bce3fff2b241a28355bf635fade15`는 main에 `89f73ea484e322918ce9da087958a4d0912457a8`로 Squash Merge되었다. PR Check [Actions Run #42651837649](https://github.com/namanvarava/Edan.Tensor/actions/runs/42651837649)는 성공했다.
- 병합 후 main의 [Actions Run #42652394931](https://github.com/namanvarava/Edan.Tensor/actions/runs/42652394931)은 `EDAN-TENSOR-WIN` self-hosted Runner에서 Restore·Build를 성공시켰지만, `Test` 단계가 `2026-11-09T16:13:11Z`부터 계속 `in_progress`로 남았다. Job 단계는 코드 컴파일이 아니라 testhost 실행·연결 대기에서 정체되었고, 실행 중 로그 다운로드도 404여서 코드 assertion 실패나 테스트 오류로 분류할 근거가 없다. Runner 상태와 실행 job 로그가 제공되기 전에는 무한 재시도나 캐시 삭제를 수행하지 않는다.
- 현재 Work에서 요구된 원문 `dotnet restore EDAN.Tensor.sln`은 종료 코드 0으로 성공했다. 원문 `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`는 `EDAN.Tensor.SharedKernel` 출력 뒤 경고 0개·오류 0개로 종료 코드 1을 반환했다. 진단 로그는 `EDAN.Tensor.Platform.Domain` 종속성 확인 중 병렬 MSBuild 작업이 오류 메시지 없이 실패하는 환경 현상을 보여 주었다.
- 원인 분리를 위해 `dotnet build EDAN.Tensor.sln --no-restore --configuration Release --maxcpucount:1`을 실행했고 전체 프로젝트가 경고 0개·오류 0개, 종료 코드 0으로 성공했다. 이는 제품 컴파일 결함보다 현재 Work의 병렬 MSBuild·OneDrive 실행 경계가 원인임을 입증한다. 이 환경 우회 옵션을 저장소나 CI에 추가하지 않았다.
- 원문 `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`는 종료 코드 1로 출력 없이 끝났다. 원인 분리를 위해 단일 노드 Test를 실행하자 첫 `SharedKernel.Tests`에서 `vstest.console`이 90초 동안 testhost에 연결하지 못하고 `MSB4181: VSTestTask 작업이 false를 반환했지만 오류를 기록하지 않았습니다`로 중단되었다. 제품 assertion·예외 로그는 없었고, 잔류 dotnet·testhost·vstest 프로세스도 정리 후 남지 않았다.
- 따라서 현재 Work의 로컬 Release Test는 성공으로 표시하지 않고 testhost 실행 환경 미검증으로 기록한다. main CI가 계속 정체된 동일 증거와 합쳐져, 재개 조건은 `EDAN-TENSOR-WIN` Runner의 Test job 종료 또는 Runner/testhost 상태·로그 제공, 그리고 loopback HTTP 통신 권한이다. 이 조건이 충족되면 원문 Restore·Build·Test와 Host Smoke를 다시 실행한다.
- 후속 확인에서 main [Actions Run #42652394931](https://github.com/namanvarava/Edan.Tensor/actions/runs/42652394931)은 `2026-11-09T16:14:02Z`에 성공으로 종료되었고, Restore·Build·Test의 CI 성공 증거가 확보되었다. 이 결과는 Work 로컬 testhost 연결 실패를 해소한 것이 아니므로 로컬 Test 성공으로 대체하지 않는다.

## 2026-11-10 PR #122 병합 후 최신 main Release·Host Smoke 재검증

- PR #122 `docs: 최신 Host Smoke 증거 기록`의 head `0ce97be02e6369d85f41880193748494dabbd569`은 `0a5de8b41386ef76d42edcc9e2d61e36c6b34061`으로 main에 Squash Merge되었다. PR Check [Actions Run #42658916399](https://github.com/namanvarava/Edan.Tensor/actions/runs/42658916399)과 병합 후 main [Actions Run #42659216754](https://github.com/namanvarava/Edan.Tensor/actions/runs/42659216754)은 자체 호스팅 Windows Runner에서 Restore·Build·Test를 모두 성공시켰다.
- 병합 후 최신 main에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 순서대로 실제 실행했다. Restore는 종료 코드 0, Build는 경고 0개·오류 0개로 종료 코드 0, Test는 231개 통과·실패 0개·건너뜀 0개로 종료 코드 0을 기록했다. API Host 통합 테스트 31개와 Worker Infrastructure 테스트 66개도 통과했다.
- 최신 Release ApiHost를 임시 SQLite 네 개와 임시 DataProtection 키 경로로 외부 프로세스에서 기동했다. Development Bypass에서 `/health`, `/health/database`, `/openapi/v1.json`, `/swagger/index.html`, 보호된 `/api/platform/tenants/mine`이 모두 HTTP 200을 반환했고 stderr는 0바이트였다. 실행 후 프로세스·임시 DB·키 경로를 정리했다.
- 최신 Release WorkerHost도 임시 SQLite 네 개로 외부 프로세스에서 기동했다. Migration과 Handler catalog 동기화 후 `/health`가 HTTP 200을 반환했고 stderr는 0바이트였다. 실행 후 프로세스와 임시 DB 경로를 정리했다.
- 이 Smoke는 Development Bypass와 로컬 SQLite·Host 경계의 실제 HTTP 실행을 증명하지만, 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한을 증명하지 않는다. 운영 DB·실제 외부 수신 시스템과 GitHub main 보호 규칙 상세 API HTTP 403도 계속 환경 미검증으로 Issue #84와 Milestone #1에서 추적한다.

## 2026-11-10 최종 main Host Smoke 재실행

- PR #125 병합 후 검증 대상 main은 `b149411ab1c5b8edc23e867c0a9fb9d213b52521`이며, 병합 후 자체 호스팅 Windows CI [Actions Run #42662765923](https://github.com/namanvarava/Edan.Tensor/actions/runs/42662765923)은 Restore·Build·Test·정리 단계를 모두 성공시켰다. 이 PR은 검증 문서만 갱신했으며 제품 소스 동작은 변경하지 않았다.
- 실제 Windows 프로세스로 Release ApiHost를 임시 SQLite 네 개와 임시 DataProtection 키 경로로 기동했다. `/health`, `/health/database`, `/openapi/v1.json`, `/swagger/index.html`, Development Bypass 보호 API가 모두 HTTP 200을 반환했고 ApiHost 표준 오류 출력은 0바이트였다.
- 같은 임시 데이터베이스 경계를 사용하여 Release WorkerHost를 기동했고 `/health`가 HTTP 200을 반환했으며 WorkerHost 표준 오류 출력은 0바이트였다. 프로세스 종료 후 임시 DB·키·로그 경로가 존재하지 않음을 확인했다.
- 이번 실행은 코드에 고정된 Development Bypass 프로필과 `EDAN-NONPROD` Tenant·안정적인 `sub`만 사용했다. 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·키 회전 권한, 운영 DB·외부 수신 시스템, GitHub main 보호 규칙 HTTP 403은 이 실행으로 검증하지 않았다.

## 2026-11-10 PR #129 병합 및 최신 main·브랜치 정리 확인

- PR #129 `docs: API 계약 검증 증거 기록`의 head `c9d057d63243815d5cc880a33a4369f4daf4c2c0`은 main에 `84d9ed422ea46058f26d07fa237514de5533c55c`으로 Squash Merge되었다. PR Check [Actions Run #42668308955](https://github.com/namanvarava/Edan.Tensor/actions/runs/42668308955)와 병합 후 main [Actions Run #42668603514](https://github.com/namanvarava/Edan.Tensor/actions/runs/42668603514)은 성공했다.
- 병합 후 로컬 `main`과 `origin/main`은 같은 SHA `84d9ed422ea46058f26d07fa237514de5533c55c`를 가리키고 작업 디렉터리는 clean이다. `codex/record-api-contract-evidence-20261110`은 원격·로컬에서 삭제했으며, 현재 원격·로컬 브랜치는 `main`만 남았다. 열린 PR은 0건이다.
- 이 기록은 PR #128의 API 계약·Tenant 선택·Worker 헤더 수정과 PR #129의 증거 문서 병합 상태를 최신 main에 고정한다. 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB·외부 수신 시스템·GitHub main 보호 규칙 HTTP 403은 Issue #84와 Milestone #1의 외부 조건으로 계속 미검증이다.

## 2026-11-10 현재 Work 실행 환경 재검증

- 검증 대상은 PR #130 병합 후 main `cf18d0436710e4440ff9dc90d415e1bf6bd85bbd`이며, 병합 후 main CI [Actions Run #42670256154](https://github.com/namanvarava/Edan.Tensor/actions/runs/42670256154)은 Restore·Build·Test를 성공시켰다.
- 요구된 `dotnet restore EDAN.Tensor.sln`은 현재 Work에서 종료 코드 0으로 완료했다.
- 요구된 `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`는 `EDAN.Tensor.SharedKernel` 출력 뒤 종료 코드 1로 끝났고, 출력에는 경고 0개·오류 0개만 표시되었다. 단일 노드와 공유 컴파일을 끈 `dotnet build EDAN.Tensor.sln --no-restore --configuration Release -m:1 -p:UseSharedCompilation=false`는 전체 프로젝트를 경고 0개·오류 0개, 종료 코드 0으로 완료했다. OneDrive 밖의 임시 checkout에서도 같은 병렬 빌드 실패가 재현되었으므로 제품 컴파일 오류가 아니라 현재 Work의 병렬 MSBuild 실행 경계로 분류한다.
- 요구된 `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`는 출력 없이 종료 코드 1로 끝났다. 같은 단일 노드·공유 컴파일 비활성화 조건에서도 첫 Testhost가 `vstest.console`의 90초 연결 시간 초과로 중단되었고 assertion·제품 예외 로그는 없었다. 잔류 dotnet·testhost·vstest 프로세스는 확인 후 남지 않도록 정리했다. 로컬 Release Test는 성공으로 표시하지 않으며, Windows 자체 호스팅 CI의 성공 결과로만 제품 회귀를 판정한다.
- Release ApiHost를 임시 SQLite 네 개와 임시 DataProtection 키 경로로 실제 프로세스 기동했을 때 Migration, 고정 Development Bypass Tenant 초기화, `http://127.0.0.1:5197` 수신 대기와 stderr 0바이트를 확인했다. 같은 Work에서 `/health`, `/health/database`, `/openapi/v1.json`, `/swagger/index.html`, 보호 API의 HTTP 상태는 소켓 접근 경계로 확인되지 않아 모두 환경 미검증으로 남겼다. HTTP 성공으로 확대하거나 운영 설정을 완화하지 않았다.
- 동일 원인에 대해 원문 명령 무한 재시도, 캐시 삭제, CI 우회, 운영 인증·DataProtection 설정 완화는 수행하지 않았다. 재개 조건은 병렬 MSBuild·Testhost·loopback TCP 접근 권한이 보장된 Windows 자체 호스팅 Runner 또는 동일 수준의 격리 환경에서 원문 Restore·Build·Test와 Host HTTP Smoke를 다시 실행하는 것이다. 이 기록은 제품 실패를 숨기지 않으면서 인프라 실패와 코드 실패를 구분한 증거다.

## 2026-11-10 PR #131 병합 후 main CI 인프라 실패

- PR #131 `docs: 현재 Work 검증 경계와 테스트 기준 기록`의 head `6b861d09879de6fdb91934b534dd192b4134919a`는 main에 `a1d3ce19a43fcb6ac5012b25a4fbc1e9ccccab82`로 Squash Merge되었다. PR Check [Actions Run #42672505068](https://github.com/namanvarava/Edan.Tensor/actions/runs/42672505068)은 Restore·Build·Test를 성공시켰다.
- 병합 후 main [Actions Run #42672786178](https://github.com/namanvarava/Edan.Tensor/actions/runs/42672786178)은 51초 후 `Failure`로 종료되었다. GitHub Actions 화면의 유일한 Annotation은 `Internal server error. Correlation ID: f25b3d2b-471b-4b15-a2ff-110586259aa1`였고, Job 목록은 비어 있어 제품 단계·테스트 assertion·Runner 로그가 생성되지 않았다.
- 실패 Job 재실행은 GitHub API에서 `403 This workflow run cannot be retried`로 거부되었다. 서비스 내부 오류와 재실행 권한 경계를 코드 실패로 분류하지 않았고, 캐시 삭제·무한 재시도·보호 정책 우회도 수행하지 않았다. 따라서 최신 main CI는 미검증이며, Run #880과 PR #131 Run #881의 성공을 Run #882 성공으로 대체하지 않는다.
- 재개 조건은 GitHub Actions 서비스 오류가 해소되고 실패 Run 또는 새 main push에 대해 동일한 자체 호스팅 Restore·Build·Test가 성공하는 것이다. 그 성공 확인 전에는 전체 개발 완료를 선언하지 않는다.

## 2026-11-10 PR #128 API 계약·Tenant 선택·Worker 헤더 검증

- API 계약 감사에서 OpenAPI 문서가 실제 Tenant 필터가 요구하는 `X-Tenant-Id`를 모든 보호 경로에서 선택사항으로 표시하던 불일치를 확인했다. 또한 Tenant 선택값이 없을 때 `TenantMembershipAuthorizationFilter`가 선택 Tenant 오류와 membership 오류를 같은 `platform.membership.forbidden` 코드로 반환하여 장애 원인을 구분하기 어려웠다. Worker API의 `Idempotency-Key`와 DLQ 재처리의 `Replay-Request-Id` 누락 응답도 통합 계약으로 고정할 필요가 있었다.
- 최소 안전 수정으로 Tenant 경로와 `/api/platform/resources`, `/api/platform/policies`의 `X-Tenant-Id`를 OpenAPI에서 필수로 표시하고, 플랫폼 관리자가 Tenant 선택 없이 수행하는 `/activate`·`/memberships` 경로는 선택사항으로 유지했다. Tenant 컨텍스트 오류는 `platform.tenant.forbidden`, 실제 활성 소속 부재는 `platform.membership.forbidden`으로 분리했다. 기존 인증·Tenant 소속·관리자 권한 검사는 변경하지 않았다.
- 대상 통합 테스트는 OpenAPI의 Bearer 보안 요구, Tenant 헤더 필수 여부와 예외 경로, Worker 필수 헤더 누락 오류, Tenant 헤더 누락 오류를 확인했다. 대상 테스트 3개와 변경 후 API 통합 테스트 32개가 통과했다.
- 변경 브랜치에서 다음 요구 명령을 실제 실행했다. `dotnet restore EDAN.Tensor.sln`은 성공했고, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`는 경고 0개·오류 0개로 성공했다. `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`는 전체 232개 통과·실패 0개·건너뜀 0개로 종료했다.
- PR #128의 head `904669c545aee60823db8422d34bc83a09f2b814`에 대한 자체 호스팅 Windows CI Run #875는 Restore·Build·Test·정리를 모두 성공시켰다. PR은 `2341f0704d55ed30804e0b0bc653fcf03a5eadeb`로 Squash Merge되었고, 병합 후 main CI Run #876도 성공했다. 병합 후 로컬 `main`과 `origin/main`은 같은 SHA를 가리켰으며 `codex/api-contract-tenant-worker-20261110`은 로컬·원격에서 삭제했다.
- 이 검증은 로컬 SQLite·Swagger/OpenAPI와 내부 Platform·DataSet·Worker 계약을 강화한 증거다. 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB·외부 수신 시스템, GitHub main 보호 규칙 HTTP 403은 검증하지 않았으며 Issue #84와 Milestone #1의 외부 조건으로 계속 추적한다.

## 2026-11-10 동기화 경로 밖 임시 checkout 재검증

- PR #132 검증을 위해 main `a1d3ce19a43fcb6ac5012b25a4fbc1e9ccccab82`을 OneDrive 밖의 명시적 임시 경로 `C:\Users\naman\AppData\Local\Temp\edan-tensor-isolated-20261110`에 detached checkout으로 만들고, 검증 뒤 worktree와 임시 경로가 삭제되었음을 확인했다. 제품 소스와 저장소 설정은 변경하지 않았다.
- 해당 경로에서 원문 `dotnet restore EDAN.Tensor.sln`은 출력 없이 종료 코드 1을 반환했다. 진단 Restore는 `_FilterRestoreGraphProjectInputItems`의 병렬 MSBuild 작업 실패를 경고 0개·오류 0개 상태로 남겼고, `-m:1 -p:UseSharedCompilation=false`를 적용한 진단 Restore는 종료 코드 0으로 완료했다. 이때 NuGet 취약성 피드 `https://api.nuget.org/v3/index.json` 접근 실패에 따른 `NU1900` 경고가 기록되었다.
- 같은 경로에서 원문 `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`도 `EDAN.Tensor.SharedKernel` 출력 뒤 경고 0개·오류 0개로 종료 코드 1을 반환했다. 단일 노드와 공유 컴파일 비활성화 조건의 Release Build는 종료 코드 0, 오류 0개, `NU1900` 경고 19개로 전체 프로젝트를 완료했다. 따라서 OneDrive는 단독 근본 원인이 아니며 현재 Work의 병렬 MSBuild·NuGet 네트워크 환경 경계가 재현된 것으로 확정한다.
- 원문 `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`는 출력 없이 정체되었다. API 통합 테스트 프로젝트만 단일 노드로 실행해도 testhost 결과가 생성되지 않았고, 관찰 시점에 dotnet·MSBuild·vstest·testhost 프로세스가 존재하지 않았다. 안전한 제한 관찰 후 중단했으며, 제품 assertion·제품 예외·테스트 실패 로그는 확인되지 않았다. 이 결과를 테스트 성공으로 표시하지 않는다.
- `dotnet --info`는 SDK 정보 출력 중 Windows Service Control Manager 접근 거부 예외를 기록했고, `dotnet --version` 자체는 성공했다. 따라서 현재 Work의 권한·프로세스 실행·NuGet 네트워크 상태를 저장소 코드 결함으로 추측하지 않는다.
- 재개 조건은 병렬 MSBuild와 testhost가 실제로 실행되고 로그가 보존되는 자체 호스팅 Windows Runner 또는 동등한 격리 환경, NuGet 감사 피드 접근 또는 그 상태를 판정할 수 있는 네트워크, loopback TCP 접근 권한이다. 조건 충족 후 원문 Restore·Build·Test와 ApiHost/WorkerHost HTTP Smoke를 다시 실행한다.

## 2026-11-10 PR #132 병합 후 최신 main 재검증

- PR #132의 head `451a3db88a93bfbcfbdf72ccb067e55813829ccd`는 `8a82bf9f2a6546c593e44928d588f12d2fa2126c`로 squash merge되었다. PR 자체 CI [Run #884](https://github.com/namanvarava/Edan.Tensor/actions/runs/42675745493)는 Restore·Build·Test·정리를 모두 성공시켰다.
- 병합 후 main [Run #885](https://github.com/namanvarava/Edan.Tensor/actions/runs/42676094148)은 merge commit `8a82bf9f2a6546c593e44928d588f12d2fa2126c`에서 Restore·Build·Test·정리를 모두 성공시켰다. `build-and-test` Job의 Set up, checkout, Runner environment, Restore, Build, Test, Post checkout, Complete job 단계가 모두 성공했다.
- PR #132 병합 직후 로컬 `main`·`origin/main`·HEAD는 같은 SHA `8a82bf9f2a6546c593e44928d588f12d2fa2126c`를 가리켰고, `git diff --check`와 작업 디렉터리 clean 상태를 확인했다. 그 시점의 PR 검색 결과 열린 PR은 0건이며, 원격·로컬 브랜치 검색 결과 `main`만 남았다. `codex/record-main-ci-infra-failure-20261110`은 원격 push 삭제와 로컬 branch 삭제를 각각 성공시켰다. 이후 이 문서의 상태 정합성을 PR #133으로 갱신했으며, 현재 main SHA는 해당 후속 병합 커밋으로 변경되었다.
- Run #882·#883의 Job 생성 전 GitHub Actions 내부 오류는 Run #884·#885에서 재현되지 않았고, 새 main CI 성공으로 최신 문서 변경의 병합 상태를 검증했다. 다만 현재 Work의 원문 Testhost·loopback HTTP 실패, 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB·외부 수신 시스템·GitHub main 보호 규칙 HTTP 403은 이 CI 성공으로 대체하지 않으며 Issue #84와 Milestone #1에서 계속 환경 미검증으로 추적한다.

## 2026-11-10 최신 main과 다음 연속 작업 기준

- PR #133의 head `583cac798d53d755a642ca6b55219e6f3d4e7d00`는 `19c75191b16ca08ae30fde14a1ed19cfc552768f`로 squash merge되었고, PR 자체 CI Run #886과 병합 후 main CI Run #887이 Restore·Release Build·Release Test를 성공시켰다.
- PR #134의 head `2aa2c13f1a179faabc8f77b092ba693203546c3e`는 `d973499f8f888bd36862ed7935ac93fb4a79ee21`로 squash merge되었고, PR 자체 CI Run #888과 병합 후 main CI [Run #889](https://github.com/namanvarava/Edan.Tensor/actions/runs/42678160541)이 Restore·Release Build·Release Test를 성공시켰다. 현재 로컬·원격 `main`은 이 SHA를 가리키며 작업 디렉터리는 clean이고, 열린 PR과 작업 브랜치는 없다.
- 다음 Work 구현 순서는 API 계약·멱등성 경쟁 조건 → Tenant·인증 경계 → Worker Lease/Fencing·체크포인트·재시도·DLQ·종료 복구 → 승인된 비라이선스 기능 → Licensing·LicenseAuthority → 라이선스 적용 검증 → 운영·외부 환경 검증이다. Licensing은 앞 단계가 안정된 뒤 마지막 내부 구현 단계로 유지한다.
- 이번 Work에서 추가한 외부 전달 저장 경계는 DB 유일성 경쟁 시 동일 Tenant·Idempotency Key의 커밋된 기존 전달을 반환하고, API 보상 삭제는 이번 요청이 실제 생성한 레코드에만 적용하도록 했다. 대상 테스트는 `CreateOrGet_ReturnsExistingDeliveryWithoutCreatingDuplicate`이며, Work의 테스트 호스트가 출력 없이 정체되어 원문 성공을 판정하지 못했으므로 Release 컴파일 0 오류와 CI 재검증을 분리해 기록한다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB·외부 수신 시스템, GitHub main 보호 규칙 HTTP 403은 여전히 환경 미검증이다. Issue #84와 Milestone #1의 외부 조건이 제공될 때만 해당 범위를 재개한다.

## 2026-11-10 PR #135 병합 후 최신 main 기준

- PR #135 `fix: 외부 전달 멱등성 경쟁 조건 보강`의 head `e9fa905679145aa5ae5d18dcddb0ad2a9f4e023e`는 `ce460ec7f3c4efcc54b4da5297879149b2f52b97`로 `main`에 Squash Merge되었다. PR 자체 CI Run #890과 병합 후 main CI [Run #891](https://github.com/namanvarava/Edan.Tensor/actions/runs/42681139618)은 Restore·Release Build·Release Test와 정리 단계를 모두 성공시켰다.
- 변경 원인은 같은 Tenant·Idempotency Key의 동시 외부 전달 생성에서 DB 유일성 충돌이 발생할 때 첫 요청의 보상 삭제가 두 번째 요청의 커밋된 레코드를 삭제할 수 있었던 경쟁 조건이다. 저장소는 충돌 시 커밋된 기존 전달을 반환하고, API는 이번 요청이 실제 생성한 경우에만 보상 삭제 식별자를 설정한다.
- 대상 회귀 테스트는 `CreateOrGet_ReturnsExistingDeliveryWithoutCreatingDuplicate`이며, 전체 Release 회귀는 main CI Run #891에서 성공했다. 병합 후 로컬·원격 `main`은 위 SHA로 일치하고 작업 디렉터리는 clean이다. PR 브랜치 `codex/fix-external-delivery-idempotency-20261110`은 원격·로컬에서 삭제했고 현재 브랜치는 `main`만 남았다.
- 현재 내부 실행 순서는 API 계약·멱등성 → Tenant·인증 경계 → Worker 장애·복구 → 승인된 비라이선스 기능 → Licensing·LicenseAuthority → 라이선스 적용 → 운영·외부 환경 검증이다. Commercial·DuckDB·Parquet·WPF의 승인 계약이 없는 상태에서 임의 기능을 추가하지 않는다.
- 실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB·외부 수신 시스템과 GitHub main 보호 규칙 HTTP 403은 여전히 환경 미검증이다. Issue #84와 Milestone #1의 외부 조건이 제공될 때만 해당 범위를 재개한다.

## 2026-11-10 PR #137·#138 병합 후 최신 main 기준

- PR #137 `docs: API 사용 가이드와 Bypass 경계 정합화`의 head `6c1155ec413f805b519200a6a6639ee6d9f5e95e`는 `3c018f02764134f7c4a8cd73965642160aa168b1`로 Squash Merge되었다. PR 자체 CI Run #894와 병합 후 main CI [Run #895](https://github.com/namanvarava/Edan.Tensor/actions/runs/42684182214)은 Restore·Release Build·Release Test와 정리 단계를 모두 성공시켰다.
- PR #137은 실제 코드의 `DevelopmentBypassExternalIdentityProfile.Fixed`와 API 엔드포인트 동작에 맞춰 API 가이드를 보완했다. Bypass Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 식별자는 코드에 고정되고 설정값으로 변경되지 않으며, `Authentication__BypassRole`은 Development Claim 역할만 선택한다. DataSet·Validation·Worker·외부 전달의 실제 호출 순서와 오류·멱등성 계약도 같은 가이드에 기록했다.
- PR #138 `docs: 외부 IdP 검증 런북의 Bypass 경계 정정`의 head `ca0ac4af77d773ec734198ef3c14c94b08935bfc`는 `6a5ff9e7227ace12e074a9d13759ac2403f59aa6`로 Squash Merge되었다. PR 자체 CI Run #896과 병합 후 main CI [Run #897](https://github.com/namanvarava/Edan.Tensor/actions/runs/42685196379)은 Restore·Release Build·Release Test와 정리 단계를 모두 성공시켰다.
- PR #138은 외부 IdP 런북에서 실제로 존재하지 않는 `Authentication__BypassSubject` 설정명을 제거하고, Development Bypass 고정 프로필은 External IdP 검증에 사용하지 않는다는 경계를 명시했다. 현재 로컬·원격 `main`은 `6a5ff9e7227ace12e074a9d13759ac2403f59aa6`로 일치하고 작업 디렉터리는 clean이다. PR #137·#138의 원격·로컬 작업 브랜치는 삭제되었고 원격·로컬 브랜치에는 `main`만 남았다.
- 최신 내부 실행 순서는 API 계약·멱등성 → Tenant·인증 경계 → Worker 장애·복구 → 승인된 비라이선스 기능 → Licensing·LicenseAuthority → 라이선스 적용 → 운영·외부 환경 검증이다. 외부 IdP·운영 DB·외부 수신 시스템·GitHub main 보호 규칙 HTTP 403은 이 CI 성공으로 대체하지 않으며 Issue #84와 Milestone #1의 외부 조건으로 유지한다.

## 2026-11-10 PR #139 병합 후 Release 재검증

- PR #139의 head `56a899ee2dd4646c2ba00830b4d87d4e81b167b9`는 `a0c990aca0ee3466701e4ccee21bc1c61815e956`로 Squash Merge되었다. PR 자체 CI Run #898과 병합 후 main CI [Run #899](https://github.com/namanvarava/Edan.Tensor/actions/runs/42686600210)은 Restore·Release Build·Release Test·정리 단계를 모두 성공시켰다.
- 최신 main에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 실제 실행했다. 제한된 Work 권한의 최초 Restore는 출력 없이 종료 코드 1을 반환했으며, 상세 실행에서 .NET SDK 10.0.402의 Service Control Manager 접근이 Win32 오류 5(액세스 거부)로 차단된 환경 원인을 확인했다. 권한 확장 후 같은 Restore는 성공했다.
- 권한 확장 환경의 Release Build는 경고 0개·오류 0개였고, 전체 Release Test는 233개 통과·실패 0개·건너뜀 0개였다. 이 재검증은 인증 우회나 테스트 생략을 사용하지 않았으며, 개발 Bypass는 코드에 고정된 비운영 시뮬레이션 프로필로만 동작했다.
- 중단 원인은 소스·의존성 결함이 아니라 로컬 Work의 Windows 서비스 조회 권한이었다. 재발 방지는 CI 자체 호스팅 Runner의 성공 증거와 로컬 권한 확장 재검증을 구분 기록하고, 실패 원인을 확인하지 않은 재시도를 하지 않는 것이다.
