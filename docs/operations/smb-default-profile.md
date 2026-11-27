# 중소기업 기본 운영 프로필

이 프로필은 별도 IT팀이 없는 단일 고객사 설치를 위한 기본값이다. `EdanFramework` 내부 메타데이터는 SQLite로 운영하고, 고객사 업무 데이터인 `Edan`은 테넌트 DB Binding으로 PostgreSQL을 기본 선택한다. SQL Server와 Oracle도 같은 Binding 계약으로 선택할 수 있다.

## 구성

```text
단일 VM
├─ API + Worker
├─ EdanFramework: SQLite(App_Data의 영속 볼륨)
├─ Keycloak: 내부 HTTPS 서비스 또는 고객 IdP
└─ Edan 업무 DB: PostgreSQL 기본 / SQL Server·Oracle 선택
```

현재 애플리케이션에서 `Database__Provider=Sqlite`는 Platform·Worker·DataSet 등 Framework 내부 컨텍스트에 적용된다. 고객사 업무 DB는 `TenantDatabaseSecrets`와 Tenant Database Binding으로 별도 지정하므로 두 영역을 같은 Provider로 강제하지 않는다.

## 기본 보안값

- 환경은 항상 `Production`으로 실행한다.
- 인증은 `External`만 허용한다. `Bypass`는 개발 환경에서만 사용한다.
- IdP Metadata와 Issuer는 HTTPS를 사용한다.
- 비밀번호·Client Secret·토큰·개인키는 `.env`, Git, 명령행, 로그에 저장하지 않는다.
- OS 보호 저장소에 Secret을 저장하고 API/Worker 시작 시 주입한다.
- 업무 DB 계정은 테이블·Allowlist Routine에 필요한 최소권한만 가진다.

## 백업과 복구 기본값

- Framework의 네 SQLite 파일을 일관된 시점에 매일 백업한다.
- 최근 30개 백업을 보존하고, 주 1회 별도 USB/NAS 또는 승인된 외부 저장소에 복사한다.
- 기본 목표는 RPO 24시간, RTO 4시간이다. 고객 계약이 있으면 계약값이 우선한다.
- 월 1회 별도 경로에 복구하여 파일 무결성, Migration History, 핵심 Health를 확인한다.

## 기본 운영 기준

- 동시 사용자 20명
- 5 RPS
- p95 응답시간 2초 이하
- p99 응답시간 5초 이하
- Worker 재시도·보상·멱등성 회귀 테스트 통과
- API·Worker·백업 실패·디스크 부족을 관리자 이메일 또는 화면 알림으로 표시
- API·Worker는 `restart: unless-stopped`로 자동 재기동

## 적용 순서

1. `smb-default-profile.env.example`을 운영 외부 경로에 복사한다.
2. 실제 HTTPS IdP 주소를 입력한다.
3. OS 보호 저장소를 만들고 DB·IdP·라이선스 Secret을 등록한다.
4. Compose에 환경 파일을 주입해 API와 Worker를 함께 기동한다.
5. Framework Migration을 적용하고 Tenant 하나를 생성한다.
6. 고객 업무 DB를 PostgreSQL로 Binding하고 필요 Routine만 Allowlist에 등록한다.
7. `/health`, `/health/database`, Routine, 복수 DB DML, 백업복구를 검증한다.

이 프로필은 단일 고객사 기본값이며, 다중 인스턴스·고가용성·대규모 동시 사용에는 PostgreSQL 기반 Framework 운영 프로필로 전환한다.
