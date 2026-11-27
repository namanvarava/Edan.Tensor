[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
    [string]$EvidenceDirectory = 'artifacts/operational-readiness',
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $RepositoryRoot

$evidencePath = Join-Path $RepositoryRoot $EvidenceDirectory
New-Item -ItemType Directory -Force -Path $evidencePath | Out-Null
$results = [System.Collections.Generic.List[object]]::new()

function Add-Result([string]$Name, [bool]$Passed, [string]$Detail) {
    $results.Add([pscustomobject]@{ name = $Name; status = if ($Passed) { 'PASS' } else { 'FAIL' }; detail = $Detail })
}

function Assert-Path([string]$Name, [string]$RelativePath) {
    $path = Join-Path $RepositoryRoot $RelativePath
    Add-Result $Name (Test-Path -LiteralPath $path) $RelativePath
}

Assert-Path 'operational-readiness-runbook' 'docs/operations/commercial-operational-readiness.md'
Assert-Path 'commercial-gate' 'scripts/verification/Assert-CommercialCompletion.ps1'
Assert-Path 'profile-gate' 'scripts/verification/Assert-DeploymentProfile.ps1'
Assert-Path 'backup-restore-runbook' 'docs/operations/platform-database-backup-restore.md'
Assert-Path 'provider-acceptance-runbook' 'docs/operations/provider-acceptance-runbook.md'
Assert-Path 'secret-resolution-source' 'src/Platform/EDAN.Tensor.Platform.Infrastructure/Persistence/TenantDatabaseResolver.cs'
Assert-Path 'mutation-coordinator' 'src/Platform/EDAN.Tensor.Platform.Infrastructure/Persistence/TenantDatabaseMutationCoordinator.cs'
Assert-Path 'consistency-checker' 'src/Platform/EDAN.Tensor.Platform.Infrastructure/Persistence/TenantDatabaseConsistencyChecker.cs'
Assert-Path 'routine-allowlist' 'src/Platform/EDAN.Tensor.Platform.Domain/Model/TenantDatabaseRoutineAllowlist.cs'
Assert-Path 'edan-baseline-schema' 'docs/database/edan-baseline-schema.md'
Assert-Path 'customer-extension-policy' 'docs/database/customer-extension-policy.md'

try {
    pwsh -NoProfile -File (Join-Path $RepositoryRoot 'scripts/verification/Assert-EdanBaselineSchema.ps1') *> (Join-Path $evidencePath 'edan-baseline-schema.log')
    Add-Result 'edan-baseline-schema' ($LASTEXITCODE -eq 0) 'Edan SYS/BAS baseline and provider projects'
} catch { Add-Result 'edan-baseline-schema' $false $_.Exception.Message }

try {
    pwsh -NoProfile -File (Join-Path $RepositoryRoot 'scripts/verification/Assert-CommercialCompletion.ps1') -SkipTests *> (Join-Path $evidencePath 'commercial-gate.log')
    Add-Result 'commercial-gate' ($LASTEXITCODE -eq 0) 'Existing common commercial gate'
} catch { Add-Result 'commercial-gate' $false $_.Exception.Message }

foreach ($profile in @(
    @{ Name = 'SME'; File = 'docs/operations/smb-default-profile.env.example' },
    @{ Name = 'Enterprise'; File = 'docs/operations/enterprise-default-profile.env.example' }
)) {
    try {
        pwsh -NoProfile -File (Join-Path $RepositoryRoot 'scripts/verification/Assert-DeploymentProfile.ps1') -EnvironmentFile (Join-Path $RepositoryRoot $profile.File) -Profile $profile.Name *> (Join-Path $evidencePath "$($profile.Name)-profile.log")
        Add-Result "$($profile.Name)-profile" ($LASTEXITCODE -eq 0) $profile.File
    } catch { Add-Result "$($profile.Name)-profile" $false $_.Exception.Message }
}

try {
    git diff --check *> (Join-Path $evidencePath 'diff-check.log')
    Add-Result 'diff-check' ($LASTEXITCODE -eq 0) 'Whitespace and patch validation'
} catch { Add-Result 'diff-check' $false $_.Exception.Message }

if (-not $SkipTests) {
    try {
        dotnet test EDAN.Tensor.sln --configuration Release --no-restore --logger 'console;verbosity=minimal' *> (Join-Path $evidencePath 'release-tests.log')
        Add-Result 'release-tests' ($LASTEXITCODE -eq 0) 'Release test suite'
    } catch { Add-Result 'release-tests' $false $_.Exception.Message }
}

$failed = @($results | Where-Object status -eq 'FAIL')
$summary = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('O')
    status = if ($failed.Count -eq 0) { 'Passed' } else { 'Failed' }
    productReadiness = if ($failed.Count -eq 0) { 'Common product operational readiness passed' } else { 'Blocked' }
    customerAcceptance = 'Required: customer Secret Manager, backup/restore, HA failover, performance, observability evidence'
    results = $results
}
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $evidencePath 'summary.json') -Encoding utf8

if ($failed.Count -gt 0) { throw "운영 준비도 게이트 실패: $($failed.name -join ', ')" }
Write-Host '운영 준비도 게이트 PASS. 고객사별 운영 인수 증적은 별도 제출이 필요합니다.'
