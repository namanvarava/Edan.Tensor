# 프로젝트 구조

| 경로 | 책임 |
| --- | --- |
| `src/Platform` | SharedKernel 및 Platform Domain·Application·Infrastructure를 포함합니다. |
| `src/DataSet` | Tenant 소유 메타데이터와 콘텐츠 저장소 추상화·구현을 분리합니다. |
| `src/Worker` | 작업 수명주기와 전용 영속 저장소를 제공합니다. |
| `src/Commercial` | 상용 업무 모듈의 Contracts·Domain·Application·Infrastructure 경계를 유지합니다. |
| `src/Licensing` | 라이선스 모듈의 Contracts·Domain·Application·Infrastructure 경계를 유지합니다. |
| `src/Hosts` | API·Worker·LicenseAuthority의 실행 진입점과 공통 HostServices를 포함합니다. |
| `tests` | 계층별 테스트, API 통합 테스트 및 Worker 프로세스 장애 검증 프로그램을 포함합니다. |

개발자는 현재의 [검증 현황](../validation/2026-11-06-main-ci-and-scope.md)과 실제 소스를 함께 확인해야 합니다.
Commercial은 공급자 중립 결제 경계까지만 구현하고 상품·가격·Webhook·환불·유료 권리는 아직 확정하지 않았습니다. Licensing은 승인된 개발 라이선스 발급·검증·폐기 기능을 별도 LicenseAuthority로 구현했습니다. WPF 클라이언트는 현재 소스에 없습니다.
