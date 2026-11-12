param(
    [int]$Port = 5201
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)

function New-LicenseAuthorityStartInfo {
    param(
        [string]$DllPath,
        [string]$WorkingDirectory,
        [int]$Port,
        [string]$KeyPath,
        [string]$LicensePath,
        [string]$StatusPath,
        [string]$EnvironmentName,
        [bool]$EnableIssuance
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = "dotnet"
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    # Windows PowerShell 5.1에는 ArgumentList가 없으므로 단일 DLL 인수를 따옴표로 전달합니다.
    $startInfo.Arguments = '"' + $DllPath + '"'

    $environment = @{
        ASPNETCORE_ENVIRONMENT = $EnvironmentName
        ASPNETCORE_URLS = "http://127.0.0.1:$Port"
        LicenseAuthority__EnableDevelopmentIssuance = $EnableIssuance.ToString().ToLowerInvariant()
        LicenseAuthority__PrivateKeyPath = $KeyPath
        LicenseAuthority__DevelopmentLicensePath = $LicensePath
        LicenseAuthority__StatusPath = $StatusPath
    }

    foreach ($entry in $environment.GetEnumerator()) {
        $startInfo.Environment[$entry.Key] = $entry.Value
    }

    return $startInfo
}

function Wait-ForLicenseAuthority {
    param([Diagnostics.Process]$Process, [string]$Uri)

    $lastStatusCode = "none"
    $lastError = "none"
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        Start-Sleep -Milliseconds 250
        try {
            $response = Invoke-WebRequest -Uri $Uri -UseBasicParsing -TimeoutSec 2
            $lastStatusCode = [int]$response.StatusCode
            if ($lastStatusCode -eq 200) {
                $statusCode = $lastStatusCode
                return [pscustomobject]@{ StatusCode = $statusCode }
            }
        }
        catch {
            $lastError = $_.Exception.Message
            if ($Process.HasExited) {
                $startupOutput = $Process.StandardOutput.ReadToEnd()
                $startupError = $Process.StandardError.ReadToEnd()
                throw "LicenseAuthority가 준비되기 전에 종료되었습니다. 종료 코드: $($Process.ExitCode), stdout=$startupOutput, stderr=$startupError"
            }
        }
    }

    throw "LicenseAuthority가 제한 시간 안에 준비되지 않았습니다. 마지막 HTTP 상태: $lastStatusCode, 마지막 오류: $lastError"
}

function Stop-LicenseAuthority {
    param([Diagnostics.Process]$Process)

    if ($null -ne $Process -and -not $Process.HasExited) {
        # Windows PowerShell 5.1에는 Kill(bool) 오버로드가 없으며 Host는 자식 프로세스를 만들지 않습니다.
        $Process.Kill()
        $Process.WaitForExit(10000) | Out-Null
    }
}

function Get-ErrorResponse {
    param([System.Management.Automation.ErrorRecord]$ErrorRecord)

    $response = $ErrorRecord.Exception.Response
    if ($null -eq $response) {
        throw $ErrorRecord
    }

    if ($null -ne $response.PSObject.Properties['Content']) {
        # 응답에 Content 속성이 있으면 PowerShell 7의 HttpResponseMessage 경로로 처리합니다.
        # 명시적인 .NET 타입 비교를 사용하지 않아 Windows PowerShell 5.1에서도 실행됩니다.
        $body = $ErrorRecord.ErrorDetails.Message
        if ([string]::IsNullOrWhiteSpace($body)) {
            try {
                $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            }
            catch {
                $body = $ErrorRecord.Exception.Message
            }
        }

        return [pscustomobject]@{
            StatusCode = [int]$response.StatusCode
            Body = $body
        }
    }

    # Windows PowerShell 5.1의 WebException 응답은 스트림으로 본문을 제공합니다.
    $reader = [IO.StreamReader]::new($response.GetResponseStream())
    try {
        return [pscustomobject]@{
            StatusCode = [int]$response.StatusCode
            Body = $reader.ReadToEnd()
        }
    }
    finally {
        $reader.Dispose()
    }
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$hostDirectory = Join-Path $repositoryRoot "src\Hosts\EDAN.Tensor.LicenseAuthority.Host\bin\Release\net10.0"
$hostDll = Join-Path $hostDirectory "EDAN.Tensor.LicenseAuthority.Host.dll"
if (-not (Test-Path -LiteralPath $hostDll)) {
    throw "LicenseAuthority Release 산출물이 없습니다. 먼저 별도 솔루션 Release Build를 실행하십시오: $hostDll"
}

foreach ($checkPort in @($Port, ($Port + 1))) {
    $listener = Get-NetTCPConnection -LocalPort $checkPort -State Listen -ErrorAction SilentlyContinue
    if ($null -ne $listener) {
        throw "LicenseAuthority Smoke 포트 $checkPort가 이미 사용 중입니다."
    }
}

$smokeRoot = Join-Path ([IO.Path]::GetTempPath()) ("edan-license-smoke-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $smokeRoot | Out-Null
$keyPath = Join-Path $smokeRoot "authority.pem"
$licensePath = Join-Path $smokeRoot "development-license.json"
$statusPath = Join-Path $smokeRoot "license-status.json"
$process = [Diagnostics.Process]::new()
$productionProcess = $null
$token = $null

try {
    $process.StartInfo = New-LicenseAuthorityStartInfo $hostDll $hostDirectory $Port $keyPath $licensePath $statusPath "Development" $true
    if (-not $process.Start()) {
        throw "LicenseAuthority 프로세스를 시작하지 못했습니다."
    }

    $health = Wait-ForLicenseAuthority $process "http://127.0.0.1:$Port/health"
    Write-Output "LicenseAuthority health: HTTP $($health.StatusCode)"

    $first = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:$Port/api/licenses/development"
    $second = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:$Port/api/licenses/development"
    if ($first.token -ne $second.token) {
        throw "개발 라이선스가 단일 발급 결과를 재사용하지 않았습니다."
    }
    $token = $first.token
    $startsAt = [DateTimeOffset]$first.descriptor.startsAtUtc
    $expiresAt = [DateTimeOffset]$first.descriptor.expiresAtUtc
    if ($expiresAt -ne $startsAt.AddYears(5)) {
        throw "개발 라이선스 만료일이 발급일 기준 5년과 일치하지 않습니다."
    }
    Write-Output "Development license issuance: single token, 5 years"

    $publicKey = Invoke-RestMethod -Method Get -Uri "http://127.0.0.1:$Port/api/licenses/public-key"
    if ([string]::IsNullOrWhiteSpace($publicKey.jwk) -or [string]::IsNullOrWhiteSpace($publicKey.keyId)) {
        throw "공개키 응답이 비어 있습니다."
    }
    Write-Output "Public JWK: HTTP 200"

    $validationBody = @{
        token = $token
        siteOrTenant = "EDAN-NONPROD"
        product = "EDAN.Tensor"
        requiredFeatures = @("Platform", "DataSet", "Worker")
    } | ConvertTo-Json
    $validation = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:$Port/api/licenses/validate" -ContentType "application/json" -Body $validationBody
    if (-not $validation.isValid) {
        throw "정상 개발 라이선스 검증이 실패했습니다."
    }
    Write-Output "License validation: HTTP 200"

    $productionProcess = [Diagnostics.Process]::new()
    $productionProcess.StartInfo = New-LicenseAuthorityStartInfo $hostDll $hostDirectory ($Port + 1) $keyPath $licensePath $statusPath "Production" $false
    if (-not $productionProcess.Start()) {
        throw "Production LicenseAuthority 프로세스를 시작하지 못했습니다."
    }
    $productionHealth = Wait-ForLicenseAuthority $productionProcess "http://127.0.0.1:$($Port + 1)/health"
    Write-Output "Production LicenseAuthority health: HTTP $($productionHealth.StatusCode)"
    try {
        Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:$($Port + 1)/api/licenses/validate" -ContentType "application/json" -Body $validationBody | Out-Null
        throw "개발 라이선스가 Production 환경에서 검증을 통과했습니다."
    }
    catch {
        $errorResponse = Get-ErrorResponse $_
        if ($errorResponse.StatusCode -ne 403 -or $errorResponse.Body -notmatch "licensing\.development_license_forbidden") {
            throw "Production 개발 라이선스 거부 응답이 올바르지 않습니다. HTTP $($errorResponse.StatusCode), Body=$($errorResponse.Body)"
        }
    }
    Write-Output "Production development license validation: HTTP 403"
    Stop-LicenseAuthority $productionProcess
    $productionProcess = $null

    Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:$Port/api/licenses/$($first.descriptor.licenseId)/revoke" | Out-Null
    try {
        Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:$Port/api/licenses/validate" -ContentType "application/json" -Body $validationBody | Out-Null
        throw "폐기된 라이선스가 검증을 통과했습니다."
    }
    catch {
        $errorResponse = Get-ErrorResponse $_
        if ($errorResponse.StatusCode -ne 403 -or $errorResponse.Body -notmatch "licensing\.revoked") {
            throw "폐기 라이선스 거부 응답이 올바르지 않습니다. HTTP $($errorResponse.StatusCode), Body=$($errorResponse.Body)"
        }
    }
    Write-Output "Revoked license validation: HTTP 403"
}
finally {
    Stop-LicenseAuthority $productionProcess
    Stop-LicenseAuthority $process
    $stdout = if ($null -ne $process) { $process.StandardOutput.ReadToEnd() } else { "" }
    $stderr = if ($null -ne $process) { $process.StandardError.ReadToEnd() } else { "" }
    if (-not [string]::IsNullOrWhiteSpace($stdout)) {
        Write-Output "LicenseAuthority stdout: $stdout"
    }
    if (-not [string]::IsNullOrWhiteSpace($stderr)) {
        Write-Output "LicenseAuthority stderr: $stderr"
    }
    if (-not [string]::IsNullOrWhiteSpace($token) -and ($stdout.Contains($token) -or $stderr.Contains($token))) {
        throw "라이선스 토큰이 Host 로그에 기록되었습니다."
    }
    Write-Output "LicenseAuthority stdout 바이트: $([Text.Encoding]::UTF8.GetByteCount($stdout))"
    Write-Output "LicenseAuthority stderr 바이트: $([Text.Encoding]::UTF8.GetByteCount($stderr))"

    if (Test-Path -LiteralPath $smokeRoot) {
        Remove-Item -LiteralPath $smokeRoot -Recurse -Force
    }
    Write-Output "임시 LicenseAuthority Smoke 디렉터리 존재 여부: $(Test-Path -LiteralPath $smokeRoot)"
}

