# Production 설정 사전 점검

배포 전에 `scripts/verification/Assert-ProductionConfiguration.ps1`를 실행한다. 이 점검은 개발용 설정이 운영으로 유입되는 것을 막는다.

```powershell
pwsh -File ./scripts/verification/Assert-ProductionConfiguration.ps1 `
  -EnvironmentFile ./production.env
```

폐쇄망 단일 고객사에서 승인된 SQLite 프로파일을 사용할 때만 다음 옵션을 추가한다.

```powershell
pwsh -File ./scripts/verification/Assert-ProductionConfiguration.ps1 `
  -EnvironmentFile ./production.env `
  -AllowOfflineSqlite
```

운영 환경 파일에는 DB 비밀번호·Client Secret·개인키·토큰을 기록하지 않는다. 해당 값은 승인된 Secret Manager가 프로세스 시작 시 주입해야 한다. `-AllowPlaintextSecrets`는 운영 배포에서 사용하지 않는다.

점검은 Production 환경, External 인증, HTTPS Metadata/Issuer, Provider별 TLS, 지원 Provider, 필수 연결 문자열을 확인한다. 이 점검이 통과해도 실제 고객 IdP·DB·Routine·백업 저장소·알림 채널의 인수 테스트는 별도로 수행해야 한다.
