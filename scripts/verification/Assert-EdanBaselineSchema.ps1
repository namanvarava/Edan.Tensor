[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$contextPath = Join-Path $RepositoryRoot 'src/Edan/EDAN.Tensor.Edan.Infrastructure/Persistence/EdanDbContext.cs'
$verificationPath = Join-Path $RepositoryRoot 'scripts/edan/Verify-EdanSchema.ps1'
$requiredProjects = @(
    'src/Hosts/EDAN.Tensor.EdanMigrations/EDAN.Tensor.EdanMigrations.csproj',
    'src/Hosts/EDAN.Tensor.EdanPostgreSqlMigrations/EDAN.Tensor.EdanPostgreSqlMigrations.csproj',
    'src/Hosts/EDAN.Tensor.EdanSqlServerMigrations/EDAN.Tensor.EdanSqlServerMigrations.csproj',
    'src/Hosts/EDAN.Tensor.EdanOracleMigrations/EDAN.Tensor.EdanOracleMigrations.csproj'
)
$requiredTables = @('ed_00001','ed_00002','ed_00003','ed_00004','ed_00005','ed_01001','ed_01002','ed_01003','ed_01004','ed_01005')

if (-not (Test-Path -LiteralPath $contextPath)) { throw "EdanDbContext 누락: $contextPath" }
if (-not (Test-Path -LiteralPath $verificationPath)) { throw "Edan 스키마 검증 스크립트 누락: $verificationPath" }
$context = Get-Content -LiteralPath $contextPath -Raw

$missing = @($requiredTables | Where-Object { $context -notmatch ('ToTable\(\s*"' + [regex]::Escape($_) + '"') })
if ($missing.Count -gt 0) { throw "기준 Edan 테이블 매핑 누락: $($missing -join ', ')" }

foreach ($project in $requiredProjects) {
    if (-not (Test-Path -LiteralPath (Join-Path $RepositoryRoot $project))) { throw "Edan Provider 프로젝트 누락: $project" }
}

$baseline = Join-Path $RepositoryRoot 'docs/database/edan-baseline-schema.md'
$extension = Join-Path $RepositoryRoot 'docs/database/customer-extension-policy.md'
foreach ($document in @($baseline, $extension)) {
    if (-not (Test-Path -LiteralPath $document)) { throw "스키마 기준 문서 누락: $document" }
}

Write-Host "Edan 기준 스키마 PASS: $($requiredTables.Count)개 테이블, Provider 4종"
