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
- API 통합: 19개

최신 `main` 커밋 `dcc01f10db4390a3a25652abb2f2fa869573d803`에서 Windows 로컬 Release 전체 테스트는 총 216개가 실패 0개·건너뜀 0개로 통과했다. 이 실행에는 개발 Bypass 선택, 외부 IdP 설정 없는 토큰 없는 요청의 인증 성공, 고정 Claim 발급, 비개발 환경 Bypass 차단 및 외부 인증 경계 회귀 테스트가 포함된다. 병합 후 [Actions Run #739](https://github.com/namanvarava/Edan.Tensor/actions/runs/42392176169)도 같은 SHA에서 `EDAN-TENSOR-WIN` 자체 호스팅 Runner로 Restore·Build·Test를 모두 성공시켰다.

2026-11-07 DataSet 검증 Fencing Token 코드 기준 커밋 `2e2a49efdac38417037b7072c1220af17eeb8afb`에서 총 211개 테스트가 실패 0개·건너뜀 0개로 통과했다. 같은 결과는 [Actions Run #692](https://github.com/namanvarava/Edan.Tensor/actions/runs/42326121824)에서 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`으로 확인할 수 있다. 이 기록은 해당 시점의 인수인계 증거이며 최신 테스트 수를 대체하지 않는다. 저장소 루트에서 다음 명령으로 최신 Release 검증을 재현할 수 있다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

원격 CI는 Windows 자체 호스팅 Runner에서 동일한 명령을 실행하며, 로컬 실행 결과와 원격 CI 결과를 구분해 검증 문서에 기록한다.
