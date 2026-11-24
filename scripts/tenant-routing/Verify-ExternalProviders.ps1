param(
    [string]$ApiBaseUrl = "http://localhost:5000",
    [string]$WorkerBaseUrl = "http://localhost:5001",
    [Parameter(Mandatory = $true)][Guid]$TenantId,
    [Parameter(Mandatory = $true)][string]$Schema,
    [string]$ProviderHost = "localhost",
    [int[]]$ProviderPorts = @(5432, 1433, 1521),
    [string]$TokenFile,
    [ValidateSet('Procedure','Function','OraclePackageProcedure')][string]$RoutineKind,
    [string]$RoutineName,
    [string]$RoutineParametersJson = '{}',
    [string]$ConsistencyChecksJson
)

$ErrorActionPreference = "Stop"

function Invoke-CheckedEndpoint {
    param([string]$Name, [string]$Uri, [hashtable]$Headers = @{}, [int]$ExpectedStatus = 200, [ValidateSet('GET','POST')][string]$Method = 'GET')
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Uri $Uri -Method $Method -Headers $Headers -TimeoutSec 20
        if ([int]$response.StatusCode -ne $ExpectedStatus) { throw "HTTP $([int]$response.StatusCode), expected $ExpectedStatus" }
        [pscustomobject]@{ Check = $Name; Result = "PASS"; Detail = "HTTP $([int]$response.StatusCode)" }
    }
    catch {
        [pscustomobject]@{ Check = $Name; Result = "FAIL"; Detail = $_.Exception.Message }
    }
}

function Invoke-CheckedJsonEndpoint {
    param([string]$Name, [string]$Uri, [hashtable]$Headers, [object]$Body)
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Uri $Uri -Method POST -Headers $Headers -ContentType 'application/json' -Body ($Body | ConvertTo-Json -Depth 10) -TimeoutSec 20
        if ([int]$response.StatusCode -lt 200 -or [int]$response.StatusCode -ge 300) { throw "HTTP $([int]$response.StatusCode)" }
        [pscustomobject]@{ Check = $Name; Result = "PASS"; Detail = "HTTP $([int]$response.StatusCode)" }
    }
    catch {
        [pscustomobject]@{ Check = $Name; Result = "FAIL"; Detail = $_.Exception.Message }
    }
}

function Test-TcpPort {
    param([string]$HostName, [int]$Port)
    $client = [System.Net.Sockets.TcpClient]::new()
    try {
        $task = $client.ConnectAsync($HostName, $Port)
        if (-not $task.Wait(5000) -or -not $client.Connected) { throw "TCP 연결 시간 초과 또는 거부" }
        [pscustomobject]@{ Check = "TCP $HostName`:$Port"; Result = "PASS"; Detail = "연결 가능" }
    }
    catch {
        [pscustomobject]@{ Check = "TCP $HostName`:$Port"; Result = "FAIL"; Detail = $_.Exception.Message }
    }
    finally { $client.Dispose() }
}

$secureToken = $null
$tokenPtr = [IntPtr]::Zero
try {
    if ([string]::IsNullOrWhiteSpace($TokenFile)) {
        $secureToken = Read-Host "Bearer 토큰 입력(화면에 표시되지 않음)" -AsSecureString
        $tokenPtr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureToken)
        $token = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($tokenPtr)
    }
    else {
        if (-not (Test-Path -LiteralPath $TokenFile)) { throw "토큰 파일을 찾을 수 없습니다: $TokenFile" }
        $token = ((Get-Content -LiteralPath $TokenFile -Raw | ConvertFrom-Json).access_token).ToString()
    }
    if ([string]::IsNullOrWhiteSpace($token)) { throw "Bearer 토큰이 필요합니다." }
    $headers = @{
        Authorization = "Bearer $token"
        'X-Tenant-Id' = $TenantId.ToString()
    }
    $results = @(
        (Test-TcpPort $ProviderHost $ProviderPorts[0])
        (Test-TcpPort $ProviderHost $ProviderPorts[1])
        (Test-TcpPort $ProviderHost $ProviderPorts[2])
        (Invoke-CheckedEndpoint "API health" "$ApiBaseUrl/health")
        (Invoke-CheckedEndpoint "API database health" "$ApiBaseUrl/health/database")
        (Invoke-CheckedEndpoint "Worker health" "$WorkerBaseUrl/health")
        (Invoke-CheckedEndpoint "Worker database health" "$WorkerBaseUrl/health/database")
        (Invoke-CheckedEndpoint "Tenant DB health" "$ApiBaseUrl/api/platform/tenants/$TenantId/database-binding/health" $headers 200 'POST')
        (Invoke-CheckedEndpoint "Table metadata" "$ApiBaseUrl/api/platform/tenants/$TenantId/database/metadata/tables" $headers)
        (Invoke-CheckedEndpoint "Routine metadata" "$ApiBaseUrl/api/platform/tenants/$TenantId/database/metadata/routines?schema=$([uri]::EscapeDataString($Schema))" $headers)
    )
    if (-not [string]::IsNullOrWhiteSpace($RoutineKind) -or -not [string]::IsNullOrWhiteSpace($RoutineName)) {
        if ([string]::IsNullOrWhiteSpace($RoutineKind) -or [string]::IsNullOrWhiteSpace($RoutineName)) { throw "RoutineKind와 RoutineName은 함께 지정해야 합니다." }
        try { $routineParameters = $RoutineParametersJson | ConvertFrom-Json -AsHashtable } catch { throw "RoutineParametersJson이 올바른 JSON 객체가 아닙니다." }
        $routineBody = [ordered]@{ Kind = $RoutineKind; SchemaName = $Schema; RoutineName = $RoutineName; Parameters = $routineParameters }
        $results += Invoke-CheckedJsonEndpoint "Routine execution" "$ApiBaseUrl/api/platform/tenants/$TenantId/database/routines/execute" $headers $routineBody
    }
    if (-not [string]::IsNullOrWhiteSpace($ConsistencyChecksJson)) {
        try { $consistencyChecks = $ConsistencyChecksJson | ConvertFrom-Json } catch { throw "ConsistencyChecksJson이 올바른 JSON 배열이 아닙니다." }
        if ($consistencyChecks -isnot [System.Collections.IEnumerable] -or $consistencyChecks -is [string]) { throw "ConsistencyChecksJson은 JSON 배열이어야 합니다." }
        $consistencyBody = [ordered]@{ Checks = @($consistencyChecks) }
        $results += Invoke-CheckedJsonEndpoint "Provider consistency" "$ApiBaseUrl/api/platform/tenants/$TenantId/database/consistency/verify" $headers $consistencyBody
    }
    $results | Format-Table -AutoSize
    if ($results.Result -contains "FAIL") { exit 1 }
}
finally {
    if ($tokenPtr -ne [IntPtr]::Zero) { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($tokenPtr) }
    Remove-Variable token, secureToken -ErrorAction SilentlyContinue
}
