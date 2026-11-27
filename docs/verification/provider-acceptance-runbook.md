# Provider 수용 검증 절차

이 절차는 개발 VM에서 PostgreSQL·SQL Server·Oracle·SQLite를 실제로 검증할 때 사용합니다. 모든 결과는 적용 시각, Git commit, DB 버전, Migration 목록, 백업 파일명과 함께 보관합니다.

## 1. 공통 사전 조건

```bash
cd /home/vboxuser/edan-tensor
git pull --ff-only origin main
dotnet --info
pwsh -File ./scripts/verification/Verify-ProviderMigrationArtifacts.ps1
pwsh -File ./scripts/edan/Verify-EdanSchema.ps1
```

로컬 변경 검증은 다음 실행기로 반복할 수 있습니다. 이 명령은 외부 DB를 변경하지 않으며, 결과를 `artifacts/local-completion-gate`에 저장합니다.

```bash
pwsh -File ./scripts/verification/Invoke-LocalCompletionGate.ps1
```

다음 조건 중 하나라도 실패하면 실제 Migration을 적용하지 않습니다.

- `docker ps`에서 대상 DB가 `Up` 또는 `healthy`
- 서비스 계정 연결 확인
- 현재 DB 백업 파일 생성 및 복구 위치 확인
- 적용 전 테이블·컬럼·PK·FK·Index 목록과 행 수 저장

## 2. Framework Migration 적용

미리보기부터 실행합니다.

```bash
pwsh -File ./scripts/postgresql/Apply-Migrations.ps1
pwsh -File ./scripts/sqlserver/Apply-Migrations.ps1 -Server 192.168.56.101 -Port 1433 -User edan
pwsh -File ./scripts/oracle/Apply-Migrations.ps1
```

검토한 SQL과 백업 증적이 승인된 경우에만 각 명령에 `-Apply`를 추가합니다. 적용 후에는 같은 명령을 다시 실행해 `No migrations were applied` 또는 `database is already up to date`를 확인합니다.

## 3. Edan SYS/BAS Migration 적용

```bash
pwsh -File ./scripts/edan/Apply-Migrations.ps1 -Provider Sqlite -ConnectionString 'Data Source=/app/App_Data/edan.db' -Apply
pwsh -File ./scripts/edan/Apply-Migrations.ps1 -Provider PostgreSQL -ConnectionString '<Edan PostgreSQL connection>' -Apply
pwsh -File ./scripts/edan/Apply-Migrations.ps1 -Provider SqlServer -ConnectionString '<Edan SQL Server connection>' -Apply
pwsh -File ./scripts/edan/Apply-Migrations.ps1 -Provider Oracle -ConnectionString '<Edan Oracle connection>' -Apply
```

적용 후 다음 물리 테이블이 모두 존재해야 합니다.

```text
ed_00001 ed_00002 ed_00003 ed_00004 ed_00005
ed_01001 ed_01002 ed_01003 ed_01004 ed_01005
```

## 4. API·Provider 검증

```bash
curl -fsS http://127.0.0.1:5000/health
curl -fsS http://127.0.0.1:5000/health/database
curl -fsS http://127.0.0.1:5001/health
curl -fsS http://127.0.0.1:5001/health/database

pwsh -File ./scripts/tenant-routing/Verify-ExternalProviders.ps1 \
  -TenantId '<tenant-id>' \
  -BindingId '<binding-id>' \
  -Schema '<provider-schema>' \
  -ProviderHost '127.0.0.1' \
  -TokenFile '/tmp/edan-token-external.json'
```

Provider를 여러 개 검증할 때는 각 바인딩 생성 응답의 `bindingId`를 해당 명령에 넣습니다. 토큰은 실행 직전에 새로 발급하고, 만료된 토큰을 재사용하지 않습니다.

Provider 검증 결과에서 다음 항목이 모두 `PASS`여야 합니다.

- TCP PostgreSQL·SQL Server·Oracle
- API/Worker Health
- API/Worker Database Health
- Tenant DB Health
- Table Metadata
- Routine Metadata

## 5. 정합성·복구 게이트

적용 전후에 다음을 비교합니다.

- Migration History 누락·중복 없음
- 테이블별 행 수 동일 또는 승인된 증가량
- PK·FK·Index 개수 및 이름 일치
- Routine Allowlist와 실제 Routine 존재 여부 일치
- 샘플 Procedure/Function 실행 결과 일치
- 백업 복원 DB에서 동일 Health·Metadata 결과

## 6. 병합 승인 조건

다음 증적이 모두 저장되기 전에는 PR 병합과 브랜치 삭제를 수행하지 않습니다.

1. 전체 솔루션 빌드 결과
2. 단위·인프라·통합 테스트 결과
3. 4개 Provider Migration 결과
4. 적용 전후 스키마 및 행 수 비교표
5. 백업·복구 결과
6. Health·Metadata·Routine 검증 결과
7. 보안·라이선스·Secret 점검 결과
8. 리뷰 승인 및 CI 결과

## 7. 최종 증적 파일 작성

모든 Provider 검증 결과를 확인한 뒤 비밀값을 제외하고 다음 파일을 작성합니다.

```bash
mkdir -p artifacts
cat > artifacts/provider-acceptance-evidence.json <<'EOF'
{
  "migrations": "PASS",
  "schema": "PASS",
  "health": "PASS",
  "routines": "PASS",
  "consistency": "PASS",
  "backupRestore": "PASS",
  "securityLicense": "PASS"
}
EOF

pwsh -File ./scripts/verification/Assert-FinalMergeEvidence.ps1 \
  -EvidenceJson ./artifacts/provider-acceptance-evidence.json
```

하나라도 `FAIL`이면 JSON에 `FAIL`을 기록하고 병합하지 않습니다. 이 게이트는 병합 가능 여부만 판정하며 실제 `main` 병합이나 브랜치 삭제는 수행하지 않습니다.
