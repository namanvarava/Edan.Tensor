[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$EnvironmentFile,

    [switch]$AllowOfflineSqlite,
    [switch]$AllowPlaintextSecrets
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $EnvironmentFile -PathType Leaf)) {
    throw "Production environment file was not found: $EnvironmentFile"
}

$values = @{}
foreach ($line in Get-Content -LiteralPath $EnvironmentFile) {
    $trimmed = $line.Trim()
    if ($trimmed.Length -eq 0 -or $trimmed.StartsWith('#')) { continue }
    $separator = $trimmed.IndexOf('=')
    if ($separator -le 0) { throw "Invalid environment line in $EnvironmentFile" }
    $key = $trimmed.Substring(0, $separator).Trim()
    $value = $trimmed.Substring($separator + 1).Trim()
    $values[$key] = $value.Trim('"', "'")
}

function Get-Required([string]$Name) {
    if (-not $values.ContainsKey($Name) -or [string]::IsNullOrWhiteSpace($values[$Name])) {
        throw "Required production setting is missing: $Name"
    }
    return [string]$values[$Name]
}

$environment = Get-Required 'ASPNETCORE_ENVIRONMENT'
if ($environment -ne 'Production') { throw 'ASPNETCORE_ENVIRONMENT must be Production.' }

$provider = Get-Required 'Database__Provider'
if ($provider -eq 'Sqlite') {
    if (-not $AllowOfflineSqlite) { throw 'SQLite is allowed only with -AllowOfflineSqlite for an approved offline single-customer deployment.' }
} elseif ($provider -notin @('PostgreSQL', 'SqlServer', 'Oracle')) {
    throw "Unsupported production provider: $provider"
}

$authMode = Get-Required 'Authentication__Mode'
if ($authMode -ne 'External') { throw 'Production authentication must use External mode.' }
$metadata = Get-Required 'Authentication__MetadataAddress'
$issuer = Get-Required 'Authentication__Issuer'
if (-not $metadata.StartsWith('https://', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Authentication metadata address must use HTTPS in Production.'
}
if (-not $issuer.StartsWith('https://', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Authentication issuer must use HTTPS in Production.'
}
if ($values.ContainsKey('Authentication__RequireHttpsMetadata') -and
    $values['Authentication__RequireHttpsMetadata'].ToLowerInvariant() -ne 'true') {
    throw 'Authentication__RequireHttpsMetadata must be true in Production.'
}

foreach ($name in @(
        'ConnectionStrings__PlatformDatabase',
        'ConnectionStrings__WorkerDatabase',
        'ConnectionStrings__DataSetDatabase',
        'ConnectionStrings__DataSetContentDatabase')) {
    [void](Get-Required $name)
}

if (-not $AllowPlaintextSecrets) {
    $raw = Get-Content -LiteralPath $EnvironmentFile -Raw
    if ($raw -match '(?im)(^|;|\s)Password\s*=') {
        throw 'Plaintext database passwords must be injected by an approved Secret Manager, not stored in the environment file.'
    }
    if ($raw -match '(?im)^.*(ClientSecret|PrivateKey|Token|Password)\s*=\s*[^$\{\s].+$') {
        throw 'Plaintext credential material must not be stored in the environment file.'
    }
}

if ($provider -eq 'PostgreSQL') {
    foreach ($name in @('ConnectionStrings__PlatformDatabase', 'ConnectionStrings__WorkerDatabase', 'ConnectionStrings__DataSetDatabase')) {
        if ($values[$name] -match '(?i)Ssl\s*Mode\s*=\s*(Disable|Prefer)') {
            throw "$name must require PostgreSQL TLS in Production."
        }
    }
}
if ($provider -eq 'SqlServer') {
    foreach ($name in @('ConnectionStrings__PlatformDatabase', 'ConnectionStrings__WorkerDatabase', 'ConnectionStrings__DataSetDatabase')) {
        if ($values[$name] -notmatch '(?i)Encrypt\s*=\s*(True|Mandatory)') {
            throw "$name must enable SQL Server encryption in Production."
        }
        if ($values[$name] -match '(?i)TrustServerCertificate\s*=\s*True') {
            throw "$name must not trust an unverified SQL Server certificate in Production."
        }
    }
}

Write-Output "Production configuration PASS: Provider=$provider; Authentication=External; TLS policy=verified"
