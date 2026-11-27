[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
    [string]$EvidenceDirectory = 'artifacts/local-completion-gate',
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
Set-Location $RepositoryRoot
$evidencePath = Join-Path $RepositoryRoot $EvidenceDirectory
New-Item -ItemType Directory -Force -Path $evidencePath | Out-Null
$failed = $false

function Invoke-Gate([string]$Name, [scriptblock]$Action) {
    $path = Join-Path $evidencePath "$Name.log"
    Write-Host "[$Name] 시작"
    & $Action *> $path
    $code = $LASTEXITCODE
    if ($null -eq $code) { $code = 0 }
    if ($code -ne 0) {
        $script:failed = $true
        Write-Host "[$Name] FAIL (exit $code)"
    } else {
        Write-Host "[$Name] PASS"
    }
}

Invoke-Gate 'provider-migration-static' {
    pwsh -NoProfile -File (Join-Path $RepositoryRoot 'scripts/verification/Verify-ProviderMigrationArtifacts.ps1')
}

Invoke-Gate 'restore' {
    dotnet restore EDAN.Tensor.sln -m:1
}

Invoke-Gate 'release-build' {
    dotnet build EDAN.Tensor.sln --configuration Release --no-restore -m:1
}

if (-not $SkipTests) {
    Invoke-Gate 'release-tests' {
        dotnet test EDAN.Tensor.sln --configuration Release --no-build --no-restore --logger 'console;verbosity=minimal'
    }
}

Invoke-Gate 'package-vulnerability' {
    dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive
}

Invoke-Gate 'diff-check' {
    git diff --check
}

$summary = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('O')
    repositoryRoot = $RepositoryRoot
    testsSkipped = [bool]$SkipTests
    status = if ($failed) { 'Failed' } else { 'Passed' }
}
$summary | ConvertTo-Json | Set-Content (Join-Path $evidencePath 'summary.json')

if ($failed) { throw "로컬 완료 게이트 실패. 증적: $evidencePath" }
Write-Host "로컬 완료 게이트 PASS. 증적: $evidencePath"
