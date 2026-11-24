[CmdletBinding()]
param(
    [Parameter(Mandatory)] [Guid] $TenantId,
    [Parameter(Mandatory)] [ValidateSet('SQLite','PostgreSQL','SqlServer','Oracle')] [string] $Provider,
    [Parameter(Mandatory)] [string] $ApiBaseUrl,
    [Parameter(Mandatory)] [string] $BearerToken,
    [Parameter(Mandatory)] [string] $Username,
    [Parameter(Mandatory)] [string] $SecretKey,
    [Parameter(Mandatory)] [string] $ServerAddress,
    [Parameter(Mandatory)] [int] $Port,
    [Parameter(Mandatory)] [string] $DatabaseName,
    [Parameter(Mandatory)] [string] $SchemaName,
    [string] $RepositoryRoot = (Join-Path $PSScriptRoot '..\..'),
    [switch] $SkipMigration
)

$ErrorActionPreference = 'Stop'
$headers = @{ Authorization = "Bearer $BearerToken" }
$bindingUri = "$ApiBaseUrl/api/platform/tenants/$TenantId/database-binding"

function Invoke-Json($method, $uri, $body) {
    $params = @{ Method = $method; Uri = $uri; Headers = $headers; UseBasicParsing = $true }
    if ($null -ne $body) { $params.ContentType = 'application/json'; $params.Body = ($body | ConvertTo-Json -Depth 10) }
    Invoke-RestMethod @params
}

if (-not $SkipMigration) {
    switch ($Provider) {
        'PostgreSQL' { & (Join-Path $RepositoryRoot 'scripts\postgresql\Apply-Migrations.ps1') -Apply }
        'SqlServer' { & (Join-Path $RepositoryRoot 'scripts\sqlserver\Apply-Migrations.ps1') -Apply }
        'Oracle' { & (Join-Path $RepositoryRoot 'scripts\oracle\Apply-Migrations.ps1') -Apply }
        'SQLite' { Write-Host 'SQLite는 API의 개발용 마이그레이션 경로를 사용합니다.' }
    }
    if ($LASTEXITCODE -ne 0) { throw "${Provider} Migration 적용에 실패했습니다. 종료 코드: $LASTEXITCODE" }
}

$binding = [ordered]@{
    Provider = $Provider
    Username = $Username
    SecretKey = $SecretKey
    ServerAddress = $ServerAddress
    Port = $Port
    DatabaseName = $DatabaseName
    SchemaName = $SchemaName
}
Invoke-Json 'POST' $bindingUri $binding | Out-Null
Invoke-Json 'POST' "$bindingUri/activate" $null | Out-Null
$health = Invoke-Json 'POST' "$bindingUri/health" $null
if ($health.Status -ne 'Healthy') { throw '테넌트 DB Health Check가 성공하지 않았습니다.' }
Write-Host "테넌트 DB Provisioning 완료: $TenantId / $Provider"
