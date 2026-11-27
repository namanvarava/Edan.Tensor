[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$EvidenceJson
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $EvidenceJson)) {
    throw "Provider 증적 파일이 없습니다: $EvidenceJson"
}

$evidence = Get-Content -LiteralPath $EvidenceJson -Raw | ConvertFrom-Json
$required = @(
    'migrations',
    'schema',
    'health',
    'routines',
    'consistency',
    'backupRestore',
    'securityLicense'
)
$missing = @($required | Where-Object { -not ($evidence.PSObject.Properties.Name -contains $_) })
if ($missing.Count -gt 0) { throw "필수 증적 항목이 없습니다: $($missing -join ', ')" }

$failed = @($required | Where-Object { $evidence.$_ -ne 'PASS' })
if ($failed.Count -gt 0) { throw "최종 병합 불가. 통과하지 않은 항목: $($failed -join ', ')" }

Write-Output '최종 병합 증적 게이트 PASS'
Write-Output '이 결과는 병합 가능 판정만 수행하며, main 병합·브랜치 삭제는 실행하지 않습니다.'
