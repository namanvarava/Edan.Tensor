# EDAN.Tensor 상용 운영 준비도 판정표

검증 기준일: 2026-11-27  
기준 커밋: 검증 시점의 `main` HEAD

이 문서는 개발 VM과 저장소에서 확인할 수 있는 완료 항목과, 실제 고객사 운영 환경에서 승인·측정해야 하는 항목을 구분한다. 개발 VM의 성공 결과를 고객사 운영 인수로 확대 해석하지 않는다.

## 완료 및 증적 확인 항목

| 영역 | 판정 | 증적 |
|---|---|---|
| main 병합 | PASS | PR #341·#342·#343·#344·#345·#346·#347 병합 완료, 열린 PR 없음 |
| Release 빌드·전체 테스트 | PASS | `artifacts/local-completion-gate/summary.json` |
| Provider Migration | PASS | PostgreSQL·SQL Server·Oracle 정적 Migration 검증 |
| Production 기본 Provider | PASS | 운영 Compose 기본값 PostgreSQL, SQLite는 명시적 오버라이드만 허용 |
| API·Worker Health | PASS | VM `/health`, `/health/database` 모두 Healthy/Connected |
| Keycloak External 인증 | PASS | VM JWKS·Issuer·Audience·Tenant membership 검증 |
| Tenant DB Binding | PASS | PostgreSQL·SQL Server·Oracle 바인딩·활성화·Health 검증 |
| Routine Allowlist | PASS | Provider별 Routine metadata와 실행 검증 |
| 복수 Provider 정합성 | PASS | Consistent 결과와 동일 결과값 확인 |
| 복수 DB DML | PASS | 성공·Provider 실패·보상·멱등성·재시도 회귀 테스트 |
| 감사 기록 | PASS | 성공·보상 상태와 오류 코드 기록 확인 |
| 백업·복구 | PASS | PostgreSQL restore, SQL Server `RESTORE VERIFYONLY`, Oracle Data Pump |
| 라이선스 경계 | PASS | Development 라이선스와 Production 차단 회귀 테스트 |
| 테스트 객체 범위 | PASS | Provider 테스트 객체를 `tensor_provider_test` 범위로 제한 |

## 상용 인수 전 필수 확정 항목

| 영역 | 현재 판정 | 반드시 확정할 값 |
|---|---|---|
| Secret Manager | INPUT_REQUIRED | Vault·KMS·클라우드 Secret Manager 중 하나, 접근 주체, 교체 주기, 복구 절차 |
| 외부 백업 | INPUT_REQUIRED | 저장소, 암호화 키, 보존 기간, 백업 주기, RPO/RTO, 복구 담당자 |
| 알림 | INPUT_REQUIRED | 이메일·Webhook·모니터링 시스템, 수신자, 임계값, 중복 억제 정책 |
| 고객 IdP | INPUT_REQUIRED | 실제 Issuer, Metadata/JWKS 주소, 인증서, Audience, 키 회전 정책 |
| 고객 업무 DB | INPUT_REQUIRED | 실제 Provider, 접속 TLS, 최소권한 계정, Schema, 방화벽 정책 |
| 업무 Routine | INPUT_REQUIRED | Routine 목록, 입력·출력 계약, Allowlist 승인자, 오류·보상 규칙 |
| 성능 기준 | INPUT_REQUIRED | 동시 사용자, 요청량, p95/p99 응답시간, DML 처리량, 장애 허용시간 |
| 운영 배포 승인 | INPUT_REQUIRED | 변경 승인자, 점검창, 롤백 기준, 인수 확인서 |

## 최종 판정 규칙

개발 VM 검증은 위 완료 항목이 모두 PASS일 때 코드·검증 완료로 판정한다. 상용 운영 완전 완료는 `INPUT_REQUIRED` 항목이 실제 운영값으로 채워지고, 해당 값으로 Migration·Health·인증·Routine·DML·백업복구·성능·장애복구를 다시 수행한 뒤 승인될 때만 판정한다.
