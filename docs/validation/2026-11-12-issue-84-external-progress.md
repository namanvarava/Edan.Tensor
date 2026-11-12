# Issue #84 비운영 IdP HTTPS 진행 검증 기록

## 검증 범위

이 문서는 Issue #84의 외부 IdP 경계 검증을 시작한 뒤 실제로 확인된 항목과 아직 수행하지 못한 항목을 분리하여 기록합니다. 이 기록은 완료 판정이나 Issue 종료 근거가 아니며, 남은 수용 기준을 수행하기 위한 중간 증거입니다.

- 검증 일시(UTC): 2026-11-12T06:38:42Z
- 검증 당시 기준 main SHA: `77ab6cdd941e33092d45797bb1b91aaca9fe2fa0`
- 문서 정정 PR의 base main SHA: `e26008290785074193230b86d6de209d4a2b9b0c`
- 검증 환경: Windows 호스트와 VirtualBox Ubuntu 비운영 VM
- 외부 IdP: Keycloak, `idp.edan.test:8443`
- 인증 모드: `External`
- Bypass 사용 여부: 사용하지 않음

## 실제 확인 결과

| 항목 | 결과 | 증거 |
| --- | --- | --- |
| OIDC Metadata HTTPS | 통과 | HTTP 200 |
| Metadata issuer | 통과 | `https://idp.edan.test:8443/realms/EDAN-NONPROD` |
| JWKS HTTPS | 통과 | Metadata의 `jwks_uri`에 HTTP 200 |
| JWKS 공개키 | 통과 | 공개키 2개와 RS256·RSA-OAEP 알고리즘 확인 |
| JWKS 개인키 노출 | 통과 | `d`, `p`, `q`, `dp`, `dq`, `qi` 필드가 없음 |
| External API Host 기동 | 통과 | 임시 격리 SQLite와 포트에서 `/health` HTTP 200 |
| 토큰 발급 | 통과 | 실제 Keycloak 토큰 발급 성공, 유효기간 300초 |
| 토큰 issuer | 통과 | Metadata issuer와 일치 |
| 토큰 audience | 통과 | `edan-tensor-api` 포함 |
| 안정적인 subject | 통과 | 허용된 비운영 테스트 subject 확인 |
| 토큰 없는 보호 요청 | 통과 | HTTP 401 |
| 정상 Tenant 조회 | 통과 | HTTP 200 |
| 다른 Tenant 접근 | 통과 | HTTP 403 |
| Bypass 로그 | 통과 | Bypass 경고 미발생 |
| API 로그의 토큰 원문 | 통과 | 토큰 형식 미검출 |
| 저장소 Release 회귀 | 통과 | 전체 273개 테스트 성공, 실패 0개, 건너뜀 0개 |
| JWT 로컬 경계 회귀 | 통과 | Issuer·Audience·만료·서명·`kid`·키 회전 테스트 1개 성공 |

토큰 원문, 비밀번호, 개인키는 증거 문서·로그·저장소에 기록하지 않았습니다.

## 아직 완료하지 못한 수용 기준

다음 항목은 현재 기록만으로 통과 처리하지 않습니다.

1. 실제 IdP가 발급한 유효 서명을 유지한 잘못된 Issuer 토큰의 HTTP 401 확인
2. 실제 IdP가 발급한 유효 서명을 유지한 잘못된 Audience 토큰의 HTTP 401 확인
3. 실제 서명 기반 만료 토큰의 HTTP 401 확인
4. 실제 서명 변경 토큰과 알 수 없는 `kid` 토큰의 HTTP 401 확인
5. 기존 키와 신규 서명을 동시에 게시한 키 회전 구간 확인
6. 기존 키 폐기 후 기존 토큰 거부와 신규 토큰 허용 확인
7. Tenant 미소속 주체의 HTTP 403 확인
8. GitHub `main` 보호 규칙·필수 Check의 실제 설정과 적용 확인

현재 JWKS의 키 2개 확인은 키 회전 완료의 증거가 아닙니다. 키 중첩·폐기 권한을 사용한 실제 회전 절차를 별도로 수행해야 합니다.

## 다음 검증 조건

- Keycloak 관리자 화면 또는 관리 API에서 기존 RS256 키와 신규 RS256 키를 겹쳐 게시합니다.
- 회전 전후에 발급한 토큰의 `kid`를 비밀값 없이 기록합니다.
- 기존 키 제거 후 기존 토큰과 신규 토큰의 HTTP 결과를 각각 확인합니다.
- 테스트 Tenant에 소속되지 않은 별도 테스트 주체를 사용하여 HTTP 403을 확인합니다.
- 저장소 관리자가 `main` 보호 규칙을 설정한 뒤 UI 또는 권한 있는 API에서 필수 Check 적용을 확인합니다.
- 모든 결과와 검증 시점의 main SHA를 Issue #84에 기록한 뒤 수용 기준을 재판정합니다.

예약 작업이나 자동화는 생성하지 않았습니다.
