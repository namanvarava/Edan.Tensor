[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidateSet('Sqlite','PostgreSQL','SqlServer','Oracle')]
    [string] $Provider,
    [Parameter(Mandatory)] [string] $ConnectionString,
    [switch] $Apply
)

$ErrorActionPreference = 'Stop'
$dotnetToolPath = Join-Path $HOME ".dotnet/tools"
if (Test-Path (Join-Path $dotnetToolPath "dotnet-ef")) { $env:PATH = "$dotnetToolPath$([IO.Path]::PathSeparator)$env:PATH" }
if (-not (Get-Command dotnet-ef -ErrorAction SilentlyContinue)) { throw "dotnet-ef를 찾을 수 없습니다. 전역 dotnet-ef 도구를 설치하세요." }
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
