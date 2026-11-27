# 상용 완전 완료 게이트

이 게이트는 저장소에 공통 제품 기능과 자동 검증이 모두 존재하는지 판정한다. 고객사 주소·계정·Secret·IdP·SLA는 저장소에 넣지 않으며, 실제 운영 인수 시 별도 입력서로 검증한다.

## 자동 판정 범위

1. 명명규칙·Provider Migration 검증
2. Tenant·Membership·DB Binding 계약
3. 라이선스 검증 경계와 테스트
4. Secret 참조 기반 자격증명 해석
5. 복수 DB DML 오케스트레이션
6. 재시도·보상·멱등성·Worker 장애 복구
7. Provider 정합성 검사
8. PostgreSQL·SQL Server·Oracle Migration 자산
9. 백업·복구 운영 문서와 증적 경계
10. SME·Enterprise 운영 프로필
11. Production 설정 보안 게이트
12. Release 테스트 및 패치 검증

## 실행

```powershell
pwsh -NoProfile -File .\scripts\verification\Assert-CommercialCompletion.ps1
```

테스트를 이미 별도 실행한 경우:

```powershell
pwsh -NoProfile -File .\scripts\verification\Assert-CommercialCompletion.ps1 -SkipTests
```

고객별 운영 인수까지 판정하는 경우에는 승인된 입력서 경로를 추가한다.

```powershell
pwsh -NoProfile -File .\scripts\verification\Assert-CommercialCompletion.ps1 `
  -CustomerInputJson .\path\to\approved-commercial-input.json
```

## 판정 규칙

- `Passed`: 공통 제품 구현과 자동 검증이 통과한 상태
- `Failed`: 소스·테스트·Migration·문서·검증 중 하나라도 누락된 상태
- 고객 입력서를 지정하지 않은 `Passed`는 고객사 운영 인수 완료를 의미하지 않는다.
- 고객 입력서에는 비밀번호·토큰·개인키를 기록하지 않는다.

## 상용 출시 조건

공통 게이트 `Passed` 이후 고객별로 Migration, Health, 인증, Tenant 격리, Routine Allowlist, 복수 DB DML, 백업 복구, 장애 전환, 성능 시험을 실제 운영값으로 재실행하고 승인 증적을 보관해야 한다.
