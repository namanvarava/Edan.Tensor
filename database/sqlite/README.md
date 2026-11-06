# SQLite 저장소

SQLite는 로컬 개발 환경의 기본 저장소입니다. Platform 메타데이터, DataSet 메타데이터와 콘텐츠, Worker 작업 상태는 각각 전용 연결과 DbContext 경계를 사용하며 EF Core Migration으로 스키마를 관리합니다. 연결 문자열과 개발 환경 Migration 설정은 [로컬 DB 설정 안내](../../docs/operations/platform-database-local-setup.md)에 설명되어 있습니다.

운영 환경에서는 승인된 배포 작업으로 Migration을 적용하고, 실제 데이터베이스 파일·운영 비밀 값·개인 서명 키를 저장소에 커밋하지 않습니다.
