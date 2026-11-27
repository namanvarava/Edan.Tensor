# 대기업 기본 운영 프로필

이 프로필은 다중 인스턴스, 중앙 관제, 고가용성, 엄격한 복구 목표가 필요한 고객을 위한 기본값이다. 고객사별 인프라 제품명은 입력받지 않고, 표준 인터페이스와 최소 요구조건만 정의한다.

## 논리 구성

```text
고객사 네트워크
├─ Load Balancer / WAF
├─ API 인스턴스 2개 이상
├─ Worker 인스턴스 2개 이상(작업 중복 방지 잠금 적용)
├─ EdanFramework: PostgreSQL HA
├─ Edan 업무 DB: Tenant Binding으로 PostgreSQL·SQL Server·Oracle 중 선택
├─ OIDC/SAML IdP
├─ Secret Manager/HSM
├─ 백업 저장소(PITR + 장기 보관)
└─ SIEM/Monitoring/Alerting
```

## 필수 운영 기준

| 영역 | 기본 기준 | 합격 증적 |
|---|---|---|
| 가용성 | API/Worker 2개 이상, 단일 인스턴스 장애 시 서비스 유지 | 장애 전환 결과와 헬스 응답 |
| 데이터 | Framework PostgreSQL HA, Edan은 고객 선택 DB | Migration·Binding·Provider 검증 |
| 보안 | External 인증, HTTPS, Secret Manager, 최소 DB 권한 | 보안 점검 및 Secret 노출 검색 |
| 복구 | RPO 15분, RTO 1시간 | 복구 리허설 기록 |
| 성능 | 100 동시 사용자, 25 RPS, p95 ≤ 1초, p99 ≤ 3초 | 부하시험 보고서 |
| 감사 | 인증·권한·Binding·Routine·DML 상태·라이선스 이벤트 추적 | 감사 조회 및 SIEM 수신 |
| 배포 | Migration 단일 실행, API/Worker 동일 릴리스, 롤백 계획 | 릴리스 체크리스트 |

## 복수 DB 업무 처리

Framework DB와 고객 업무 DB 사이에는 분산 트랜잭션을 사용하지 않는다. 각 DML 요청은 `operation_id`와 멱등키를 발급하고, 단계별 결과를 기록한다. 실패 시 자동 재시도 후 보상 작업 또는 수동 조치 상태로 전환하며, 부분 성공을 숨기지 않고 API·감사 로그·알림에 표시한다.

## 운영 전환 조건

다음이 모두 충족될 때만 Enterprise 운영 승인을 낸다.

1. 실제 고객 인프라 값을 입력한 운영 입력서가 승인됨
2. PostgreSQL HA와 백업 복구가 검증됨
3. 세 Provider의 Tenant Binding·Metadata·Routine Allowlist 검증이 통과됨
4. 복수 DB DML의 성공·실패·재시도·보상·멱등성 테스트가 통과됨
5. 장애 전환·라이선스 만료·Secret 회전·감사 보관 정책이 리허설됨
6. 성능 목표와 보안 점검 결과가 계약 기준을 충족함

고객사 주소, 계정, 인증서, Secret, SLA 수치는 이 문서에 기록하지 않고 고객별 운영 입력서와 보호 저장소에서 관리한다.
