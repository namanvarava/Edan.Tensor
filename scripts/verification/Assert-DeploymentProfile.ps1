[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string]$EnvironmentFile,
    [Parameter(Mandatory = $true)] [ValidateSet('SME', 'Enterprise')] [string]$Profile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $EnvironmentFile -PathType Leaf)) { throw "Environment file was not found: $EnvironmentFile" }

$values = @{}
foreach ($line in Get-Content -LiteralPath $EnvironmentFile) {
    $value = $line.Trim()
    if ($value.Length -eq 0 -or $value.StartsWith('#')) { continue }
    $separator = $value.IndexOf('=')
    if ($separator -le 0) { throw "Invalid environment line: $line" }
    $values[$value.Substring(0, $separator).Trim()] = $value.Substring($separator + 1).Trim().Trim('"', "'")
}

function Required([string]$Name) {
    if (-not $values.ContainsKey($Name) -or [string]::IsNullOrWhiteSpace($values[$Name])) { throw "Missing setting: $Name" }
    return [string]$values[$Name]
}

$actualProfile = Required 'Operations__Profile'
$provider = Required 'Database__Provider'
if ($actualProfile -ne $Profile) { throw "Profile mismatch: expected $Profile, got $actualProfile" }

if ($Profile -eq 'SME') {
    if ($provider -ne 'Sqlite') { throw 'SME profile must use SQLite for Framework metadata.' }
} else {
    if ($provider -ne 'PostgreSQL') { throw 'Enterprise profile must use PostgreSQL for Framework metadata.' }
    foreach ($name in @('Operations__MinimumApiInstances','Operations__MinimumWorkerInstances','Operations__TargetRpoMinutes','Operations__TargetRtoMinutes')) { [void](Required $name) }
    if ([int](Required 'Operations__MinimumApiInstances') -lt 2) { throw 'Enterprise requires at least two API instances.' }
    if ([int](Required 'Operations__MinimumWorkerInstances') -lt 2) { throw 'Enterprise requires at least two Worker instances.' }
}

if ((Required 'ASPNETCORE_ENVIRONMENT') -ne 'Production') { throw 'Deployment profiles must run in Production.' }
if ((Required 'Authentication__Mode') -ne 'External') { throw 'Deployment profiles require External authentication.' }
if ((Required 'Authentication__MetadataAddress') -notmatch '^https://') { throw 'IdP metadata must use HTTPS.' }
Write-Host "Deployment profile PASS: $Profile / $provider"
