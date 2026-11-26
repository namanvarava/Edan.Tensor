[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
)

$ErrorActionPreference = 'Stop'
$failures = [System.Collections.Generic.List[string]]::new()

function Assert-Condition([bool]$Condition, [string]$Message) {
    if (-not $Condition) { $failures.Add($Message) }
}

$providerProjects = @(
    'src/Hosts/EDAN.Tensor.PostgreSqlMigrations/EDAN.Tensor.PostgreSqlMigrations.csproj',
    'src/Hosts/EDAN.Tensor.SqlServerMigrations/EDAN.Tensor.SqlServerMigrations.csproj',
    'src/Hosts/EDAN.Tensor.OracleMigrations/EDAN.Tensor.OracleMigrations.csproj',
    'src/Hosts/EDAN.Tensor.EdanMigrations/EDAN.Tensor.EdanMigrations.csproj',
    'src/Hosts/EDAN.Tensor.EdanPostgreSqlMigrations/EDAN.Tensor.EdanPostgreSqlMigrations.csproj',
    'src/Hosts/EDAN.Tensor.EdanSqlServerMigrations/EDAN.Tensor.EdanSqlServerMigrations.csproj',
    'src/Hosts/EDAN.Tensor.EdanOracleMigrations/EDAN.Tensor.EdanOracleMigrations.csproj'
)

foreach ($relative in $providerProjects) {
    $path = Join-Path $RepositoryRoot $relative
    Assert-Condition (Test-Path -LiteralPath $path) "Provider Migration 프로젝트가 없습니다: $relative"
}

$contextFiles = @(
    'src/Platform/EDAN.Tensor.Platform.Infrastructure/Persistence/PlatformDbContext.cs',
    'src/Worker/EDAN.Tensor.Worker.Infrastructure/Persistence/WorkerDbContext.cs',
    'src/DataSet/EDAN.Tensor.DataSet.Infrastructure/Persistence/DataSetDbContext.cs',
    'src/Edan/EDAN.Tensor.Edan.Infrastructure/Persistence/EdanDbContext.cs'
)
foreach ($relative in $contextFiles) {
    $path = Join-Path $RepositoryRoot $relative
    Assert-Condition (Test-Path -LiteralPath $path) "DbContext가 없습니다: $relative"
}

$platform = Get-Content (Join-Path $RepositoryRoot $contextFiles[0]) -Raw
$worker = Get-Content (Join-Path $RepositoryRoot $contextFiles[1]) -Raw
$dataset = Get-Content (Join-Path $RepositoryRoot $contextFiles[2]) -Raw
$edan = Get-Content (Join-Path $RepositoryRoot $contextFiles[3]) -Raw

foreach ($table in 1..14) {
    Assert-Condition ($platform.Contains(('edw_01{0:D3}' -f $table)) -or $platform.Contains(('edw_010{0:D2}' -f $table))) "Platform 순차 테이블 매핑 누락: edw_010{0:D2}" -f $table
}
foreach ($table in 4..7) {
    Assert-Condition ($worker.Contains(('edw_03{0:D3}' -f $table))) "Worker 순차 테이블 매핑 누락: edw_03{0:D3}" -f $table
}
foreach ($table in 1..9) {
    Assert-Condition ($dataset.Contains(('edw_05{0:D3}' -f $table))) "DataSet 순차 테이블 매핑 누락: edw_05{0:D3}" -f $table
}
foreach ($table in 1..5) {
    Assert-Condition ($edan.Contains(('ed_000{0:D2}' -f $table))) "Edan SYS 테이블 매핑 누락: ed_000{0:D2}" -f $table
}
foreach ($table in 1..5) {
    Assert-Condition ($edan.Contains(('ed_010{0:D2}' -f $table))) "Edan BAS 테이블 매핑 누락: ed_010{0:D2}" -f $table
}

$migrationRoots = @(
    'src/Platform/EDAN.Tensor.Platform.Infrastructure/Persistence/Migrations',
    'src/Worker/EDAN.Tensor.Worker.Infrastructure/Persistence/Migrations',
    'src/DataSet/EDAN.Tensor.DataSet.Infrastructure/Persistence/Migrations',
    'src/Edan/EDAN.Tensor.Edan.Infrastructure/Persistence/Migrations'
)
foreach ($relative in $migrationRoots) {
    $path = Join-Path $RepositoryRoot $relative
    Assert-Condition (Test-Path -LiteralPath $path) "Migration 디렉터리가 없습니다: $relative"
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    throw "Provider Migration 정적 검증 실패: $($failures.Count)건"
}

Write-Output 'Provider Migration 정적 검증 PASS'
Write-Output "검증 프로젝트: $($providerProjects.Count)개"
Write-Output '검증 DbContext: Platform, Worker, DataSet, Edan'
