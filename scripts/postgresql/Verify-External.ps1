param(
    [string]$ApiBaseUrl = "http://localhost:5000",
    [string]$WorkerBaseUrl = "http://localhost:5001",
    [string]$TenantEndpoint = "/api/platform/tenants/mine"
)

$ErrorActionPreference = "Stop"

function Test-Endpoint {
    param([string]$Name, [string]$Uri, [hashtable]$Headers = @{})

    try {
        $response = Invoke-WebRequest -UseBasicParsing -Uri $Uri -Headers $Headers -TimeoutSec 20
        [pscustomobject]@{ Name = $Name; Status = [int]$response.StatusCode; Result = "PASS" }
    }
    catch {
        $status = 0
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
        [pscustomobject]@{ Name = $Name; Status = $status; Result = "FAIL" }
    }
}

$results = @(
    Test-Endpoint "API health" "$ApiBaseUrl/health"
    Test-Endpoint "API database health" "$ApiBaseUrl/health/database"
    Test-Endpoint "Worker health" "$WorkerBaseUrl/health"
    Test-Endpoint "Worker database health" "$WorkerBaseUrl/health/database"
)

$secureToken = Read-Host "Bearer 토큰 입력(화면에 표시되지 않음)" -AsSecureString
$tokenPtr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureToken)
try {
    $token = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($tokenPtr)
    if (-not [string]::IsNullOrWhiteSpace($token)) {
        $results += Test-Endpoint "Authenticated tenant API" "$ApiBaseUrl$TenantEndpoint" @{ Authorization = "Bearer $token" }
    }
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($tokenPtr)
    Remove-Variable token, secureToken -ErrorAction SilentlyContinue
}

$results | Format-Table -AutoSize
if ($results.Result -contains "FAIL") { exit 1 }
