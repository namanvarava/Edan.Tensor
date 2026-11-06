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
Commercial·Licensing의 기반 프로젝트는 업무 기능 완료를 의미하지 않으며 WPF 클라이언트는 현재 소스에 없습니다.
