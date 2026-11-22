param(
    [string]$HostUrl = "http://127.0.0.1:5000",
    [string]$Server = "192.168.56.101",
    [int]$Port = 1433,
    [string]$PlatformDatabase = "edan_platform",
    [string]$WorkerDatabase = "edan_worker",
    [string]$DataSetDatabase = "edan_dataset",
    [string]$User = "sa"
)

$ErrorActionPreference = "Stop"
$secure = Read-Host "SQL Server 비밀번호" -AsSecureString
$passwordPtr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try { $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPtr) }
finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPtr) }
if ([string]::IsNullOrWhiteSpace($password)) { throw "SQL Server 비밀번호가 비어 있습니다." }

function New-SqlServerConnection([string]$database) {
    return "Server=$Server,$Port;Database=$database;User Id=$User;Password=$password;Encrypt=False;TrustServerCertificate=True"
}

$project = Join-Path $PSScriptRoot "../../src/Hosts/EDAN.Tensor.ApiHost/EDAN.Tensor.ApiHost.csproj"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Authentication__Mode = "Bypass"
$env:Database__Provider = "SqlServer"
$env:Database__ApplyMigrationsInDevelopment = "false"
$env:ConnectionStrings__PlatformDatabase = New-SqlServerConnection $PlatformDatabase
$env:ConnectionStrings__WorkerDatabase = New-SqlServerConnection $WorkerDatabase
$env:ConnectionStrings__DataSetDatabase = New-SqlServerConnection $DataSetDatabase
$env:ConnectionStrings__DataSetContentDatabase = "Data Source=App_Data/edan-tensor-dataset-content.db;Cache=Shared"

try {
    dotnet run --project $project --configuration Release --no-build --urls $HostUrl
    if ($LASTEXITCODE -ne 0) { throw "ApiHost 실행에 실패했습니다. 종료 코드: $LASTEXITCODE" }
}
finally {
    Remove-Item Env:ASPNETCORE_ENVIRONMENT, Env:Authentication__Mode, Env:Database__Provider, Env:Database__ApplyMigrationsInDevelopment, Env:ConnectionStrings__PlatformDatabase, Env:ConnectionStrings__WorkerDatabase, Env:ConnectionStrings__DataSetDatabase, Env:ConnectionStrings__DataSetContentDatabase -ErrorAction SilentlyContinue
    Remove-Variable password -ErrorAction SilentlyContinue
}
