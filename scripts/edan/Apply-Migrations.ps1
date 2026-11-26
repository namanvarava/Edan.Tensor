[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidateSet('Sqlite','PostgreSQL','SqlServer','Oracle')]
    [string] $Provider,
    [Parameter(Mandatory)] [string] $ConnectionString,
    [switch] $Apply
)

$ErrorActionPreference = 'Stop'
$projectName = switch ($Provider) {
    'Sqlite' { 'EDAN.Tensor.EdanMigrations' }
    'PostgreSQL' { 'EDAN.Tensor.EdanPostgreSqlMigrations' }
    'SqlServer' { 'EDAN.Tensor.EdanSqlServerMigrations' }
    'Oracle' { 'EDAN.Tensor.EdanOracleMigrations' }
}
$project = Join-Path $PSScriptRoot "..\..\src\Hosts\$projectName\$projectName.csproj"
$project = [IO.Path]::GetFullPath($project)

if (-not (Test-Path $project)) { throw "Edan Migration 프로젝트를 찾을 수 없습니다: $project" }

$env:Database__Provider = $Provider
$env:ConnectionStrings__EdanDatabase = $ConnectionString

$migrationList = dotnet ef migrations list --project $project --context EdanDbContext --no-build
if ($LASTEXITCODE -ne 0) { throw "Edan $Provider Migration 목록 조회에 실패했습니다." }

if (-not $Apply) {
    $migrationList
    Write-Host "검토 모드입니다. 실제 적용은 -Apply를 지정하세요."
    exit 0
}

dotnet ef database update --project $project --context EdanDbContext --no-build
if ($LASTEXITCODE -ne 0) { throw "Edan $Provider Migration 적용에 실패했습니다." }
Write-Host "Edan $Provider Migration 적용이 완료되었습니다."
