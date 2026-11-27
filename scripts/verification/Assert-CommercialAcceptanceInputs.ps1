[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
    [string]$InputJson
)

$ErrorActionPreference = 'Stop'
$document = Get-Content -LiteralPath $InputJson -Raw | ConvertFrom-Json
$failures = [System.Collections.Generic.List[string]]::new()

function Get-RequiredValue {
    param([object]$Object, [string]$Path)
    $current = $Object
    foreach ($part in $Path.Split('.')) {
        if ($null -eq $current -or $null -eq $current.PSObject.Properties[$part]) { return $null }
        $current = $current.$part
    }
    if ($current -is [string] -and [string]::IsNullOrWhiteSpace($current)) { return $null }
    return $current
}

$required = @(
    'responsibility.customer', 'responsibility.operationsOwner', 'responsibility.securityApprover',
    'responsibility.deploymentApprover', 'responsibility.maintenanceWindowUtc', 'responsibility.rollbackCriteria',
    'secretManager.product', 'secretManager.accessPrincipal', 'secretManager.credentialRotation',
    'secretManager.recoveryProcedure', 'identity.issuer', 'identity.metadataAddress', 'identity.audience',
    'identity.certificateOwner', 'network.firewallPolicy', 'businessDatabase.provider',
    'businessDatabase.databaseAndSchema', 'businessDatabase.minimumPrivilegeEvidence',
    'businessDatabase.allowedTables', 'businessDatabase.allowedRoutines', 'businessDatabase.contractVersion',
    'businessDatabase.compensationApproval', 'backupAlerting.backupStore', 'backupAlerting.encryptionKeyOwner',
    'backupAlerting.retentionAndSchedule', 'backupAlerting.rpo', 'backupAlerting.rto',
    'backupAlerting.recoveryOwner', 'backupAlerting.alertChannelAndRecipients', 'backupAlerting.thresholds',
    'performanceAcceptance.concurrentUsers', 'performanceAcceptance.rps', 'performanceAcceptance.p95',
    'performanceAcceptance.p99', 'performanceAcceptance.multiDbDmlThroughput',
    'performanceAcceptance.outageTolerance', 'performanceAcceptance.finalAcceptanceEvidence'
)

foreach ($path in $required) {
    if ($null -eq (Get-RequiredValue -Object $document -Path $path)) {
        $failures.Add("Required acceptance value is missing: $path")
    }
}

foreach ($path in @('identity.issuer', 'identity.metadataAddress')) {
    $value = Get-RequiredValue -Object $document -Path $path
    if ($null -ne $value -and $value -notmatch '^https://') {
        $failures.Add("Identity endpoint must use HTTPS: $path")
    }
}

$serialized = $document | ConvertTo-Json -Depth 20
if ($serialized -match '(?i)"(password|clientSecret|privateKey|accessToken|token)"\s*:') {
    $failures.Add('Acceptance input must not contain passwords, secrets, private keys, tokens, or client secrets.')
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output "Commercial acceptance input validation passed: $InputJson"
