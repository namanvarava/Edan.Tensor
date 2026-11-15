# EDAN.Tensor 데이터베이스 명세 확정본

이 디렉터리는 2026-11-15에 검토를 마친 데이터베이스 명세 확정본을 보관합니다.

## 포함 산출물

- `edan-tensor-schema-dictionary-final.xlsx`: 전자사전, 테이블 목록, 컬럼 명세, 인덱스 명세, 변경 영향, 33개 테이블별 명세 시트와 테이블 시트 목록을 포함한 검토용 엑셀입니다.
- `sqlite-validation.json`: 현재 개발용 SQLite 네 개의 무결성 검사, 외래키 위반 검사, 테이블·컬럼·인덱스·외래키 조회 결과입니다.
- `final-inspect.ndjson`: 확정 엑셀의 시트와 표 구조를 확인한 검사 결과입니다.
- `verify-inspect.ndjson`: 확정 엑셀의 최종 시트·명세 상태 검증 결과입니다.

## 확정 범위

- 제품 테이블은 Platform 19개, DataSet 9개, DataSetContent 1개, Worker 4개로 총 33개입니다.
- 컬럼 명세 287건과 인덱스 명세 33건의 검토 상태를 모두 `확정(명세)`로 기록했습니다.
- 테이블별 명세 시트는 `T01_`부터 `T33_`까지 생성했습니다.
- 현재 SQLite 네 개는 모두 `integrity_check=ok`이며 외래키 위반은 0건입니다.
- SQLite 설명 메타데이터인 `__schema_comments`에는 전체 362건의 설명 행이 확인되었습니다.

## 물리 스키마 적용 경계

이번 적용은 승인된 명세·문서·검증 결과를 확정한 것입니다. 기존 EF Core Migration과 공개 API의 호환성을 보존하기 위해 현행 물리 테이블·컬럼명은 변경하지 않았습니다. 권장 논리명으로 실제 DB를 변경하려면 별도 Migration 승인, 데이터 변환, 롤백, 네 DB 회귀 검증이 필요합니다.

## 로그 처리

로컬 실행·테스트 로그는 삭제하지 않고 `.gitignore`의 `*.log`, `*.err.log`, `*.out.log` 규칙으로 저장소 병합에서 제외했습니다. 로그는 장애 분석 증거가 될 수 있으므로 보존 정책과 민감정보 포함 여부를 확인한 뒤 운영 정책에 따라 관리합니다.

## Release 검증 상태

- `dotnet restore EDAN.Tensor.sln`은 .NET SDK 10.0.402의 Workload Resolver가 `Microsoft.NET.SDK.WorkloadAutoImportPropsLocator`와 `Microsoft.NET.SDK.WorkloadManifestTargetsLocator`를 찾지 못하는 `MSB4276` 환경 오류로 실패했습니다.
- `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`도 같은 SDK Workload Resolver 환경 오류로 실패했습니다. 소스 컴파일 오류와 경고는 출력되지 않았습니다.
- `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`는 테스트 산출물을 찾은 뒤 testhost가 90초 동안 연결되지 않아 실행이 중단되었습니다.
- 따라서 이 명세 변경만으로 Release 전체 검증과 개발 완료를 선언하지 않습니다. .NET SDK Workload Resolver를 복구한 환경에서 복원·빌드·전체 테스트와 최신 main CI 성공을 다시 확인해야 합니다.
