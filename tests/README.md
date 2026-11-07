# 테스트

저장소에는 도메인·응용·인프라·API 통합 테스트 프로젝트가 포함되어 있으며, 보안·복원력·아키텍처 경계는 해당 프로젝트의 테스트에서 검증한다.

현재 확인된 테스트 구성은 다음과 같다.

- SharedKernel: 11개
- Platform Domain: 27개
- Platform Application: 30개
- Platform Infrastructure: 12개
- DataSet Domain: 6개
- DataSet Application: 14개
- DataSet Infrastructure: 10개
- Worker Domain: 21개
- Worker Infrastructure: 66개
- API 통합: 21개

최신 기준은 `main` SHA `38539a85d32732cfc4111261576fc36fa1a2d052`이다. Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`에서 실행한 [Actions Run #762](https://github.com/namanvarava/Edan.Tensor/actions/runs/42409767353)은 Restore·Release Build·Release Test·정리 단계를 모두 성공시켰고, 전체 테스트는 218개 통과·실패 0개·건너뜀 0개였다. 이 문단은 아래의 역사적 실행 기록과 구분되는 현재 검증 기준이다.

이번 Bypass 외부 설정 무시와 로컬 Tenant 초기화 회귀를 포함한 검증 기준에서 자체 호스팅 Windows Release 전체 테스트는 총 218개가 실패 0개·건너뜀 0개로 통과했다. 이 실행에는 개발 Bypass 선택, 외부 IdP 설정 없는 토큰 없는 요청의 인증 성공, 잘못된 외부 IdP 설정을 Bypass에서 무시하는 동작, 고정 Claim 발급, 비개발 환경 Bypass 차단, 로컬 Tenant·관리자 소속의 중복 방지 및 외부 인증 경계 회귀 테스트가 포함된다. PR #63 검증은 [Actions Run #744](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399072656)에서 확인했고, 병합 후 최신 `main`은 [Actions Run #746](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399622276)의 `EDAN-TENSOR-WIN` Runner에서 Restore·Build·Test를 다시 성공시켰다.

2026-11-07 DataSet 검증 Fencing Token 코드 기준 커밋 `2e2a49efdac38417037b7072c1220af17eeb8afb`에서 총 211개 테스트가 실패 0개·건너뜀 0개로 통과했다. 같은 결과는 [Actions Run #692](https://github.com/namanvarava/Edan.Tensor/actions/runs/42326121824)에서 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`으로 확인할 수 있다. 이 기록은 해당 시점의 인수인계 증거이며 최신 테스트 수를 대체하지 않는다. 저장소 루트에서 다음 명령으로 최신 Release 검증을 재현할 수 있다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

원격 CI는 Windows 자체 호스팅 Runner에서 동일한 명령을 실행하며, 로컬 실행 결과와 원격 CI 결과를 구분해 검증 문서에 기록한다.
