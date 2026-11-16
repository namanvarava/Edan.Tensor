param(
    [int]$ApiPort = 5197,
    [int]$WorkerPort = 5198
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)

function New-HostStartInfo {
    param(
        [string]$DllPath,
        [string]$WorkingDirectory,
        [int]$Port,
        [string]$DataRoot,
        [string]$KeyRoot
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
        ASPNETCORE_ENVIRONMENT = "Development"
        ASPNETCORE_URLS = "http://127.0.0.1:$Port"
        Database__ApplyMigrationsInDevelopment = "true"
        Authentication__Mode = "Bypass"
        DataProtection__DevelopmentKeyPath = $KeyRoot
        ExternalDelivery__AllowLoopback = "true"
        ConnectionStrings__PlatformDatabase = "Data Source=$(Join-Path $DataRoot 'platform.db')"
        ConnectionStrings__WorkerDatabase = "Data Source=$(Join-Path $DataRoot 'worker.db')"
        ConnectionStrings__DataSetDatabase = "Data Source=$(Join-Path $DataRoot 'dataset.db');Cache=Shared"
        ConnectionStrings__DataSetContentDatabase = "Data Source=$(Join-Path $DataRoot 'dataset-content.db');Cache=Shared"
    }

    foreach ($entry in $environment.GetEnumerator()) {
        $startInfo.Environment[$entry.Key] = $entry.Value
    }

    return $startInfo
}

function Wait-ForHttpSuccess {
    param(
        [Diagnostics.Process]$Process,
        [string]$Uri,
        [string]$Name
    )

    # 새 SQLite 데이터베이스를 생성하고 네 개의 스키마를 Migration하는 시간을 고려합니다.
    for ($attempt = 0; $attempt -lt 120; $attempt++) {
        Start-Sleep -Milliseconds 500
        try {
            $response = Invoke-WebRequest -Uri $Uri -UseBasicParsing -TimeoutSec 2
            if ($response.StatusCode -eq 200) {
                return $response
            }
        }
        catch {
            if ($Process.HasExited) {
                throw "$Name 준비 중 프로세스가 종료되었습니다. 종료 코드: $($Process.ExitCode)"
            }
        }
    }

    throw "$Name 응답이 제한 시간 안에 HTTP 200이 되지 않았습니다."
}

function Stop-SmokeProcess {
    param([Diagnostics.Process]$Process)

    if ($null -eq $Process) {
        return
    }

    try {
        $Process.Refresh()
        if ($Process.HasExited) {
            return
        }

        # Windows PowerShell 5.1에는 Kill(bool) 오버로드가 없으며 DLL Host는 자식 프로세스를 만들지 않습니다.
        $Process.Kill()
        $Process.WaitForExit(10000) | Out-Null
    }
    catch [InvalidOperationException] {
        # 상태 확인과 종료 사이에 Host가 먼저 끝났으면 정리 작업을 성공으로 처리합니다.
        return
    }
}

function Read-SmokeProcessError {
    param([Diagnostics.Process]$Process)

    if ($null -eq $Process) {
        return ""
    }

    try {
        if ($null -ne $Process.StandardError) {
            return $Process.StandardError.ReadToEnd()
        }
    }
    catch [InvalidOperationException] {
        # 프로세스가 시작되지 않았거나 표준 오류 스트림이 연결되지 않았으면 빈 로그로 처리합니다.
        return ""
    }

    return ""
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$apiDirectory = Join-Path $repositoryRoot "src\Hosts\EDAN.Tensor.ApiHost\bin\Release\net10.0"
$workerDirectory = Join-Path $repositoryRoot "src\Hosts\EDAN.Tensor.WorkerHost\bin\Release\net10.0"
$apiDll = Join-Path $apiDirectory "EDAN.Tensor.ApiHost.dll"
$workerDll = Join-Path $workerDirectory "EDAN.Tensor.WorkerHost.dll"

if (-not (Test-Path -LiteralPath $apiDll)) {
    throw "ApiHost Release 산출물이 없습니다. 먼저 Release Build를 실행하십시오: $apiDll"
}

if (-not (Test-Path -LiteralPath $workerDll)) {
    throw "WorkerHost Release 산출물이 없습니다. 먼저 Release Build를 실행하십시오: $workerDll"
}

foreach ($port in @($ApiPort, $WorkerPort)) {
    $listener = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($null -ne $listener) {
        throw "Smoke 포트 $port가 이미 사용 중입니다. 기존 프로세스를 종료한 뒤 다시 실행하십시오."
    }
}

$smokeRoot = Join-Path ([IO.Path]::GetTempPath()) ("edan-tensor-smoke-" + [Guid]::NewGuid().ToString("N"))
$dataRoot = Join-Path $smokeRoot "data"
$keyRoot = Join-Path $smokeRoot "keys"
New-Item -ItemType Directory -Path $dataRoot, $keyRoot | Out-Null

$apiProcess = [Diagnostics.Process]::new()
$workerProcess = [Diagnostics.Process]::new()
$apiProcess.StartInfo = New-HostStartInfo $apiDll $apiDirectory $ApiPort $dataRoot $keyRoot
$workerProcess.StartInfo = New-HostStartInfo $workerDll $workerDirectory $WorkerPort $dataRoot $keyRoot

try {
    if (-not $apiProcess.Start()) {
        throw "ApiHost 프로세스를 시작하지 못했습니다."
    }

    $apiHealthResponse = Wait-ForHttpSuccess $apiProcess "http://127.0.0.1:$ApiPort/health" "ApiHost health"
    Write-Output "ApiHost health: HTTP $($apiHealthResponse.StatusCode)"
    $databaseResponse = Wait-ForHttpSuccess $apiProcess "http://127.0.0.1:$ApiPort/health/database" "ApiHost database health"
    Write-Output "ApiHost database health: HTTP $($databaseResponse.StatusCode)"
    $databaseHealth = $databaseResponse.Content | ConvertFrom-Json
    if ($databaseHealth.Status -ne "Connected" -or $databaseHealth.Indicator -ne "Green") {
        throw "ApiHost database health가 Connected/Green이 아닙니다."
    }

    $openApiResponse = Wait-ForHttpSuccess $apiProcess "http://127.0.0.1:$ApiPort/openapi/v1.json" "ApiHost OpenAPI"
    Write-Output "ApiHost OpenAPI: HTTP $($openApiResponse.StatusCode)"
    $swaggerResponse = Wait-ForHttpSuccess $apiProcess "http://127.0.0.1:$ApiPort/swagger/index.html" "ApiHost Swagger UI"
    Write-Output "ApiHost Swagger UI: HTTP $($swaggerResponse.StatusCode)"
    $bypassResponse = Wait-ForHttpSuccess $apiProcess "http://127.0.0.1:$ApiPort/api/platform/tenants/mine" "Development Bypass Tenant API"
    Write-Output "Development Bypass Tenant API: HTTP $($bypassResponse.StatusCode)"

    if (-not $workerProcess.Start()) {
        throw "WorkerHost 프로세스를 시작하지 못했습니다."
    }

    $workerHealthResponse = Wait-ForHttpSuccess $workerProcess "http://127.0.0.1:$WorkerPort/health" "WorkerHost health"
    Write-Output "WorkerHost health: HTTP $($workerHealthResponse.StatusCode)"
    $workerDatabaseResponse = Wait-ForHttpSuccess $workerProcess "http://127.0.0.1:$WorkerPort/health/database" "WorkerHost database health"
    Write-Output "WorkerHost database health: HTTP $($workerDatabaseResponse.StatusCode)"
    $workerDatabaseHealth = $workerDatabaseResponse.Content | ConvertFrom-Json
    if ($workerDatabaseHealth.Status -ne "Connected" -or $workerDatabaseHealth.Indicator -ne "Green") {
        throw "WorkerHost database health가 Connected/Green이 아닙니다."
    }
    Write-Output "Host Smoke 결과: 성공"
}
finally {
    Stop-SmokeProcess $workerProcess
    Stop-SmokeProcess $apiProcess

    $apiError = Read-SmokeProcessError $apiProcess
    $workerError = Read-SmokeProcessError $workerProcess
    $apiErrorBytes = [Text.Encoding]::UTF8.GetByteCount($apiError)
    $workerErrorBytes = [Text.Encoding]::UTF8.GetByteCount($workerError)
    Write-Output "ApiHost stderr 바이트: $apiErrorBytes"
    Write-Output "WorkerHost stderr 바이트: $workerErrorBytes"
    if ($apiErrorBytes -gt 0) {
        Write-Output "ApiHost stderr 내용:"
        Write-Output $apiError
    }
    if ($workerErrorBytes -gt 0) {
        Write-Output "WorkerHost stderr 내용:"
        Write-Output $workerError
    }

    if (Test-Path -LiteralPath $smokeRoot) {
        Remove-Item -LiteralPath $smokeRoot -Recurse -Force
    }

    Write-Output "임시 Smoke 디렉터리 존재 여부: $(Test-Path -LiteralPath $smokeRoot)"
}
