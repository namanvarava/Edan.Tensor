# 운영 프로필 선택

## 기본값: SME

별도 프로필을 지정하지 않은 단일 고객사 설치는 SME 프로필을 사용한다.

```text
Operations__Profile=SME
Database__Provider=Sqlite
```

Framework 내부 데이터는 SQLite 영속 볼륨에 저장하고, 고객 업무 DB는 Tenant Binding으로 연결한다. 외부 HTTPS IdP, OS 보호 Secret 저장소, 일일 백업, RPO 24시간·RTO 4시간, 20명·5 RPS 기준을 기본값으로 한다.

## Enterprise 선택

다중 인스턴스와 HA가 필요한 경우 Enterprise 환경 파일을 주입한다.

```text
Operations__Profile=Enterprise
Database__Provider=PostgreSQL
```

Enterprise는 PostgreSQL HA, API·Worker 2개 이상, Secret Manager/HSM, PITR/WAL, RPO 15분·RTO 1시간, 100명·25 RPS 기준을 사용한다.

## 검증

```powershell
pwsh -NoProfile -File .\scripts\verification\Assert-DeploymentProfile.ps1 `
  -EnvironmentFile .\path\to\profile.env -Profile SME
```

대기업 환경은 `-Profile Enterprise`로 동일하게 검증한다. 고객사별 Secret·IdP 주소·백업 저장소·SLA는 환경 파일에 평문으로 기록하지 않고 보호된 운영 입력과 Secret Manager에서 주입한다.
