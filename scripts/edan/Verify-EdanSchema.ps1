[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$context = Join-Path $root 'src\Edan\EDAN.Tensor.Edan.Infrastructure\Persistence\EdanDbContext.cs'
if (-not (Test-Path $context)) { throw "EdanDbContext를 찾을 수 없습니다." }
$text = Get-Content $context -Raw

$expected = @(
    'ed_00001','ed_00002','ed_00003','ed_00004','ed_00005',
    'ed_01001','ed_01002','ed_01003','ed_01004','ed_01005'
)
$missing = @($expected | Where-Object { $text -notmatch ('ToTable\(\s*"' + [regex]::Escape($_) + '"') })
if ($missing.Count -gt 0) { throw "Edan table mapping 누락: $($missing -join ', ')" }

$projects = @{
    Sqlite = 'src\Edan\EDAN.Tensor.Edan.Infrastructure\Persistence\Migrations'
    PostgreSQL = 'src\Hosts\EDAN.Tensor.EdanPostgreSqlMigrations\Migrations'
    SqlServer = 'src\Hosts\EDAN.Tensor.EdanSqlServerMigrations\Migrations'
    Oracle = 'src\Hosts\EDAN.Tensor.EdanOracleMigrations\Migrations'
}
foreach ($entry in $projects.GetEnumerator()) {
    $dir = Join-Path $root $entry.Value
    $files = if (Test-Path $dir) { @(Get-ChildItem $dir -Filter '*InitialEdanSysBasSchema.cs' -File) } else { @() }
    if ($files.Count -ne 1) { throw "$($entry.Key) Edan 초기 Migration이 정확히 1개가 아닙니다: $($files.Count)" }
    Write-Host ("{0}: PASS ({1})" -f $entry.Key, $files[0].Name)
}
Write-Host 'Edan SYS/BAS mapping and provider migration structure: PASS'
