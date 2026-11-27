[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
    [string]$EvidenceDirectory = 'artifacts/commercial-completion-gate',
    [string]$CustomerInputJson,
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $RepositoryRoot

$evidencePath = Join-Path $RepositoryRoot $EvidenceDirectory
New-Item -ItemType Directory -Force -Path $evidencePath | Out-Null
$results = [System.Collections.Generic.List[object]]::new()

function Add-Result([string]$Name, [bool]$Passed, [string]$Detail) {
    $results.Add([pscustomobject]@{
        name = $Name
        status = if ($Passed) { 'PASS' } else { 'FAIL' }
        detail = $Detail
    })
}

function Assert-Path([string]$Name, [string]$RelativePath) {
    $path = Join-Path $RepositoryRoot $RelativePath
    Add-Result $Name (Test-Path -LiteralPath $path) $RelativePath
}

Assert-Path 'production-configuration-gate' 'scripts/verification/Assert-ProductionConfiguration.ps1'
Assert-Path 'provider-migration-gate' 'scripts/verification/Verify-ProviderMigrationArtifacts.ps1'
Assert-Path 'commercial-input-template' 'docs/operations/commercial-acceptance-input-template.md'
Assert-Path 'sme-profile' 'docs/operations/smb-default-profile.md'
Assert-Path 'enterprise-profile' 'docs/operations/enterprise-default-profile.md'
Assert-Path 'enterprise-profile-environment' 'docs/operations/enterprise-default-profile.env.example'
Assert-Path 'provider-runbook' 'docs/operations/provider-acceptance-runbook.md'

$requiredSources = @(
    'src/Platform/EDAN.Tensor.Platform.Infrastructure/Persistence/TenantDatabaseMutationCoordinator.cs',
    'src/Platform/EDAN.Tensor.Platform.Infrastructure/Persistence/TenantDatabaseConsistencyChecker.cs',
    'src/Platform/EDAN.Tensor.Platform.Infrastructure/Persistence/TenantDatabaseResolver.cs',
    'src/Licensing/EDAN.Tensor.Licensing.Application/Services/LicenseValidationService.cs',
    'src/Worker/EDAN.Tensor.Worker.Infrastructure/Persistence/WorkerTaskRepository.cs'
)
foreach ($source in $requiredSources) {
    Assert-Path "source:$source" $source
}

$requiredTests = @(
    'tests/EDAN.Tensor.Platform.Infrastructure.Tests/PlatformInfrastructureTests.cs',
    'tests/EDAN.Tensor.Worker.Infrastructure.Tests/WorkerProcessCrashRecoveryTests.cs',
    'tests/EDAN.Tensor.Licensing.Tests/LicenseAuthorityTests.cs'
)
foreach ($test in $requiredTests) {
    Assert-Path "test:$test" $test
}

try {
    pwsh -NoProfile -File (Join-Path $RepositoryRoot 'scripts/verification/Verify-ProviderMigrationArtifacts.ps1') *> (Join-Path $evidencePath 'provider-migrations.log')
    Add-Result 'provider-migration-static' ($LASTEXITCODE -eq 0) 'Provider migration artifacts'
} catch {
    Add-Result 'provider-migration-static' $false $_.Exception.Message
}

try {
    git diff --check *> (Join-Path $evidencePath 'diff-check.log')
    Add-Result 'diff-check' ($LASTEXITCODE -eq 0) 'Whitespace and patch validation'
} catch {
    Add-Result 'diff-check' $false $_.Exception.Message
}

if (-not $SkipTests) {
    try {
        dotnet test EDAN.Tensor.sln --configuration Release --no-restore --logger 'console;verbosity=minimal' *> (Join-Path $evidencePath 'release-tests.log')
        Add-Result 'release-tests' ($LASTEXITCODE -eq 0) 'Release test suite'
    } catch {
        Add-Result 'release-tests' $false $_.Exception.Message
    }
}

$customerAcceptanceRequired = $false
if (-not [string]::IsNullOrWhiteSpace($CustomerInputJson)) {
    $customerAcceptanceRequired = $true
    try {
        pwsh -NoProfile -File (Join-Path $RepositoryRoot 'scripts/verification/Assert-CommercialAcceptanceInputs.ps1') -InputJson $CustomerInputJson *> (Join-Path $evidencePath 'customer-acceptance.log')
        Add-Result 'customer-acceptance-inputs' ($LASTEXITCODE -eq 0) $CustomerInputJson
    } catch {
        Add-Result 'customer-acceptance-inputs' $false $_.Exception.Message
    }
}

$failed = @($results | Where-Object status -eq 'FAIL')
$summary = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('O')
    repositoryRoot = $RepositoryRoot
    productScope = '공통 제품 구현·자동 검증. 고객사별 인프라·Secret·SLA는 별도 인수 단계.'
    customerAcceptanceRequired = $customerAcceptanceRequired
    status = if ($failed.Count -eq 0) { 'Passed' } else { 'Failed' }
    results = $results
}
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $evidencePath 'summary.json') -Encoding utf8

if ($failed.Count -gt 0) {
    throw "상용 공통 완료 게이트 실패: $($failed.name -join ', ')"
}
Write-Host '상용 공통 완료 게이트 PASS. 고객별 운영 인수는 별도 입력이 필요합니다.'
