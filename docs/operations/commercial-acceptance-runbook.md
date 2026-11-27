# EDAN.Tensor 상용 운영 인수 실행 순서

이 절차는 [상용 운영 인수 입력서](./commercial-acceptance-input-template.md)와 입력 검증 게이트를 통과한 뒤 고객사 VM에서 실행한다. 비밀값은 명령행·문서·로그에 남기지 않고 승인된 Secret Manager에서 주입한다.

## 사전 조건

```powershell
pwsh -File ./scripts/verification/Assert-CommercialAcceptanceInputs.ps1 `
  -InputJson ./docs/operations/commercial-acceptance-input.json
```

검증이 실패하면 다음 단계로 진행하지 않는다.

## 실행 순서

1. 변경 승인·백업 확인: 점검창, 롤백 기준, 최근 복구 가능 백업을 확인한다.
2. Migration: 대상 Provider의 Platform/Worker/DataSet Migration을 적용하고 pending model changes가 없는지 확인한다.
3. 서비스 기동: API·Worker·IdP·대상 DB를 기동하고 재기동 후에도 자동 복구되는지 확인한다.
4. Health: API/Worker health와 database health가 모두 정상인지 확인한다.
5. 인증·테넌트: 실제 Issuer/JWKS/Audience로 토큰을 발급하고 테넌트 구성원·활성 Binding을 확인한다.
6. Provider metadata: PostgreSQL·SQL Server·Oracle 중 승인된 Provider의 테이블·Routine metadata를 확인한다.
7. Routine 실행: Allowlist에 승인된 Routine만 호출하고 입력·출력 계약과 권한 오류를 확인한다.
8. 복수 DB DML: 성공, Provider 실패, timeout, 재시도, 보상, 멱등성 시나리오를 실행하고 감사 상태를 확인한다.
9. 백업·복구: 외부 저장소 백업을 만들고 별도 복구 대상에 복원한 뒤 행 수·Migration·핵심 업무 결과를 비교한다.
10. 장애·재기동: API/Worker/Provider 연결 장애와 VM 재기동을 실행하고 알림·재처리·중복 억제를 확인한다.
11. 성능: 승인된 동시 사용자·RPS·p95/p99·DML 처리량 목표를 측정하고 결과를 기록한다.
12. 승인: 모든 증적 링크와 측정값을 인수서에 기록하고 운영 승인자와 보안 승인자가 서명한다.

## 완료 판정

각 단계에 PASS 증적이 있고, 실패 시 보상·재시도·감사·알림이 기대한 결과를 보이면 상용 운영 완전 완료로 판정한다. 입력값이나 증적이 하나라도 비어 있으면 개발 VM 검증 완료로만 기록하고 상용 인수 완료로 판정하지 않는다.
