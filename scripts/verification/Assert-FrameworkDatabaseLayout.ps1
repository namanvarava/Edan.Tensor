[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$compose = Get-Content (Join-Path $RepositoryRoot 'docker-compose.tenant-routing.yml') -Raw
foreach ($name in @('PlatformDatabase', 'WorkerDatabase', 'DataSetDatabase')) {
    if ($compose -notmatch "ConnectionStrings__${name}: \$\{ConnectionStrings__${name}:-Data Source=/app/App_Data/edan-framework\.db;Cache=Shared\}") {
        throw "SQLite Framework 통합 기본 연결 누락: $name"
    }
}

$scripts = @(
    'scripts/postgresql/Apply-Migrations.ps1',
    'scripts/sqlserver/Apply-Migrations.ps1',
    'scripts/sqlserver/Run-ApiHost.ps1',
    'scripts/sqlserver/Run-WorkerHost.ps1',
    'scripts/postgresql/Run-WorkerHost.ps1'
)
foreach ($relative in $scripts) {
    $text = Get-Content (Join-Path $RepositoryRoot $relative) -Raw
    if ($text -notmatch 'edan_framework') { throw "통합 Framework DB 기본값 누락: $relative" }
}

foreach ($relative in @(
    'scripts/database/postgresql/Apply-DatabaseComments.sql',
    'scripts/database/sqlserver/Apply-DatabaseComments.sql',
    'scripts/database/oracle/Apply-DatabaseComments.sql',
    'docs/database/local-dbeaver-provider-setup.md'
)) {
    if (-not (Test-Path (Join-Path $RepositoryRoot $relative))) { throw "로컬 Provider 문서/스크립트 누락: $relative" }
}

Write-Host 'Framework 통합 DB layout and DBeaver documentation: PASS'
